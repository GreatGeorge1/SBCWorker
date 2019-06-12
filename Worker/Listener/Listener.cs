using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Worker.EntityFrameworkCore;
using Worker.Models;

namespace Worker.Host
{
    public partial class Listener 
    {
        private readonly ILogger logger;
        private readonly ControllerDbContext context;
        private readonly ListenerPort port;
        private SerialPort stream;
        private ExecutedMethod executedMethod { get; set; }

        public Listener(ILogger logger, ListenerPort port,
           ControllerDbContext dbcontext)
        {
            this.logger = logger;
            this.port = port;
            this.context = dbcontext;
        }

        public async Task ListenAsync(CancellationToken stoppingToken)
        {
            if (!PortHelpers.PortNameExists(port.PortName))
            {
                throw new Exception($"{port.PortName} not exists");
            }
            stream = new SerialPort(port.PortName);
            stream.BaudRate = 115200;
            stream.ReadTimeout = 500;
            stream.WriteTimeout = 500;
            stream.DataReceived += DataReceivedAction;
            stream.ErrorReceived += ErrorReceivedAction;
            stream.PinChanged += PinChangedAction;
            stream.Parity = Parity.None;
            stream.StopBits = StopBits.One;
            stream.DataBits = 8;

            if (port.IsRS485)
            {
                stream.Handshake = Handshake.None;
                stream.RtsEnable = true;
                logger.LogInformation($"Port {port.PortName} in RS485 mode");
            }
        
            stream.Open();
            if (!stream.IsOpen)
            {
                logger.LogCritical($"Error opening serial port {port.PortName}");
                throw new Exception($"Error opening serial port {port.PortName}");
            }
            logger.LogInformation($"Port {port.PortName} open");
            if (stream == null)
            {
                logger.LogCritical($"No serial port {port.PortName}");
                throw new Exception($"No serial port {port.PortName}");
            }
            if (stream.CtsHolding)
            {
                logger.LogInformation($"Cts detected {port.PortName}");
            }
            else
            {
                logger.LogInformation($"Cts NOT detected {port.PortName}");
            }
            logger.LogInformation($"Port listener started: {port.PortName}");
        }

        private void PinChangedAction(object sender, SerialPinChangedEventArgs e)
        {
            logger.LogInformation($"Port {port.PortName} pin changed: {e.ToString()}");
        }

        private void ErrorReceivedAction(object sender, SerialErrorReceivedEventArgs e)
        {
            logger.LogInformation($"Port {port.PortName} erorr: {e.ToString()}");
        }

        private async void DataReceivedAction(object sender, SerialDataReceivedEventArgs e)
        {
            logger.LogInformation("DataReceived Action raised");
            var message = await ReadMessage();
            ProtocolCommands command=ProtocolCommands.NotSet;

            if (message.Equals("COMPLETED"))
            {
                logger.LogWarning("COMPLETED");
                executedMethod.IsCompleted = true;
                return;
            }

            try
            {
                command = await ReadCommand(message);

            } catch(CommandHeaderNotFoundException ex)
            {
                logger.LogInformation(ex.Message);
            }

            if (command == ProtocolCommands.NotSet)
            {
                if (executedMethod != null)
                {
                    if (!executedMethod.MethodInfo.IsControllerHosted)
                    {
                        if (executedMethod.MethodInfo.HasCommandValue && string.IsNullOrWhiteSpace(executedMethod.CommandValue))
                        {
                            executedMethod.CommandValue = message;
                        }else if(executedMethod.MethodInfo.IsHashable && string.IsNullOrWhiteSpace(executedMethod.Hash))
                        {
                            executedMethod.Hash = message;
                        }
                    }
                    else
                    {
                        if(executedMethod.MethodInfo.HasResponseHeader && executedMethod.ResponseHeader == ProtocolResponse.NotSet)
                        {
                            try
                            {
                                var responseHeader = await ReadResponse(message);
                            }catch(ResponseHeaderNotFoundException ex)
                            {
       
                                logger.LogWarning($"Flow warning, recieved message : {message}");
                                executedMethod = null;
                                logger.LogWarning("Executed method set to NULL");
                            }
                        }else if(executedMethod.MethodInfo.HasResponseValue && string.IsNullOrWhiteSpace(executedMethod.ResponseValue))
                        {
                            executedMethod.ResponseValue = message;
                        }
                    }
                }
                else
                {
                    logger.LogWarning($"Flow warning, recieved message : {message}");
                }
            }
            else
            {
                if (executedMethod != null)
                {
                    if (executedMethod.IsCompleted)
                    {
                        await ExecuteMethod(command);
                    }
                    else
                    {
                        logger.LogWarning($"Interrupted method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
                     //   executedMethod.IsCompleted = true;//fix response
                        await ExecuteMethod(command);
                    }
                }
                else
                {
                    await ExecuteMethod(command);
                }
            }
        }

        private async Task ExecuteMethod(ProtocolCommands command)
        {
            ProtocolMethod methodInfo;
            Protocol.Methods.TryGetValue(command, out methodInfo);
            executedMethod = null;
            executedMethod = new ExecutedMethod { MethodInfo = methodInfo, IsCompleted = false, IsFired = false, ResponseHeader=ProtocolResponse.NotSet };
            executedMethod.PropertyChanged += OnExecuteMethodChange;
            logger.LogWarning($"Executed method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
        }

        private async void OnExecuteMethodChange(object sender, PropertyChangedEventArgs args)
        {
            logger.LogInformation($"Executed method property changed: {args.PropertyName}");
            if (!executedMethod.IsFired)
            {
                if (!executedMethod.MethodInfo.IsControllerHosted)
                {
                    if (Protocol.CheckReadyTerminalHosted(executedMethod))
                    {
                        executedMethod.IsFired = true;
                        await ProcessTerminalHostedMethod();
                    }
                }
                else
                {
                    if (Protocol.CheckReadyControllerHosted(executedMethod))
                    {
                        executedMethod.IsFired = true;
                        await ProcessControllerHostedMethod();
                    }
                }
            }
        }

        private async Task ProcessTerminalHostedMethod()
        {
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case ProtocolCommands.Card:
                    var res = await VerifyCard(executedMethod.CommandValue, executedMethod.Hash);
                    if (res)
                    {
                        executedMethod.ResponseHeader = ProtocolResponse.CardOk;
                        var response = executedMethod.CreateResponse();
                        while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == ProtocolCommands.Card)
                        {
                            await WriteMessage(response);
                            await Task.Delay(200);
                        }
                    }
                    else
                    {
                        executedMethod.ResponseHeader = ProtocolResponse.CardError;
                        var response = executedMethod.CreateResponse();
                        while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader==ProtocolCommands.Card)
                        {
                            await WriteMessage(response);
                            await Task.Delay(200);
                        }
                    }
                   // executedMethod.IsCompleted = true;
                    break;
                case ProtocolCommands.Finger:
                    var finger = await VerifyFinger(executedMethod.CommandValue, executedMethod.Hash);
                    if (finger)
                    {
                        executedMethod.ResponseHeader = ProtocolResponse.FingerOk;
                        var response = executedMethod.CreateResponse();
                        while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == ProtocolCommands.Finger)
                        {
                            await WriteMessage(response);
                            await Task.Delay(200);
                        }
                    }
                    else
                    {
                        executedMethod.ResponseHeader = ProtocolResponse.FingerError;
                        var response = executedMethod.CreateResponse();
                        while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == ProtocolCommands.Finger)
                        {
                            await WriteMessage(response);
                            await Task.Delay(200);
                        }
                    }
                    //executedMethod.IsCompleted = true;
                    break;
                //case ProtocolCommands.
            }
        }

        private async Task ProcessControllerHostedMethod()
        {
            //switch (executedMethod.MethodInfo.CommandHeader)
            //{
            //    case ProtocolCommands.Card:
            //        break;
            //}
        }


        private async Task<ProtocolCommands> ReadCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                logger.LogInformation($"Command is null {command}");
                return ProtocolCommands.NotSet;
            }
            logger.LogInformation($"Command is {command}");

            var res = Protocol.GetCommandHeader(command);
            return res;
        }

        private async Task<ProtocolResponse> ReadResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                logger.LogInformation($"Command is null {response}");
                return ProtocolResponse.NotSet;
            }
            logger.LogInformation($"Message is {response}");

            var res = Protocol.GetResponseHeader(response);
            return res;
        }

        public async Task<string> ReadMessage()
        {         
            try
            {
               // logger.LogInformation($"port :{portName}, reading: {stream.ReadExisting()}");
                string res = stream.ReadLine();
                var snew = new string(res.Where(c => !char.IsControl(c)).ToArray());
                logger.LogInformation($"port :{port.PortName}, readed: {res}, filtered:{snew}");
                return snew;
            }
            catch (IOException ex)
            {
                logger.LogWarning(ex.ToString());
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning("r:timeout");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex.ToString());
            }
            
            return null;
        }

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _=ListenAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}

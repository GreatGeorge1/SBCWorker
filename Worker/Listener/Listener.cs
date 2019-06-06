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

namespace Worker.Host
{
    public partial class Listener 
    {
        private readonly ILogger logger;
        private readonly ControllerDbContext context;
        private readonly string portName;
        private SerialPort stream;
        private ExecutedMethod executedMethod { get; set; }

        public Listener(ILogger logger, string portName,
           ControllerDbContext dbcontext)
        {
            this.logger = logger;
            this.portName = portName;
            this.context = dbcontext;
        }

        public async Task ListenAsync(CancellationToken stoppingToken)
        {
            if (!PortHelpers.PortNameExists(portName))
            {
                throw new Exception($"{portName} not exists");
            }
            stream = new SerialPort(portName);
            stream.BaudRate = 115200;
            stream.ReadTimeout = 500;
            stream.WriteTimeout = 500;
           // stream.Encoding = Encoding.UTF8;
            stream.RtsEnable = true;
            stream.DtrEnable = true;
            // stream.Handshake = Handshake.RequestToSendXOnXOff;
            stream.DataReceived += DataReceivedAction;
            stream.ErrorReceived += ErrorReceivedAction;
            stream.PinChanged += PinChangedAction;
          //  stream.NewLine = "\r\n";
            // stream.NewLine = 0x30.ToString();
            // stream.Handshake = Handshake.XOnXOff;
            stream.Parity = Parity.None;
            stream.StopBits = StopBits.One;
            stream.DataBits = 8;
            //stream.DiscardNull = true;
            stream.WriteBufferSize = 1024;
         //  stream.Handshake = Handshak;
            stream.Open();
            if (!stream.IsOpen)
            {
                logger.LogCritical($"Error opening serial port {portName}");
                throw new Exception($"Error opening serial port {portName}");
            }
            logger.LogInformation($"Port {portName} open");
            if (stream == null)
            {
                logger.LogCritical($"No serial port {portName}");
                throw new Exception($"No serial port {portName}");
            }
            if (stream.CtsHolding)
            {
                logger.LogInformation($"Cts detected {portName}");
            }
            else
            {
                logger.LogInformation($"Cts NOT detected {portName}");
            }
            logger.LogInformation($"Port listener started: {portName}");
        }

        private void PinChangedAction(object sender, SerialPinChangedEventArgs e)
        {
            logger.LogInformation($"Port {portName} pin changed: {e.ToString()}");
        }

        private void ErrorReceivedAction(object sender, SerialErrorReceivedEventArgs e)
        {
            logger.LogInformation($"Port {portName} erorr: {e.ToString()}");
        }

        private async void DataReceivedAction(object sender, SerialDataReceivedEventArgs e)
        {
            logger.LogInformation("DataReceived Action raised");
            var message = await ReadMessage();
            ProtocolCommands command=ProtocolCommands.NotSet;
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
                        var resposne = Protocol.CreateResponse(ProtocolResponse.CardOk);
                        await WriteMessage(resposne);
                    }
                    else
                    {
                        var resposne = Protocol.CreateResponse(ProtocolResponse.CardError);
                        await WriteMessage(resposne);
                    }
                    executedMethod.IsCompleted = true;
                    break;
                case ProtocolCommands.Finger:break;
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
            logger.LogInformation($"Command is {response}");

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
                logger.LogInformation($"port :{portName}, readed: {res}, filtered:{snew}");
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

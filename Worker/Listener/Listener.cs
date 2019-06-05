using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    public class Listener 
    {
        private readonly ILogger logger;
        private readonly ControllerDbContext context;
        private readonly string portName;
        private SerialPort stream;

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
            stream.ReadTimeout = 2000;
            stream.WriteTimeout = 2000;
            stream.Encoding = Encoding.UTF8;
            stream.RtsEnable = true;
            stream.DtrEnable = true;
            // stream.Handshake = Handshake.RequestToSendXOnXOff;
            stream.DataReceived += DataReceivedAction;
            stream.ErrorReceived += ErrorReceivedAction;
            stream.PinChanged += PinChangedAction;
            stream.NewLine = "\r\n";
            // stream.NewLine = 0x30.ToString();
            // stream.Handshake = Handshake.XOnXOff;
            stream.Parity = Parity.Even;
            stream.StopBits = StopBits.One;
            stream.DataBits = 8;
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
            var command = await ReadCommand();
            //switch (ListenerState)
            //{
            //    case State.Ready:
            //        ReadCommand();
            //        break;
            //    case State.Card:
            //        VerifyCard();
            //        break;
            //    case State.CardAuthorized:
            //        ReadCommand();
            //        break;
            //    case State.Finger:
            //        VerifyFinger();
            //        break;
            //    case State.FingerAuthorized:
            //        ReadCommand();
            //        break;
            //    case State.Error:
            //        ProceedError();
            //        break;
            //    default:
            //        ReadMessage();
            //        break;
            //};
        }

        private async Task<ProtocolCommands> ReadCommand()
        {
            string command = await ReadMessage();
            var snew = new string(command.Where(c => !char.IsControl(c)).ToArray());

            if (string.IsNullOrEmpty(snew))
            {
                logger.LogInformation($"Command is null {snew}");
            }
            logger.LogInformation($"Command is {snew}");

            var res = Protocol.GetCommand(command);
            return res;
        }

        public async Task<string> ReadMessage()
        {
           // if (!stop)
            {
                try
                {
                    //logger.LogInformation($"port :{portName}, reading: {stream.ReadExisting()}");
                    string res = stream.ReadLine();
                    logger.LogInformation($"port :{portName}, readed: {res}");
                    return res;
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.Host.Transports
{
    public class SerialPortTransport : ITransport
    {
        private readonly SerialConfig port;
        private SerialPort stream;
        private ILogger logger { get; set; }
        public MessageQueue<string> InputQueue { get; set; }

        public SerialPortTransport(SerialConfig port, ILogger logger = null)
        {
            InputQueue = new MessageQueue<string>();
            this.port = port ?? throw new ArgumentNullException(nameof(port));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (logger != null)
            {
                this.logger = logger;
            }
            else
            {
                this.logger = new NullLogger<SerialPortTransport>();
            }
        }

        public bool Init()
        {
            if (!PortHelpers.PortNameExists(port.PortName))
            {
                logger.LogCritical($"{port.PortName} not exists");
                //throw new Exception($"{port.PortName} not exists");
                return false;
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
                //throw new Exception($"Error opening serial port {port.PortName}");
                return false;
            }
            logger.LogInformation($"Port {port.PortName} open");
            if (stream == null)
            {
                logger.LogCritical($"No serial port {port.PortName}");
                // throw new Exception($"No serial port {port.PortName}");
                return false;
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
            return true;
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
            await ReadMessageAsync();
        }
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
        /// <returns></returns>
        public Task ReadMessageAsync()
        {
            try
            {
                do
                {
                    string res = stream.ReadLine();
                    var snew = new string(res.Where(c => !char.IsControl(c)).ToArray());
                    logger.LogInformation($"port :{port.PortName}, readed: {res}, filtered:{snew}");
                    InputQueue.Enqueue(snew);
                } while (stream.RtsEnable == true);
            }
            catch (IOException ex)
            {
                logger.LogWarning(ex.ToString());
            }
            catch (System.TimeoutException ex)
            {
                logger.LogWarning("r:timeout");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex.ToString());
            }
            return Task.CompletedTask;
        }

        public async Task<bool> WriteMessageAsync(string message)
        {
            if (stream.IsOpen)
            {
                try
                {
                    if (port.IsRS485)
                    {
                        stream.RtsEnable = false;
                        await Task.Delay(50);
                    }
                    stream.Write(message);
                    if (port.IsRS485)
                    {
                        await Task.Delay(50);
                        stream.RtsEnable = true;
                    }
                    var bytes = stream.BytesToWrite;
                    var size = stream.WriteBufferSize;
                    logger.LogInformation($"wrote to port {port.PortName}: {message}, bytes {bytes}, buff_size {size}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"ex {port.PortName}");
                    logger.LogWarning(ex.ToString());
                    if (port.IsRS485)
                    {
                        stream.RtsEnable = true;
                    }
                    //throw new Exception(ex.Message);
                    return false;
                }
            }
            else
            {
                logger.LogCritical($"Cannot write to port: {port.PortName}, port closed");
                return false;
            }
            return true;
        }
    }
}

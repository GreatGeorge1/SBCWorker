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
    public class SerialPortTransport : IByteTransport
    {
        private readonly SerialConfig port;
        private SerialPort stream;
        private ILogger logger { get; set; }
        public MessageQueue<byte[]> InputQueue { get; set; }
        public MessageQueue<byte[]> OutputQueue { get; set; }

        private bool serial_read = false;
        public SerialPortTransport(SerialConfig port, ILogger logger = null)
        {
            InputQueue = new MessageQueue<byte[]>();
            OutputQueue=new MessageQueue<byte[]>();
            OutputQueue.EnqueueEvent += EnqueueOutputMessageAction;
            this.port = port ?? throw new ArgumentNullException(nameof(port));
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
            stream.Encoding = Encoding.UTF8;
            stream.NewLine = "\r\n";

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

        private async void EnqueueOutputMessageAction(object sender, MessageQueueEnqueueEventArgs<byte[]> args)
        {
            var msg=OutputQueue.Dequeue();
            await WriteMessageAsync(msg);
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
        public async Task ReadMessageAsync()
        {
            try
            {
                if (serial_read)
                {
                    return;
                }
                serial_read = true;
                do
                {
                    await Task.Delay(100);
                    var bytes = new List<byte>();
                    byte r = 0x0d; //'\r'
                    byte n = 0x0a; //'\n'
                    byte temp;
                    do
                    {
                        var _byte = stream.ReadByte();
                        temp = (byte) _byte;
                        if (temp != r && temp != n)
                        {
                            bytes.Add((byte) _byte);
                        }
                    } while (stream.BytesToRead > 0 && !(temp == r || temp == n));
                    var res = bytes.ToArray();
                    string str = BitConverter.ToString(res);
                    Console.WriteLine(str);
                    InputQueue.Enqueue(res);
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
            serial_read = false;
            return;
        }

        public async Task<bool> WriteMessageAsync(byte[] message)
        {
            if (stream.IsOpen)
            {
                try
                {
                    while (serial_read)
                    {
                        await Task.Delay(50);
                    }
                    if (port.IsRS485)
                    {
                        stream.RtsEnable = false;
                        await Task.Delay(50);
                    }
                   // stream.Write(message);
                    stream.Write(message, 0, message.Length);
                   
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

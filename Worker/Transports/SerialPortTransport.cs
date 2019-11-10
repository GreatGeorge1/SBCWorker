using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.Host.Transports
{
    public class SerialPortTransport : BackgroundService,IByteTransport
    {
        private readonly SerialConfig port;
        private SerialPort stream;

        private ILogger Logger { get; set; }
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
                this.Logger = logger;
            }
            else
            {
                this.Logger = new NullLogger<SerialPortTransport>();
            }
        }

        public bool Init()
        {
            if (!PortHelpers.PortNameExists(port.PortName))
            {
                Logger.LogCritical($"{port.PortName} not exists");
                //throw new Exception($"{port.PortName} not exists");
                return false;
            }
            stream = new SerialPort(port.PortName)
            {
                BaudRate = 115200,
                ReadTimeout = 500,
                WriteTimeout = 500,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Encoding = Encoding.UTF8,
                NewLine = "\r\n"
            };
            stream.DataReceived += DataReceivedAction;
            stream.ErrorReceived += ErrorReceivedAction;
            stream.PinChanged += PinChangedAction;
           

            if (port.IsRS485)
            {
                stream.Handshake = Handshake.None;
                stream.RtsEnable = true;
                Logger.LogInformation($"Port {port.PortName} in RS485 mode");
            }

            stream.Open();
            if (!stream.IsOpen)
            {
                Logger.LogCritical($"Error opening serial port {port.PortName}");
                //throw new Exception($"Error opening serial port {port.PortName}");
                return false;
            }
            Logger.LogInformation($"Port {port.PortName} open");
            if (stream == null)
            {
                Logger.LogCritical($"No serial port {port.PortName}");
                // throw new Exception($"No serial port {port.PortName}");
                return false;
            }
            if (stream.CtsHolding)
            {
                Logger.LogInformation($"Cts detected {port.PortName}");
            }
            else
            {
                Logger.LogInformation($"Cts NOT detected {port.PortName}");
            }
            Logger.LogInformation($"Port listener started: {port.PortName}");
            return true;
        }

        private async void EnqueueOutputMessageAction(object sender, MessageQueueEnqueueEventArgs<byte[]> args)
        {
            var msg=OutputQueue.Dequeue();
            await WriteMessageAsync(msg).ConfigureAwait(false);
        }
        private void PinChangedAction(object sender, SerialPinChangedEventArgs e)
        {
            Logger.LogInformation($"Port {port.PortName} pin changed: {e.ToString()}");
        }

        private void ErrorReceivedAction(object sender, SerialErrorReceivedEventArgs e)
        {
            Logger.LogInformation($"Port {port.PortName} erorr: {e.ToString()}");
        }

        private void DataReceivedAction(object sender, SerialDataReceivedEventArgs e)
        {
            Logger.LogInformation("DataReceived Action raised");
            ReadMessage();
        }
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
        /// <returns></returns>
        public void ReadMessage()
        {
            if (serial_read)
            {
                return;
            }
            serial_read = true;
            Stopwatch sw = new Stopwatch();
            try
            {
                do
                {
                    sw.Start();
                    var list = new List<byte>();
                    var bytes = new List<byte>();
                    do
                    {
                        if (sw.ElapsedMilliseconds >= 2000)
                        {
                            Logger.LogWarning("ReadTimeout");
                            serial_read = false;
                            return;
                        }
                        byte _byte = 0;
                        try
                        {
                            _byte = (byte)stream.ReadByte();
                        }
                        catch(InvalidOperationException)
                        {
                            continue;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        list.Add(_byte);
                    } while (stream.BytesToRead > 0 && list.Count < 5);
                    var len = list.LastOrDefault();
                    if (len == 0)
                    {
                        Logger.LogWarning("corrupted message");
                        serial_read = false;
                        return;
                    }
                    int k = len;
                    bytes.AddRange(list);
                    sw.Reset();
                    do
                    {
                        if (sw.ElapsedMilliseconds >= 2000)
                        {
                            Logger.LogWarning("ReadTimeout");
                            serial_read = false;
                            return;
                        }
                        byte _byte = 0;
                        try
                        {
                            _byte = (byte)stream.ReadByte();
                        }
                        catch
                        {
                            continue;
                        }
                        bytes.Add(_byte);
                        k--;
                    } while (stream.BytesToRead > 0 && k > 0);

                    var res = bytes.ToArray();
                    string str = BitConverter.ToString(res);
                    Console.WriteLine(str);
                    InputQueue.Enqueue(res);
                } while (stream.RtsEnable == true);
            }
            catch (IOException ex)
            {
                Logger.LogWarning(ex.ToString());
            }
            catch (System.TimeoutException ex)
            {
                Logger.LogWarning("r:timeout");
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogWarning(ex.ToString());
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
                        await Task.Delay(50).ConfigureAwait(false);
                    }
                    if (port.IsRS485)
                    {
                        stream.RtsEnable = false;
                        await Task.Delay(50).ConfigureAwait(false);
                    }
                   // stream.Write(message);
                    stream.Write(message, 0, message.Length);
                   
                    if (port.IsRS485)
                    {
                        await Task.Delay(50).ConfigureAwait(false);
                        stream.RtsEnable = true;
                    }
                    var bytes = stream.BytesToWrite;
                    var size = stream.WriteBufferSize;
                    Logger.LogInformation($"wrote to port {port.PortName}: {message}, bytes {bytes}, buff_size {size}");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"ex {port.PortName}");
                    Logger.LogWarning(ex.ToString());
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
                Logger.LogCritical($"Cannot write to port: {port.PortName}, port closed");
                return false;
            }
            return true;
        }

        public string GetInfo()
        {
            return $"port:'{port.PortName}' isRs485:'{port.IsRS485.ToString()}'";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                this.Init();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}

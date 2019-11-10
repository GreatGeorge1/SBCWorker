using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Protocol;
using Worker.EntityFrameworkCore;
using Worker.Host.SignalR;
using Worker.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace Worker.Host
{
    public class ListenerHost : BackgroundService
    {
        private readonly ILogger<ListenerHost> _logger;
        private readonly IEnumerable<SerialConfig> _ports;
        private readonly ListenerFactory _factory;
        private readonly InputMessageQueue inputQueue;
        private readonly OutputMessageQueue outputQueue;
        private readonly List<Listener<byte[]>> listeners = new List<Listener<byte[]>>();

        public ListenerHost(ILogger<ListenerHost> logger, 
            IEnumerable<SerialConfig> ports,
            ListenerFactory factory, 
            InputMessageQueue inputQueue,
            OutputMessageQueue outputQueue)
        {
            _logger = logger;
            _ports = ports;
            _factory = factory;
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach(var port in _ports)
            {
                try
                {
                    Console.WriteLine($"PORT: {port.PortName}");
                    var res1 = inputQueue.Dictionary.TryGetValue(port.PortName, out ConcurrentMessageBag<SignalRMessage> tempQueue);

                    var res2 = outputQueue.Dictionary.TryGetValue(port.PortName, out ConcurrentMessageBag<SignalRresponse> tempOutQueue);

                    if (!res1 || !res2)
                    {
                        throw new NullReferenceException(nameof(IMessageQueue));
                    }
                    var listener = _factory.NewListener(port, tempQueue, tempOutQueue);
                    listeners.Add(listener);
                }
                catch
                {
                    Console.WriteLine($"PORT: {port.PortName} init ERROR");
                }
                
            }
           
        }

    }

    public class SignalRMessage
    {
        public string Port { get; set; }
        public SignalRMethod Method { get; set; }
        public string UserId { get; set; }
        public int Uid { get; set; }
        public string BleString { get; set; }
        public int Privilage { get; set; }
        public string JsonString { get; set; }
        public int Timeout { get; set; }
        public string Address { get; set; }
    }

    public class SignalRresponse
    {
        public string JsonString { get; set; }
        public SignalRMethod Method { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }

    public enum SignalRMethod
    {
        GetFingerTimeoutCurrent,
        AddFinger,
        SendConfig,
        DeleteFingerById,
        AddFingerByBle,
        DeleteAllFingerprints,
        SetFingerTimeout,
        GetConfig
    }


    #region client requests

    public class GetConfigRes
    {
        public string JsonString { get; set; }
        public string Port { get; set; }
    }

    public class GetConfigReq
    {
        public string Address { get; set; }
        public string Port { get; set; }
    }
    public class SetFingerTimeoutReq
    {
        public int Timeout { get; set; }
        public string Port { get; set; }
    }

    public class GetFingerTimeoutReq
    {
        public string Port { get; set; }
    }

    public class AddFingerReq
    {
        public int Uid { get; set; }
        public int Privilage { get; set; }
        public string Port { get; set; }
    }

    public class SendConfigReq
    {
        public string JsonString { get; set; }
        public string Port { get; set; }
    }

    public class DeleteAllFingerprintsReq
    {
        public string Port { get; set; }
    }

    public class DeleteFingerByIdReq
    {
        public int Id { get; set; }
        public string Port { get; set; }
    }

    public class AddFingerByBleReq
    {
        public string UserId { get; set; }
        public string Ble { get; set; }
        public int Id { get; set; }
        public int Privilage { get; set; }
        public string Port { get; set; }
    }

    #endregion
}

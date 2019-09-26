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

namespace Worker.Host
{
    public class ListenerHost : BackgroundService
    {
        private readonly ILogger<ListenerHost> _logger;
        private readonly IEnumerable<SerialConfig> _ports;
        //private readonly IOptions<WorkerOptions> _options;
        private readonly ListenerFactory _factory;
        private readonly MessageQueue<SignalRMessage> inputQueue;
        private readonly MessageQueue<SignalRMessage> outputQueue;
        private readonly ServerSignalRClient client;

        public ListenerHost(ILogger<ListenerHost> logger, IEnumerable<SerialConfig> ports,ListenerFactory factory, ServerSignalRClient client)
        {
            _logger = logger;
            _ports = ports;
            _factory = factory;
            this.client = client;
            inputQueue = new MessageQueue<SignalRMessage>();
            outputQueue = new MessageQueue<SignalRMessage>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach(var port in _ports)
            {
                var listener = _factory.NewListener(port, inputQueue);
                await listener.ExecuteAsync(stoppingToken);
                //await Listener.ListenPortAsync(port,_context, _logger, cancellationToken)
                client.Connection.On<string>("GetFingerTimeoutCurrent", port1 => 
                {
                    Console.WriteLine("SignalR GetFingerTimeoutCurrent HIT!");
                    inputQueue.Enqueue(new SignalRMessage { Port = port1, Method = SignalRMethod.GetFingerTimeoutCurrent });
                });

                client.Connection.On<int, int>("AddFinger", (uid, privilage) => 
                {
                    Console.WriteLine("SignalR AddFinger HIT!");
                    Console.WriteLine($"uid: '{uid}', privilage: '{privilage}'");
                    inputQueue.Enqueue(new SignalRMessage { Method = SignalRMethod.AddFinger, Uid=uid, Privilage=privilage  });
                });

                client.Connection.On<int>("SetFingerTimeout", timeout =>
                {
                    Console.WriteLine($"Set timeout from server:{timeout}");
                });



                //inputQueue.EnqueueEvent += listener.OnSignalRMessage;

            }

            await client.StartAsync(stoppingToken);
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }

    public class SignalRMessage
    {
        public string Port { get; set; }
        public SignalRMethod Method { get; set; }
        public int Uid { get; set; }
        public int Privilage { get; set; }
    }

    public enum SignalRMethod
    {
        GetFingerTimeoutCurrent,
        AddFinger
    }
}

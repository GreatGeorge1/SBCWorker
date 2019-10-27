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
        //private readonly IOptions<WorkerOptions> _options;
        private readonly ListenerFactory _factory;
      //  private readonly MessageQueue<SignalRMessage> inputQueue;
        private readonly ConcurrentDictionary<string,MessageQueue<dynamic>> queueList;
        //private readonly MessageQueue<SignalRMessage> outputQueue;
        private readonly ServerSignalRClient client;

        public ListenerHost(ILogger<ListenerHost> logger, IEnumerable<SerialConfig> ports,ListenerFactory factory, ServerSignalRClient client)
        {
            _logger = logger;
            _ports = ports;
            _factory = factory;
            this.client = client;
            queueList = new ConcurrentDictionary<string,MessageQueue<dynamic>>();
            //inputQueue = new MessageQueue<SignalRMessage>();
           // outputQueue = new MessageQueue<SignalRMessage>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach(var port in _ports)
            {
                var tempQueue = new MessageQueue<dynamic>();
                queueList.TryAdd(port.PortName, tempQueue);
                Console.WriteLine($"PORT: {port.PortName}");
                var listener = _factory.NewListener(port, tempQueue);
                await listener.ExecuteAsync(stoppingToken).ConfigureAwait(false);
                //await Listener.ListenPortAsync(port,_context, _logger, cancellationToken)
                client.Connection.On<dynamic>("GetFingerTimeoutCurrent", req => 
                {
                    Console.WriteLine("SignalR GetFingerTimeoutCurrent HIT!");

                    if (req.Port is null) {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }
                    Console.WriteLine($"req.Port is '{req.Port}'");
                    MessageQueue<dynamic> queue;
                    var flag=queueList.TryGetValue(req.Port, out queue);

                    if (flag==true && !(queue is null)) 
                    {
                        queue.Enqueue(new { Port = req.Port, Method = SignalRMethod.GetFingerTimeoutCurrent });
                    }
                  //  inputQueue.Enqueue(new SignalRMessage { Port = port1, Method = SignalRMethod.GetFingerTimeoutCurrent });
                });

                client.Connection.On<AddFingerReq>("AddFinger", req =>
                {
                    Console.WriteLine("SignalR AddFinger HIT!");
                                 
                    if (req.Uid == 0)
                    {
                        Console.WriteLine("req.Uid is null or 0");
                        return;
                    }
                    if(req.Privilage == 0)
                    {
                        Console.WriteLine("req.Privilage is null or 0");
                        return;
                    }
                    if (req.Port is null)
                    {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }
                    Console.WriteLine($"req.Port is '{(string)req.Port}'");
                    Console.WriteLine($"uid: '{req.Uid}', privilage: '{req.Privilage}' port: '{req.Port}'");

                    MessageQueue<dynamic> queue;
                    var flag = queueList.TryGetValue(req.Port, out queue);

                    if (flag == true && !(queue is null))
                    {
                        queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.AddFinger, Uid=req.Uid, Privilage= req.Privilage });
                    }

                });

                client.Connection.On<SendConfigReq>("SendConfig", req => 
                {
                    Console.WriteLine("SignalR SendConfig HIT!");
                   
                    if (req.JsonString is null)
                    {
                        Console.WriteLine("req.JsonString is null or not exist");
                        return;
                    }
                    if (req.Port is null)
                    {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }

                    MessageQueue<dynamic> queue;
                    var flag = queueList.TryGetValue(req.Port, out queue);

                    if (flag == true && !(queue is null))
                    {
                        queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.SendConfig, JsonString = req.JsonString });
                    }
                 
                });

                client.Connection.On<DeleteFingerByIdReq>("DeleteFingerById", req => 
                {
                    Console.WriteLine("SignalR DeleteFingerById HIT!");
                    if (req.Port is null)
                    {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }
                    if(req.Id == 0)
                    {
                        Console.WriteLine("req.Id is null or 0");
                        return;
                    }

                    MessageQueue<dynamic> queue;
                    var flag = queueList.TryGetValue(req.Port, out queue);

                    if (flag == true && !(queue is null))
                    {
                        queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.DeleteFingerById, Uid=req.Id });
                    }

                });

                client.Connection.On<AddFingerByBleReq>("AddFingerByBle", req => 
                {
                    Console.WriteLine("SignalR AddFingerByBle HIT!");
                    if (req.Port is null)
                    {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }
                    if(req.UserId is null)
                    {
                        Console.WriteLine("req.UserId is null or not exist");
                        return;
                    }
                    if (req.Ble is null)
                    {
                        Console.WriteLine("req.Ble is null or not exist");
                        return;
                    }
                    if(req.Id == 0)
                    {
                        Console.WriteLine("req.Id is null or 0");
                        return;
                    }
                    if (req.Privilage == 0 )
                    {
                        Console.WriteLine("req.Privilage is null or 0");
                        return;
                    }

                    MessageQueue<dynamic> queue;
                    var flag = queueList.TryGetValue(req.Port, out queue);

                    if (flag == true && !(queue is null))
                    {
                        queue.Enqueue(new SignalRMessage 
                        {
                            Port = req.Port, 
                            Method = SignalRMethod.AddFingerByBle, 
                            Uid = req.Id,
                            UserId=req.UserId,
                            BleString=req.Ble,
                            Privilage=req.Privilage
                        });
                    }
                });

                client.Connection.On<SetFingerTimeoutReq>("SetFingerTimeout", req =>
                {
                    Console.WriteLine("SignalR SetFingerTimeout HIT!");
                    if (req.Port is null)
                    {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }

                    //if(req.Timeout )
                    //{
                    //    Console.WriteLine("req.Timeout is null or not exist");
                    //    return;
                    //}

                    MessageQueue<dynamic> queue;
                    queueList.TryGetValue(req.Port, out queue);

                    if (!(queue is null))
                    {
                        queue.Enqueue(new 
                        {
                            Method = SignalRMethod.SetFingerTimeout,
                            Port = req.Port,
                            Timeout = req.Timeout
                        });
                    }
                });

                client.Connection.On<DeleteAllFingerprintsReq>("DeleteAllFingerprints", req=> 
                {
                    Console.WriteLine("SignalR DeleteAllFingerprints HIT!");
                    if (req.Port is null)
                    {
                        Console.WriteLine("req.Port is null or not exist");
                        return;
                    }

                    MessageQueue<dynamic> queue;
                    queueList.TryGetValue(req.Port, out queue);

                    if (!(queue is null))
                    {
                        queue.Enqueue(new
                        {
                            Method = SignalRMethod.DeleteAllFingerprints,
                            Port = req.Port
                        });
                    }
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
        public string UserId { get; set; }
        public int Uid { get; set; }
        public string BleString { get; set; }
        public int Privilage { get; set; }
        public string JsonString { get; set; }
        public int Timeout { get; set; }
    }

    public enum SignalRMethod
    {
        GetFingerTimeoutCurrent,
        AddFinger,
        SendConfig,
        DeleteFingerById,
        AddFingerByBle,
        DeleteAllFingerprints,
        SetFingerTimeout
    }


    #region client requests
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

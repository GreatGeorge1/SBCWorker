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
using Protocol;
using Worker.Host.Transports;
using Protocol.Events;

namespace Worker.Host
{
    public class Saga 
    {
        public  bool IsCompleted { get; set; } = false;
        public  bool IsFired { get; set; } = false;
    }
    public class AddFingerSaga:Saga
    {
        public string UserBLE { get; set; }
        public string UserId { get; set; }
        public int FingerId { get; set; } = 0;
        public int UserPrivilage { get; set; } = 0;
        private bool BleAuthIsCompleted { get; set; } = false;//step 1
        public async Task CompleteBle(Protocol.Host host, int id, int privilage)
        {
            Console.WriteLine("Saga CompleteBle HIT!");
            var res=await host.ExecuteControllerMethodAsync(CommandHeader.FingerWriteInBase, new byte[] { (byte)id, (byte)privilage });
            FingerWriteIsCompleted = true;
            IsCompleted = true;
            Console.WriteLine("Info: Saga is completed");
        } 
        public bool FingerWriteIsCompleted { get; set;  }= false;//step 2
    }

    public partial class Listener
    {
        private readonly ILogger logger;
        private readonly ControllerDbContext context;
        // private readonly SerialConfig port;
        private readonly Protocol.Host host;
        private readonly string PortName;
        private readonly MessageQueue<SignalRMessage> inputQueue;

        private AddFingerSaga FSaga = null;
        private readonly Queue<AddFingerSaga> SagaQueue = new Queue<AddFingerSaga>();

        public Listener(ILogger logger, SerialConfig port,
           ControllerDbContext dbcontext, MessageQueue<SignalRMessage> inputQueue)
        {
            PortName = port.PortName;
            this.logger = logger;
            this.context = dbcontext;
            this.host = new Protocol.Host(new SerialPortTransport(port, logger));
            host.CardCommandEvent += OnCardCommandEvent;
            host.FingerCommandEvent += OnFingerCommandEvent;
            host.BleCommandEvent += OnBleCommandEvent;

            this.inputQueue = inputQueue;
            inputQueue.EnqueueEvent += OnSignalRMessage;
        }

        public async Task EnqueueSaga(AddFingerSaga saga)
        {
            Console.WriteLine("EnqueueSaga");
            SagaQueue.Enqueue(saga);
            _=ProcessSaga();
        }

        public async Task ProcessSaga()
        {
            Console.WriteLine("ProcessSaga");
            if (FSaga is null)
            {
                FSaga=SagaQueue.Dequeue();
                FSaga.IsFired = true;
                Console.WriteLine("Info: Saga is fired"); 
            }
            else
            {
                while (FSaga.IsCompleted != true)
                {
                    Task.Delay(1000).Wait();
                }
                FSaga = SagaQueue.Dequeue();
                FSaga.IsFired = true;
                Console.WriteLine("Info: Saga is fired");
            }
        }

        public async void OnSignalRMessage(object sender, MessageQueueEnqueueEventArgs<SignalRMessage> args)
        {
            //if (PortName != args.Item.Port)
            //{
            //    return;
            //}
            //Console.WriteLine("OnSignalRMessage HIT");
            var item = inputQueue.Dequeue();//govno
            switch(args.Item.Method){
                case SignalRMethod.GetFingerTimeoutCurrent:
                    Console.WriteLine("OnSignalRMessage GetFingerTimeoutCurrent HIT2");       
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerTimeoutCurrent, new byte[] { });
                    break;
                case SignalRMethod.AddFinger:
                    Console.WriteLine("OnSignalRMessage AddFinger HIT2");
                    Console.WriteLine($"uid: '{args.Item.Uid}' hex: '{BitConverter.ToString(new byte[] { (byte)args.Item.Uid }) }'");
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerWriteInBase, new byte[] { (byte)args.Item.Uid, (byte)args.Item.Privilage });
                    break;
                case SignalRMethod.SendConfig:
                    Console.WriteLine("OnSignalRMessage SendConfig HIT2");
                    Console.WriteLine($"json: '{args.Item.JsonString}' ");
                    byte[] jsonBytes = ASCIIEncoding.ASCII.GetBytes(args.Item.JsonString);
                    await host.ExecuteControllerMethodAsync(CommandHeader.TerminalConf, jsonBytes);
                    break;
                case SignalRMethod.DeleteFingerById:
                    Console.WriteLine("OnSignalRMessage DeleteFingerById HIT2");
                    Console.WriteLine($"port: '{args.Item.Port}' uid: '{args.Item.Uid}'");//TODO REF
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerDeleteId, new byte[] { (byte)args.Item.Uid, 0x00 });
                    break;
                case SignalRMethod.AddFingerByBle:
                    Console.WriteLine("OnSignalRMessage AddFingerByBle HIT2");
                    Console.WriteLine($"port: '{args.Item.Port}' uid: '{args.Item.Uid}' userid: '{args.Item.UserId}' Ble: '{args.Item.BleString}'");
                    await EnqueueSaga(new AddFingerSaga
                    {
                        UserBLE = args.Item.BleString,
                        FingerId = args.Item.Uid,
                        UserPrivilage = args.Item.Privilage,
                        UserId=args.Item.UserId
                    });
                    break;
                case SignalRMethod.DeleteAllFingerprints:
                    Console.WriteLine("OnSignalRMessage DeleteAllFingerprints HIT2");
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerDeleteAll, new byte[] { 0x01, 0x00 });
                    break;
                case SignalRMethod.SetFingerTimeout:
                    Console.WriteLine("OnSignalRMessage SetFingerTimeout HIT2");
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerSetTimeout, new byte[] { (byte)args.Item.Timeout, 0x00 });
                    break;
                default:Console.WriteLine("OnSignalRMessage UNexpected");
                    break;
            }
        }

        private async void OnCardCommandEvent(object sender, CardCommandEventArgs args)
        {
            var res = await VerifyCard(args.Card);
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Card)
            {
                return;
            }
            if (res)
            {
                await host.SendResponseToTerminal(new byte[] { 0x00,0x01}, CommandHeader.Card);
            }
            else
            {
                await host.SendResponseToTerminal(new byte[] { 0x00, 0x00 }, CommandHeader.Card);
            }
        }

        private async void OnFingerCommandEvent(object sender, FingerCommandEventArgs args)
        {
            var res = await VerifyFinger(args.Finger);
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Finger)
            {
                return;
            }
            if (res)
            {
                await host.SendResponseToTerminal(new byte[] { 0x00, 0x01 }, CommandHeader.Finger);
            }
            else{
                await host.SendResponseToTerminal(new byte[] { 0x00, 0x00 }, CommandHeader.Finger);
            }
        }

        private async void OnBleCommandEvent(object sender, BleCommandEventArgs args)
        {
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Ble)
            {
                return;
            }
            //TODO AUTH
            await host.SendResponseToTerminal(new byte[] { 0x00, 0x01 }, CommandHeader.Ble);
            if (!(FSaga is null) && FSaga.IsFired==true && FSaga.IsCompleted==false)
            {
                Console.WriteLine("BLE auth with Saga");
               
                string bleStr= ASCIIEncoding.ASCII.GetString(args.Ble);
                //if (FSaga.UserBLE == bleStr)
                if (true)
                {
                    FSaga.CompleteBle(host, FSaga.FingerId, FSaga.UserPrivilage);
                    //Saga = saga;
                }
            }
        }

        public Task ExecuteAsync(CancellationToken ct)
        {
            _=host.ExecuteAsync(ct);
            return Task.CompletedTask;
        }
    }
}

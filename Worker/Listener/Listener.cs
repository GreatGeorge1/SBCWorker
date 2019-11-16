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
using System.Collections.Concurrent;

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
            bool res=false;
            int i = 0;
            while (res != true)
            {
                if (i >= 3)
                {
                    break;
                }
                res=await host.ExecuteControllerMethodAsync(CommandHeader.FingerWriteInBase, new byte[] { (byte)id, (byte)privilage }).ConfigureAwait(false);
                i++;
            }
            if (i >= 3)
            {
                Console.WriteLine("Info: Saga is failed, can't execute method");
                return;
            }
            FingerWriteIsCompleted = true;
            IsCompleted = true;
            Console.WriteLine("Info: Saga is completed");
        } 
        public bool FingerWriteIsCompleted { get; set;  }= false;//step 2
    }

    public partial class Listener<QueueT>
    {
        private readonly ILogger logger;
        private readonly ControllerDbContext context;
        // private readonly SerialConfig port;
        private readonly Protocol.Host host;
        private readonly string PortName;
        private readonly ConcurrentMessageBag<SignalRMessage> inputQueue;
        private readonly ConcurrentMessageBag<SignalRresponse> outputQueue;

        private AddFingerSaga FSaga = null;
        private readonly ConcurrentQueue<AddFingerSaga> SagaQueue = new ConcurrentQueue<AddFingerSaga>();

        public Listener(ILogger logger, SerialConfig port,
           ControllerDbContext dbcontext, ConcurrentMessageBag<SignalRMessage> inputQueue, ConcurrentMessageBag<SignalRresponse> outputQueue)
        {
            PortName = port.PortName;
            this.logger = logger;
            this.context = dbcontext;
            this.host = new Protocol.Host(new SerialPortTransport(port, logger));
            host.CardCommandEvent += OnCardCommandEvent;
            host.FingerCommandEvent += OnFingerCommandEvent;
            host.BleCommandEvent += OnBleCommandEvent;
            host.GetConfigEvent += OnGetConfigEvent;

            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            inputQueue.EnqueueEvent += OnSignalRMessage;
        }

        public Task EnqueueSaga(AddFingerSaga saga)
        {
            Console.WriteLine("EnqueueSaga");
            SagaQueue.Enqueue(saga);
            _=ProcessSaga();
            return Task.CompletedTask;
        }

        public async Task ProcessSaga()
        {
            Console.WriteLine("ProcessSaga");
            if (FSaga is null)
            {
                _=SagaQueue.TryDequeue(out AddFingerSaga Saga);
                FSaga = Saga;
                FSaga.IsFired = true;
                Console.WriteLine("Info: Saga is fired"); 
            }
            else
            {
                while (FSaga.IsCompleted != true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }
                _ = SagaQueue.TryDequeue(out AddFingerSaga Saga);
                FSaga = Saga;
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
            var t = args;
            _ = inputQueue.TryDequeue(out SignalRMessage item);//govno
            switch (t.Item.Method){
                case SignalRMethod.GetConfig:
                    Console.WriteLine("OnSignalRMessage GetConfig HIT2");
                    await host.ExecuteControllerMethodAsync(CommandHeader.TerminalSysInfo, new byte[] { 0x00, 0x00 }, (string)t.Item.Address).ConfigureAwait(false);

                    break;
                case SignalRMethod.GetFingerTimeoutCurrent:
                    Console.WriteLine("OnSignalRMessage GetFingerTimeoutCurrent HIT2");       
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerTimeoutCurrent, new byte[] { 0x00, 0x00 }).ConfigureAwait(false);
                    break;
                case SignalRMethod.AddFinger:
                    Console.WriteLine("OnSignalRMessage AddFinger HIT2");
                    Console.WriteLine($"uid: '{t.Item.Uid}' hex: '{BitConverter.ToString(new byte[] { (byte)t.Item.Uid }) }'");
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerWriteInBase, new byte[] { (byte)t.Item.Uid, (byte)t.Item.Privilage }).ConfigureAwait(false);
                    break;
                case SignalRMethod.SendConfig:
                    Console.WriteLine("OnSignalRMessage SendConfig HIT2");
                    Console.WriteLine($"json: '{t.Item.JsonString}' ");
                    byte[] jsonBytes = ASCIIEncoding.ASCII.GetBytes(t.Item.JsonString);
                    await host.ExecuteControllerMethodAsync(CommandHeader.TerminalConf, jsonBytes).ConfigureAwait(false);
                    break;
                case SignalRMethod.DeleteFingerById:
                    Console.WriteLine("OnSignalRMessage DeleteFingerById HIT2");
                    Console.WriteLine($"port: '{t.Item.Port}' uid: '{t.Item.Uid}'");//TODO REF
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerDeleteId, new byte[] { (byte)t.Item.Uid, 0x00 }).ConfigureAwait(false);
                    break;
                case SignalRMethod.AddFingerByBle:
                    Console.WriteLine("OnSignalRMessage AddFingerByBle HIT2");
                    Console.WriteLine($"port: '{t.Item.Port}' uid: '{t.Item.Uid}' userid: '{t.Item.UserId}' Ble: '{t.Item.BleString}'");
                    await EnqueueSaga(new AddFingerSaga
                    {
                        UserBLE = t.Item.BleString,
                        FingerId = t.Item.Uid,
                        UserPrivilage = t.Item.Privilage,
                        UserId=t.Item.UserId
                    });
                    break;
                case SignalRMethod.DeleteAllFingerprints:
                    Console.WriteLine("OnSignalRMessage DeleteAllFingerprints HIT2");
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerDeleteAll, new byte[] { 0x01, 0x00 }).ConfigureAwait(false);
                    break;
                case SignalRMethod.SetFingerTimeout:
                    Console.WriteLine("OnSignalRMessage SetFingerTimeout HIT2");
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerSetTimeout, new byte[] { (byte)t.Item.Timeout, 0x00 }).ConfigureAwait(false);
                    break;
                default:Console.WriteLine("OnSignalRMessage UNexpected");
                    break;
            }
        }

        public async void OnGetConfigEvent(object sender, GetConfigEventArgs args)
        {
            Console.WriteLine($"OnGetConfigEvent Hit json:'{Encoding.ASCII.GetString(args.Json)}' address:'{args.Address}'");
            outputQueue.Enqueue(new SignalRresponse{ JsonString= Encoding.ASCII.GetString(args.Json), Method=SignalRMethod.GetConfig, Address=args.Address, Port=PortName});
        }

        private async void OnCardCommandEvent(object sender, CardCommandEventArgs args)
        {
            var res = await VerifyCard(args.Card).ConfigureAwait(false);
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Card)
            {
                return;
            }
            if (res)
            {
                await host.SendResponseToTerminal(new byte[] { 0x00,0x01}, CommandHeader.Card).ConfigureAwait(false);
            }
            else
            {
                await host.SendResponseToTerminal(new byte[] { 0x00, 0x00 }, CommandHeader.Card).ConfigureAwait(false);
            }
        }

        private async void OnFingerCommandEvent(object sender, FingerCommandEventArgs args)
        {
            var res = await VerifyFinger(args.Finger).ConfigureAwait(false);
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Finger)
            {
                return;
            }
            if (res)
            {
                await host.SendResponseToTerminal(new byte[] { 0x00, 0x01 }, CommandHeader.Finger).ConfigureAwait(false);
            }
            else{
                await host.SendResponseToTerminal(new byte[] { 0x00, 0x00 }, CommandHeader.Finger).ConfigureAwait(false);
            }
        }

        private async void OnBleCommandEvent(object sender, BleCommandEventArgs args)
        {
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Ble)
            {
                return;
            }
            //TODO AUTH
            await host.SendResponseToTerminal(new byte[] { 0x00, 0x01 }, CommandHeader.Ble).ConfigureAwait(false);
            if (!(FSaga is null) && FSaga.IsFired==true && FSaga.IsCompleted==false)
            {
                Console.WriteLine("BLE auth with Saga");
               
                string bleStr= ASCIIEncoding.ASCII.GetString(args.Ble);
                //if (FSaga.UserBLE == bleStr)
                if (true)
                {
                    await Task.WhenAny(FSaga.CompleteBle(host, FSaga.FingerId, FSaga.UserPrivilage)).ConfigureAwait(false);
                    //Saga = saga;
                }
            }
        }

    }
}

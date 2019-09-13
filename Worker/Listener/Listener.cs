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
    public partial class Listener
    {
        private readonly ILogger logger;
        private readonly ControllerDbContext context;
        // private readonly SerialConfig port;
        private readonly Protocol.Host host;
        private readonly string PortName;
        private readonly MessageQueue<SignalRMessage> inputQueue;

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

        public async void OnSignalRMessage(object sender, MessageQueueEnqueueEventArgs<SignalRMessage> args)
        {
            //if (PortName != args.Item.Port)
            //{
            //    return;
            //}
            Console.WriteLine("OnSignalRMessage HIT");
            var item = inputQueue.Dequeue();//govno
            switch(args.Item.Method){
                case SignalRMethod.GetFingerTimeoutCurrent:
                    Console.WriteLine("OnSignalRMessage GetFingerTimeoutCurrent HIT");
                   
                    await host.ExecuteControllerMethodAsync(CommandHeader.FingerTimeoutCurrent, new byte[] { });
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
            await host.SendResponseToTerminal(new byte[] { 0x00, 0x01 }, CommandHeader.Ble);
        }

        public Task ExecuteAsync(CancellationToken ct)
        {
            _=host.ExecuteAsync(ct);
            return Task.CompletedTask;
        }
    }
}

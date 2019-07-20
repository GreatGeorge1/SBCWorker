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

        public Listener(ILogger logger, SerialConfig port,
           ControllerDbContext dbcontext)
        {
            this.logger = logger;
            this.context = dbcontext;
            this.host = new Protocol.Host(new SerialPortTransport(port, logger));
            host.CardCommandEvent += OnCardCommandEvent;
            host.FingerCommandEvent += OnFingerCommandEvent;
        }

        private async void OnCardCommandEvent(object sender, CardCommandEventArgs args)
        {
            var res = await VerifyCard(args.Card, args.Md5Hash);
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Card)
            {
                return;
            }
            if (res)
            {
                await host.SendResponseToTerminal(ResponseHeader.CardOk, CommandHeader.Card);
            }
            else
            {
                await host.SendResponseToTerminal(ResponseHeader.CardError, CommandHeader.Card);
            }
        }

        private async void OnFingerCommandEvent(object sender, FingerCommandEventArgs args)
        {
            var res = await VerifyFinger(args.Finger, args.Md5Hash);
            if (host.ExecutedMethod.MethodInfo.CommandHeader != CommandHeader.Finger)
            {
                return;
            }
            if (res)
            {
                await host.SendResponseToTerminal(ResponseHeader.FingerOk,CommandHeader.Finger);
            }
            else{
                await host.SendResponseToTerminal(ResponseHeader.FingerError, CommandHeader.Finger);
            }
        }

        public Task ExecuteAsync(CancellationToken ct)
        {
            _=host.ExecuteAsync(ct);
            return Task.CompletedTask;
        }
    }
}

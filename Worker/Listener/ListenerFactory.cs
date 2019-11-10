using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Worker.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Worker.Models;
using Protocol;

namespace Worker.Host
{
    public class ListenerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public ListenerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        //public Listener<SignalRMessage> NewListener(SerialConfig port, MessageQueue<SignalRMessage> inputQueue)
        //{
        //    var logger = _serviceProvider.GetService<ILogger<Listener>>();
        //    var context = _serviceProvider.GetService<ControllerDbContext>();
        //    return new Listener<SignalRMessage>(logger, port, context, inputQueue);
        //}

        public Listener<dynamic> NewListener(SerialConfig port, MessageQueue<SignalRMessage> inputQueue, MessageQueue<SignalRresponse> outputQueue)
        {
            var logger = _serviceProvider.GetService<ILogger<Listener>>();
            var context = _serviceProvider.GetService<ControllerDbContext>();
            return new Listener<dynamic>(logger, port, context, inputQueue, outputQueue);
        }
    }
}

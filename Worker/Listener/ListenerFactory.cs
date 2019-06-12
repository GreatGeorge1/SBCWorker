using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Worker.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Worker.Models;

namespace Worker.Host
{
    public class ListenerFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceProvider _serviceProvider;
        public ListenerFactory(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public Listener NewListener(ListenerPort port)
        {
            var logger = _serviceProvider.GetService<ILogger<Listener>>();
            var context = _serviceProvider.GetService<ControllerDbContext>();
            return new Listener(logger, port, context);
        }
    }
}

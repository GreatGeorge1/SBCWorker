using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Worker.EntityFrameworkCore;
using Worker.Models;

namespace Worker.Host
{
    public class ListenerHost : BackgroundService
    {
        private readonly ILogger<ListenerHost> _logger;
        private readonly IEnumerable<SerialConfig> _ports;
        //private readonly IOptions<WorkerOptions> _options;
        private readonly ListenerFactory _factory;

        public ListenerHost(ILogger<ListenerHost> logger, IEnumerable<SerialConfig> ports,ListenerFactory factory)
        {
            _logger = logger;
            _ports = ports;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            foreach(var port in _ports)
            {
                var listener = _factory.NewListener(port);
                await listener.ExecuteAsync(stoppingToken);
                //await Listener.ListenPortAsync(port,_context, _logger, cancellationToken)
            }


            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }
}

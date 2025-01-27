using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Worker.EntityFrameworkCore;
using Worker.Host.SignalR;
using Worker.Models;

namespace Worker.Host
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
            .ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile("hostsettings.json", optional: true);
                configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                configHost.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                configApp.SetBasePath(Directory.GetCurrentDirectory());
                configApp.AddJsonFile("appsettings.json", optional: true);
                configApp.AddJsonFile(
                    $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                    optional: true);
                configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                configApp.AddCommandLine(args);
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddConsole();
                configLogging.AddDebug();
            })
            .ConfigureServices((hostContext, services) =>
            {
                //var configBuilder = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("appsettings.json", optional: true);
                //var config = configBuilder.Build();

               // var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
                var connectingString = hostContext.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<ControllerDbContext>(opt =>
                {
                    opt.UseSqlite(connectingString);
                       // d => d.MigrationsAssembly(migrationsAssembly));
                });
                services.AddSingleton<ListenerFactory>();
                services.Configure<WorkerOptions>(hostContext.Configuration.GetSection("ListenerOptions"));
                services.Configure<SignalROptions>(hostContext.Configuration.GetSection("SignalROptions"));
                var options = new WorkerOptions();

                hostContext.Configuration.GetSection("ListenerOptions").Bind(options);
                foreach (var port in options.Ports)
                {
                    services.AddSingleton(new SerialConfig(port.PortName,port.IsRS485));
                }
                services.AddSingleton<InputMessageQueue>();
                services.AddSingleton<OutputMessageQueue>();

                services.AddHostedService<SignalRService>();
                services.AddHostedService<ListenerHost>();

            }).Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Api
{
    public static class Extensions
    {
        public static IHostBuilder UseNServiceBus(this IServiceCollection services)
        {
            var host = Host.CreateDefaultBuilder()
                .UseNServiceBus(context =>
                {
                    var endpointConfiguration = new EndpointConfiguration("transactions");
                    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                    transport.ConnectionString("host=localhost").UseDirectRoutingTopology();

                    endpointConfiguration.SendFailedMessagesTo("error");
                    endpointConfiguration.AuditProcessedMessagesTo("audit");
                    endpointConfiguration.SendHeartbeatTo("error");

                    return endpointConfiguration;
                })
                .Build();

            host.Run();

            return host as HostBuilder;
        }
    }
}

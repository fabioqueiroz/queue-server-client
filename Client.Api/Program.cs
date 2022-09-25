using Common;
using Common.Data.Access;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Api
{
    public class Program
    {
        //    public static void Main(string[] args)
        //    {
        //        CreateHostBuilder(args).Build().Run();
        //    }

        //    public static IHostBuilder CreateHostBuilder(string[] args) =>
        //        Host.CreateDefaultBuilder(args)
        //            .UseNServiceBus(context =>
        //            {
        //                var endpointConfiguration = new EndpointConfiguration("transactions");
        //                var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        //                transport.ConnectionString("host=localhost").UseDirectRoutingTopology();

        //                endpointConfiguration.LicensePath(context.Configuration["NServiceBus:LicensePath"]);

        //                endpointConfiguration.SendFailedMessagesTo("error");
        //                endpointConfiguration.AuditProcessedMessagesTo("audit");
        //                endpointConfiguration.SendHeartbeatTo("heartbeat");

        //                return endpointConfiguration;
        //            })
        //            .ConfigureWebHostDefaults(webBuilder =>
        //            {
        //                webBuilder.UseStartup<Startup>();
        //            });
        //}

        public static async Task Main()
        {
            Console.Title = "Client";

            var connectionString = @"Data Source=DESKTOP-39JIMJV\SQLEXPRESS;Initial Catalog=ServerQueue;Integrated Security=False;User Id=sa;Password=Fabio1980;MultipleActiveResultSets=True";
            var endpointConfiguration = new EndpointConfiguration("transactions");

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendFailedMessagesTo("failed_messages");

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(connectionString);
            transport.DefaultSchema("client");
            transport.UseSchemaForQueue("error", "dbo");
            transport.UseSchemaForQueue("audit", "dbo");

            endpointConfiguration.LicensePath("C:\\Users\\fabio\\AppData\\Local\\ParticularSoftware\\license.xml");

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();

            persistence.ConnectionBuilder(connectionBuilder: () =>
            {
                return new SqlConnection(connectionString);
            });

            var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
            dialect.Schema("client");
            persistence.TablePrefix("");

            var subscriptions = transport.SubscriptionSettings();
            subscriptions.DisableSubscriptionCache();
            subscriptions.SubscriptionTableName(tableName: "Subscriptions", schemaName: "dbo");

            endpointConfiguration.EnableOutbox();

            // sql helper
            SqlHelper.CreateSchema(connectionString, "client");

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key != ConsoleKey.Enter)
                {
                    break;
                }
                var orderSubmitted = new Payment
                {
                    Id = 1,
                    Debitor = "Test",
                    Creditor = "Test Creditor",
                    Amount = 100
                };
                await endpointInstance.Publish(orderSubmitted)
                    .ConfigureAwait(false);
            }
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}


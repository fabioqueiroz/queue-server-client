using Common.Data.Access;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        public static string _licensePath = string.Empty;

        //public static void Main(string[] args)
        //{
        //    CreateHostBuilder(args).Build().Run();
        //}

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .UseNServiceBus(context =>
        //        {
        //            var endpointConfiguration = new EndpointConfiguration("transactions");
        //            // RabbitMQ implementation
        //            //var transport = endpointConfiguration.UseTransport<RabbitMQTransport>(); 
        //            //transport.ConnectionString("host=localhost").UseDirectRoutingTopology();

        //            //endpointConfiguration.SendFailedMessagesTo("error");
        //            //endpointConfiguration.AuditProcessedMessagesTo("audit");
        //            //endpointConfiguration.SendHeartbeatTo("heartbeat");

        //            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        //            transport.ConnectionString(context.Configuration["ConnectionStrings:DefaultConnection"]);
        //            transport.DefaultSchema("server");
        //            transport.UseSchemaForEndpoint("outbox.client", "client");
        //            transport.UseSchemaForQueue("error", "dbo");
        //            transport.UseSchemaForQueue("audit", "dbo");

        //            endpointConfiguration.LicensePath(context.Configuration["NServiceBus:LicensePath"]);

        //            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        //            persistence.ConnectionBuilder(connectionBuilder: () =>  
        //            {
        //                return new SqlConnection(context.Configuration["ConnectionStrings:DefaultConnection"]);
        //            });

        //            var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        //            dialect.Schema("server");
        //            persistence.TablePrefix("");

        //            var subscriptions = transport.SubscriptionSettings();
        //            subscriptions.DisableSubscriptionCache();
        //            subscriptions.SubscriptionTableName(tableName: "subscriptions", schemaName: "dbo");

        //            endpointConfiguration.EnableOutbox();

        //            // sql helper
        //            SqlHelper.CreateSchema(context.Configuration["ConnectionStrings:DefaultConnection"], "server");
        //            SqlHelper.ExecuteQuery(context.Configuration["ConnectionStrings:DefaultConnection"], File.ReadAllText("Startup.sql"));

        //            //var endpointInstance = await Endpoint.Start(endpointConfiguration)
        //            //    .ConfigureAwait(false);

        //            //await endpointInstance.Stop();
        //            ///////////

        //            return endpointConfiguration;
        //        })
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });

        public static async Task Main()
        {
            Console.Title = "Server";

            var connectionString = @"Data Source=DESKTOP-39JIMJV\SQLEXPRESS;Initial Catalog=ServerQueue;Integrated Security=False;User Id=sa;Password=Fabio1980;MultipleActiveResultSets=True";
            var endpointConfiguration = new EndpointConfiguration("transactions");

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(connectionString);
            transport.DefaultSchema("server");
            transport.UseSchemaForEndpoint("outbox.client", "client");
            transport.UseSchemaForQueue("error", "dbo");
            transport.UseSchemaForQueue("audit", "dbo");

            endpointConfiguration.LicensePath("C:\\Users\\fabio\\AppData\\Local\\ParticularSoftware\\license.xml");

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.ConnectionBuilder(connectionBuilder: () =>
            {
                return new SqlConnection(connectionString);
            });

            var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
            dialect.Schema("server");
            persistence.TablePrefix("");

            var subscriptions = transport.SubscriptionSettings();
            subscriptions.DisableSubscriptionCache();
            subscriptions.SubscriptionTableName(tableName: "Subscriptions", schemaName: "dbo");

            endpointConfiguration.EnableOutbox();

            // sql helper
            SqlHelper.CreateSchema(connectionString, "server");
            SqlHelper.ExecuteSql(connectionString, File.ReadAllText("Startup.sql"));

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Connecting to endpoint instance");
            Console.ReadKey();

            await endpointInstance.Stop();
        }
    }
}

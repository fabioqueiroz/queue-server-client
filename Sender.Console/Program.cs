using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Common.Data.Access;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SqlServer;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.SqlOutbox.Sender";
        var random = new Random();

        var endpointConfiguration = new EndpointConfiguration("Samples.SqlOutbox.Sender");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");

        #region SenderConfiguration

        var connection = @"Data Source=DESKTOP-39JIMJV\SQLEXPRESS;Initial Catalog=ServerQueue;Integrated Security=False;User Id=sa;Password=Fabio1980;MultipleActiveResultSets=True";

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(connection);
        transport.DefaultSchema("sender");
        transport.UseSchemaForQueue("error", "dbo");
        transport.UseSchemaForQueue("audit", "dbo");

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(connection);
            });
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        dialect.Schema("sender");
        persistence.TablePrefix("");

        var subscriptions = transport.SubscriptionSettings();
        subscriptions.DisableSubscriptionCache();

        subscriptions.SubscriptionTableName(
            tableName: "Subscriptions",
            schemaName: "dbo");

        endpointConfiguration.EnableOutbox();

        #endregion

        SqlHelper.CreateSchema(connection, "sender");
        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press enter to send a message");
        Console.WriteLine("Press any key to exit");

        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();

            if (key.Key != ConsoleKey.Enter)
            {
                break;
            }
            var orderSubmitted = new OrderSubmitted
            {
                OrderId = Guid.NewGuid(),
                Value = random.Next(100)
            };
            await endpointInstance.Publish(orderSubmitted)
                .ConfigureAwait(false);
        }
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Common.Data.Access;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SqlServer;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.SQLOutboxEF.Receiver";

        var connection = @"Data Source=DESKTOP-39JIMJV\SQLEXPRESS;Initial Catalog=ServerQueue;Integrated Security=False;User Id=sa;Password=Fabio1980;MultipleActiveResultSets=True";

        var endpointConfiguration = new EndpointConfiguration("Samples.SqlOutbox.Receiver");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");

        #region ReceiverConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(connection);
        transport.DefaultSchema("receiver");
        transport.UseSchemaForEndpoint("Samples.SqlOutbox.Sender", "sender");
        transport.UseSchemaForQueue("error", "dbo");
        transport.UseSchemaForQueue("audit", "dbo");

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(connection);
            });
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        dialect.Schema("receiver");
        persistence.TablePrefix("");

        var subscriptions = transport.SubscriptionSettings();
        subscriptions.DisableSubscriptionCache();

        subscriptions.SubscriptionTableName(
            tableName: "Subscriptions",
            schemaName: "dbo");

        endpointConfiguration.EnableOutbox();

        #endregion
        SqlHelper.CreateSchema(connection, "receiver");

        SqlHelper.ExecuteSql(connection, File.ReadAllText("Startup.sql"));

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}
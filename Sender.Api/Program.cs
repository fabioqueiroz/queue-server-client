using Common.Data.Access;
using NServiceBus;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseNServiceBus(context =>
{
    var random = new Random();

    var endpointConfiguration = new EndpointConfiguration("Samples.SqlOutbox.Sender");
    endpointConfiguration.EnableInstallers();
    endpointConfiguration.SendFailedMessagesTo("error");

    #region SenderConfiguration

    var connection = @"Data Source=DESKTOP-39JIMJV\SQLEXPRESS;Initial Catalog=ServerQueueApi;Integrated Security=False;User Id=sa;Password=Fabio1980;MultipleActiveResultSets=True";

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

    return endpointConfiguration;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

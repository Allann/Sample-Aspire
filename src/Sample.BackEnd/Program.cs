using MassTransit;

using Sample.Components;
using Sample.Components.OrderManagement;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessOrderConsumer>();

    x.AddConfigureEndpointsCallback((provider, name, cfg) =>
    {
        if (cfg is ISqlReceiveEndpointConfigurator sql)
        {
            sql.LockDuration = TimeSpan.FromMinutes(10);

            // Ensure messages are consumed in order within a partition
            // Prevents head-of-line blocking across customers
            sql.SetReceiveMode(SqlReceiveMode.PartitionedOrdered);
        }
    });

    x.AddDelayedMessageScheduler();

    x.UsingRabbitMq((context, cfg) =>
    {
        string? connectionString = builder.Configuration.GetConnectionString("messaging");
        if (connectionString == null)
        {
            throw new InvalidOperationException("Connection string 'messaging' is not configured.");
        }

        cfg.Host(new Uri(connectionString), h => { });

        cfg.UseSqlMessageScheduler();

        cfg.UsePublishFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
        cfg.UseSendFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);

        cfg.ConfigureEndpoints(context);
    });
});

IHost host = builder.Build();
host.Run();
using MassTransit;

using Sample.Components;
using Sample.Contracts;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();

builder.Services.AddMassTransit(x =>
{
    x.AddDelayedMessageScheduler();
    x.UsingRabbitMq((context, cfg) =>
    {
        string? connectionString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(connectionString);

        cfg.UsePublishFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
        cfg.UseSendFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.MapPost("/order", async (OrderModel order, IPublishEndpoint publishEndpoint) =>
    {
        await publishEndpoint.Publish(new ProcessOrder
        {
            OrderId = order.OrderId,
            CustomerNumber = order.CustomerNumber
        });

        return Results.Ok(new OrderInfoModel(order.OrderId, DateTime.UtcNow));
    })
    .WithName("ProcessOrder")
    .WithOpenApi();

app.Run();
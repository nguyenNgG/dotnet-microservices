using MassTransit;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;
using Play.Catalog.Service.Utilities;
using Play.Common.MongoDB;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var serviceSettings = builder.Configuration
    .GetSection(nameof(ServiceSettings))
    .Get<ServiceSettings>();

//// Add MongoDB services & do dependency injection
builder.Services.AddMongo().AddMongoRepository<Item>("items");

//// Add MassTransit RabbitMQ
builder.Services.AddMassTransit(
    x =>
        x.UsingRabbitMq(
            (context, configurator) =>
            {
                var rabbitMQSettings = builder.Configuration
                    .GetSection(nameof(RabbitMQSettings))
                    .Get<RabbitMQSettings>();
                configurator.Host(rabbitMQSettings?.Host);
                // Define the prefix for the queue
                configurator.ConfigureEndpoints(
                    context,
                    new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false)
                );
            }
        )
);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
    // transform /App-Entities to /app-entities
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

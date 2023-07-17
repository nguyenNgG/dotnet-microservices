using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryitems")
    .AddMongoRepository<CatalogItem>("catalogitems")
    .AddMassTransitWithRabbitMq();

AddCatalogClient(builder);

builder.Services.AddControllers();

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

static void AddCatalogClient(WebApplicationBuilder builder)
{
    Random jitterer = new Random();

    builder.Services
        .AddHttpClient<CatalogClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7147");
        })
        // Handle HTTP 50x, HTTP 408, network failures
        .AddTransientHttpErrorPolicy(
            policyBuilder =>
                policyBuilder
                    // Also retry if failed because of the timeout policy
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(
                        5,
                        // Exponential backoff: 2 ^ retryAttempt + random
                        retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                        onRetry: (outcome, timespan, retryAttempt) =>
                        {
                            var serviceProvider = builder.Services.BuildServiceProvider();
                            serviceProvider
                                .GetService<ILogger<CatalogClient>>()
                                ?.LogWarning(
                                    $"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}"
                                );
                        }
                    )
        )
        .AddTransientHttpErrorPolicy(
            policyBuilder =>
                policyBuilder
                    .Or<TimeoutRejectedException>()
                    .CircuitBreakerAsync(
                        3,
                        TimeSpan.FromSeconds(15),
                        onBreak: (outcome, timespan) =>
                        {
                            var serviceProvider = builder.Services.BuildServiceProvider();
                            serviceProvider
                                .GetService<ILogger<CatalogClient>>()
                                ?.LogWarning(
                                    $"Opening the circuit for {timespan.TotalSeconds} seconds..."
                                );
                        },
                        onReset: () =>
                        {
                            var serviceProvider = builder.Services.BuildServiceProvider();
                            serviceProvider
                                .GetService<ILogger<CatalogClient>>()
                                ?.LogWarning($"Closing the circuit...");
                        }
                    )
        )
        // Must be placed below other policies
        // Wait at most 1 second before timeout
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}

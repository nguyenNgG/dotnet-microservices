using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(
            this IServiceCollection services
        )
        {
            services.AddMassTransit(configuration =>
            {
                // Register message consumers
                // The entry assembly has the consumers defined
                configuration.AddConsumers(Assembly.GetEntryAssembly());

                configuration.UsingRabbitMq(
                    (context, configurator) =>
                    {
                        var configuration = context.GetService<IConfiguration>();
                        var serviceSettings = configuration
                            ?.GetSection(nameof(ServiceSettings))
                            .Get<ServiceSettings>();
                        var rabbitMQSettings = configuration
                            ?.GetSection(nameof(RabbitMQSettings))
                            .Get<RabbitMQSettings>();
                        configurator.Host(rabbitMQSettings?.Host);
                        // Define the prefix for the queue
                        configurator.ConfigureEndpoints(
                            context,
                            new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false)
                        );
                        // Configure retries if message is not able to be consumed by the consumer
                        configurator.UseMessageRetry(retryConfigurator =>
                        {
                            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                        });
                    }
                );
            });

            return services;
        }
    }
}

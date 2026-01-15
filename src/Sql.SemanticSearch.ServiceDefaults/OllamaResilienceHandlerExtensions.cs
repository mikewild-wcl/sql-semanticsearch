using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.ServiceDefaults;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Extension members don't need to be static")]
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This shared extension needs to be visible")]
public static class OllamaResilienceHandlerExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddOllamaResilienceHandler()
        {
            services.ConfigureHttpClientDefaults(http =>
            {
#pragma warning disable EXTEXP0001 // RemoveAllResilienceHandlers is experimental
                http.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

                // Turn on resilience by default
                http.AddStandardResilienceHandler(config =>
                {
                    // Extend the HTTP Client timeout for Ollama
                    config.AttemptTimeout.Timeout = TimeSpan.FromMinutes(3);

                    // Must be at least double the AttemptTimeout to pass options validation
                    config.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
                    config.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
                });

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            return services;
        }
    }
}
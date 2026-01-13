using DbUp;
using DbUp.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.DatabaseDeployment;
using Sql.SemanticSearch.Shared;
using System.Globalization;
using System.Reflection;

#pragma warning disable CA1848 // Use the LoggerMessage delegates

var builder = Host.CreateApplicationBuilder();
builder.AddServiceDefaults();

using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
var logger = loggerFactory.CreateLogger<Program>();

var aiProvider = EnvironmentVariables.DefaultAIProvider;
var embeddingDimensions = int.TryParse(builder.Configuration[ParameterNames.EmbeddingDimensions], out var dimensions) && dimensions > 0 ? dimensions : 1536;
var embeddingModel = builder.Configuration[ParameterNames.EmbeddingModel];
var ollamaEndpoint = builder.Configuration[EnvironmentVariables.OllamaTunnelEndpoint];

var connectionString = builder.Configuration.GetConnectionString(ResourceNames.SqlDatabase);

var serviceProvider = builder.Build().Services;

Dictionary<string, string> variables = new()
{
    { EnvironmentVariables.AIProvider, aiProvider.ToUpperInvariant() },
    { EnvironmentVariables.AIEndpoint, ollamaEndpoint! },
    //{ EnvironmentVariables.AIClientKey, Env.GetString("OPENAI_KEY")},
    { EnvironmentVariables.EmbeddingModel, embeddingModel! },
    { EnvironmentVariables.EmbeddingDimensions, embeddingDimensions.ToString("D", CultureInfo.InvariantCulture)}
};

logger.LogInformation("Starting deployment...");
var dbup = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithVariables(variables)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .AddLoggerFromServiceProvider(serviceProvider)
    .Build();

var result = dbup.PerformUpgrade();

if (!result.Successful)
{
    logger.LogError(result.Error, "Deployment failed.");
    return -1;
}

logger.LogInformation("Deployed successfully!");
#pragma warning restore CA1848

return 0;

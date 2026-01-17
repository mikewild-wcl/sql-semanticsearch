using DbUp;
using DbUp.Extensions.Logging;
using DbUp.Helpers;
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
var externalEbeddingModel = builder.Configuration[ParameterNames.SqlServerExternalEmbeddingModel];

var ollamaEndpoint = builder.Configuration[EnvironmentVariables.OllamaTunnelEndpoint];
var ollamaUri = new Uri(new Uri(ollamaEndpoint!), "api/embed"); /* SQL Server uses api/embed */

var connectionString = builder.Configuration.GetConnectionString(ResourceNames.SqlDatabase);

var serviceProvider = builder.Build().Services;

Dictionary<string, string> variables = new()
{
    { EnvironmentVariables.AIProvider, aiProvider.ToUpperInvariant() },
    { EnvironmentVariables.AIEndpoint, ollamaUri.AbsoluteUri! },
    //{ EnvironmentVariables.AIClientKey, Env.GetString("OPENAI_KEY")},
    { EnvironmentVariables.EmbeddingModel, embeddingModel! },
    { EnvironmentVariables.EmbeddingDimensions, embeddingDimensions.ToString("D", CultureInfo.InvariantCulture) },
    { EnvironmentVariables.ExternalEmbeddingModel, externalEbeddingModel! }
};

logger.LogInformation("Starting deployment...");
var result = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithVariables(variables)
    .WithScriptsEmbeddedInAssembly(
            typeof(Program).Assembly,
            f => !f.Contains("always-run.", StringComparison.InvariantCultureIgnoreCase))
    .WithTransaction()
    .AddLoggerFromServiceProvider(serviceProvider)
    .Build()
    .PerformUpgrade();

if (result.Successful)
{
    result = DeployChanges.To
        .SqlDatabase(connectionString)
        .WithVariables(variables)
        .WithScriptsEmbeddedInAssembly(
            typeof(Program).Assembly,
            f => f.Contains("always-run.", StringComparison.InvariantCultureIgnoreCase))
        .JournalTo(new NullJournal())
        .WithTransaction()
        .AddLoggerFromServiceProvider(serviceProvider)
        .Build()
        .PerformUpgrade();
}

if (!result.Successful)
{
    logger.LogError(result.Error, "Deployment failed.");
    return -1;
}

logger.LogInformation("Deployed successfully!");
#pragma warning restore CA1848

return 0;

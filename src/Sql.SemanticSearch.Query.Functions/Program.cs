using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Search.Interfaces;
using Sql.SemanticSearch.ServiceDefaults;
using Sql.SemanticSearch.Shared;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var aiSettings = builder.Configuration.GetAISettings();

if (string.Equals(aiSettings.Provider, "OLLAMA", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOllamaResilienceHandler();
}

builder.ConfigureFunctionsWebApplication();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);

builder.Services
    .AddSingleton(aiSettings)
    .AddTransient<IDatabaseConnection, DapperConnection>()
    .AddTransient<ISearchService, SearchService>();

await builder.Build().RunAsync().ConfigureAwait(true);


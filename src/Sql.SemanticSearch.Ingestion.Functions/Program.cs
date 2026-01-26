using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sql.SemanticSearch.Core.ArXiv;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.ServiceDefaults;
using Sql.SemanticSearch.Shared;
using System.Data;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var aiSettings = builder.Configuration.GetAISettings();

if (string.Equals(aiSettings.Provider, "OLLAMA", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOllamaResilienceHandler();
}

builder.ConfigureFunctionsWebApplication();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);
builder.Services.AddSingleton(new Func<IDbConnection>(() => 
    new SqlConnection(builder.Configuration.GetConnectionString(ResourceNames.SqlDatabase))));

builder.Services
    .AddSingleton(aiSettings)
    .AddTransient<IDatabaseConnection, DapperConnection>()
    .AddTransient<IIngestionService, IngestionService>();

builder.Services.AddHttpClient<IArxivApiClient, ArxivApiClient>(client =>
{
    client.BaseAddress = new("http://export.arxiv.org/api/");
});

await builder.Build().RunAsync().ConfigureAwait(true);

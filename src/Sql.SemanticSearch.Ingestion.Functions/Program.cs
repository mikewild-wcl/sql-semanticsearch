using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sql.SemanticSearch.Core.ArXiv;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Shared;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);

builder.Services.AddTransient<IIngestionService, IngestionService>();

builder.Services.AddHttpClient<IArxivApiClient, ArxivApiClient>(client =>
{
    client.BaseAddress = new("http://export.arxiv.org/api/");
});

await builder.Build().RunAsync().ConfigureAwait(true);

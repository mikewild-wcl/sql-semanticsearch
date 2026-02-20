using Microsoft.Data.SqlClient;
using Sql.SemanticSearch.Api.Endpoints;
using Sql.SemanticSearch.Api.Mcp;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Search.Interfaces;
using Sql.SemanticSearch.ServiceDefaults;
using Sql.SemanticSearch.Shared;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var aiSettings = builder.Configuration.GetAISettings();
var useOllamaDefaults = string.Equals(aiSettings.Provider, "OLLAMA", StringComparison.OrdinalIgnoreCase);

builder.AddServiceDefaults(useOllamaDefaults);
builder.AddSqlServerResiliencePipeline();

builder.Services.AddOpenApi();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);
builder.Services.AddSingleton(new Func<IDbConnection>(() =>
    new SqlConnection(builder.Configuration.GetConnectionString(ResourceNames.SqlDatabase))));
TypeHandlerRegistry.RegisterHandlers(); // Dapper handlers for mapping data back from database

builder.Services
    .AddSingleton(aiSettings)
    .AddTransient<IDatabaseConnection, DapperConnection>()
    .AddTransient<ISearchService, SearchService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<SearchTools>()
    .WithPrompts<SearchPrompts>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapSearchEndpoints();

app.MapMcp();

await app.RunAsync().ConfigureAwait(true);

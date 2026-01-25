using Sql.SemanticSearch.Api.Endpoints;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Search.Interfaces;
using Sql.SemanticSearch.ServiceDefaults;
using Sql.SemanticSearch.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var aiSettings = builder.Configuration.GetAISettings();

if (string.Equals(aiSettings.Provider, "OLLAMA", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOllamaResilienceHandler();
}

builder.Services.AddOpenApi();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);
TypeHandlerRegistry.RegisterHandlers(); // Dapper handlers for mapping data back from database

builder.Services
    .AddSingleton(aiSettings)
    .AddTransient<IDatabaseConnection, DapperConnection>()
    .AddTransient<ISearchService, SearchService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapSearchEndpoints();


await app.RunAsync().ConfigureAwait(true);

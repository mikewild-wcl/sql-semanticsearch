using Microsoft.AspNetCore.Mvc;
using Sql.SemanticSearch.Api.Endpoints;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Search.Interfaces;
using Sql.SemanticSearch.ServiceDefaults;
using Sql.SemanticSearch.Shared;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var aiSettings = builder.Configuration.GetAISettings();

if (string.Equals(aiSettings.Provider, "OLLAMA", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOllamaResilienceHandler();
}

builder.Services.AddOpenApi();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);

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
//app.MapPost("/api/search", 
//    async (
//        [Description("Search for documents related to a user prompt.")]
//        [FromBody] SearchRequest searchRequest,
//        ISearchService searchService) =>
//        {
//            var results = await searchService.Search(searchRequest);
//            return Results.Ok(results);
//        })
//    .WithSummary("Post a search prompt.")
//    .WithDescription("This endpoint handles posted search requests and returns a response.")
//    .WithTags("Search");

app.MapGet("/weatherforecast", () =>
{
#pragma warning disable CA5394 // Do not use insecure randomness
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
#pragma warning restore CA5394 // Do not use insecure randomness
    return forecast;
})
.WithName("GetWeatherForecast");

await app.RunAsync().ConfigureAwait(true);

#pragma warning disable S3903 // Types should be defined in named namespaces
sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
#pragma warning restore S3903 // Types should be defined in named namespaces
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

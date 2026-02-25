using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sql.SemanticSearch.Core.ArXiv;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Chunking;
using Sql.SemanticSearch.Core.Chunking.Interfaces;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.ServiceDefaults;
using Sql.SemanticSearch.Shared;
using System.Data;

var builder = FunctionsApplication.CreateBuilder(args);

var aiSettings = builder.Configuration.GetAISettings();
var useOllamaDefaults = string.Equals(aiSettings.Provider, "OLLAMA", StringComparison.OrdinalIgnoreCase);

builder.AddServiceDefaults(useOllamaDefaults);

builder.AddSqlServerResiliencePipeline();

builder.ConfigureFunctionsWebApplication();

builder.AddSqlServerClient(connectionName: ResourceNames.SqlDatabase);
builder.Services.AddSingleton(new Func<IDbConnection>(() => 
    new SqlConnection(builder.Configuration.GetConnectionString(ResourceNames.SqlDatabase))));

builder.Services.AddSingleton(_ =>
{
    var markItDownMcpUrl = $"{Environment.GetEnvironmentVariable(EnvironmentVariableNames.MarkitdownMcpUri)}/mcp";
    var uri = new Uri(markItDownMcpUrl);
    return new MarkItDownMcpReader(uri);
});

builder.Services.AddTransient<IDocumentReader, PdfDocumentReader>();
/*
//TODO: Consider a factory here:
services.AddSingleton<Func<string, IDocumentReader>>(sp => key =>
    key switch
    {
        DocumentType.Pdf => _serviceProvider.GetRequiredService<PdfDocumentReader>(),
        DocumentType.Docx => _serviceProvider.GetRequiredService<DocxDocumentReader>(),
        DocumentType.md => _serviceProvider.GetRequiredService<MarkdownDocumentReader>(),
        _ => throw new ArgumentException("Invalid document type", nameof(documentType))
    });
*/

builder.Services
    .AddSingleton(aiSettings)
    .AddTransient<IDatabaseConnection, DapperConnection>()
    .AddTransient<IDocumentChunkingService, DocumentChunkingService>()
    .AddTransient<IngestionChunkWriter<string>, SqlServerChunkWriter>()
    .AddTransient<IIngestionService, IngestionService>();

builder.Services.AddHttpClient<IArxivApiClient, ArxivApiClient>(client =>
{
    client.BaseAddress = new("http://export.arxiv.org/api/");
});

await builder.Build().RunAsync().ConfigureAwait(true);

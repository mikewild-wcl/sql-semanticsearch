using Scalar.Aspire;
using Sql.SemanticSearch.AppHost.Extensions;
using Sql.SemanticSearch.AppHost.ParameterDefaults;
using Sql.SemanticSearch.Shared;
using System.Collections.Generic;

var builder = DistributedApplication.CreateBuilder(args);

var aiProviderParameter = builder.AddParameter(ParameterNames.AIProvider);
var embeddingModelParameter = builder.AddParameter(ParameterNames.EmbeddingModel);
var embeddingDimensionsParameter = builder.AddParameter(ParameterNames.EmbeddingDimensions);
var sqlServerExternalEmbeddingModelParameter = builder.AddParameter(ParameterNames.SqlServerExternalEmbeddingModel);
var gpuVendorParameter = builder.AddParameter(ParameterNames.GpuVendor, value: new BooleanParameterDefault(false));

var sqlServerPortParameter = builder.AddParameter(ParameterNames.SqlServerPort, value: new EmptyParameterDefault());
var sqlPasswordParameter = builder.AddParameter(ParameterNames.SqlServerPassword, value: new EmptyParameterDefault(), secret: true);

var ollama = builder.AddOllama(ResourceNames.Ollama)
    .WithGPUSupportIfAvailable(gpuVendorParameter)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

ollama.AddModel(ResourceNames.Embeddings, embeddingModelParameter.GetValue()!);

var devTunnel = builder.AddDevTunnel(ResourceNames.OllamaTunnel)
   .WithReference(ollama)
   .WithAnonymousAccess()
   .WaitFor(ollama);

var sqlServer = builder.AddSqlServer(ResourceNames.SqlServer)
    .WithImage("mssql/server", "2025-latest")
    .WithHostPortAndEndpointIfProvided(ResourceNames.SqlServerEndpoint, sqlServerPortParameter)
    .WithDataVolume()
    .WithPassword(sqlPasswordParameter)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase(ResourceNames.SqlDatabase, DatabaseNames.DocumentsDatabase);

var databaseDeployment = builder.AddProject<Projects.DatabaseDeployment>(ResourceNames.DatabaseDeployment)
    .WithReference(sqlServer)
    .WithReference(ollama, devTunnel)
    .WithEnvironment(ParameterNames.AIProvider, aiProviderParameter)
    .WithEnvironment(ParameterNames.EmbeddingDimensions, embeddingDimensionsParameter)
    .WithEnvironment(ParameterNames.EmbeddingModel, embeddingModelParameter)
    .WithEnvironment(ParameterNames.SqlServerExternalEmbeddingModel, sqlServerExternalEmbeddingModelParameter)
    .WaitFor(devTunnel)
    .WaitFor(sqlServer);

var ingestionFunctions = builder.AddAzureFunctionsProject<Projects.IngestionFunctions>(ResourceNames.IngestionFunctions)
    .WithReference(sqlServer)
    .WithEnvironment(ParameterNames.AIProvider, aiProviderParameter)
    .WithEnvironment(ParameterNames.SqlServerExternalEmbeddingModel, sqlServerExternalEmbeddingModelParameter)
    .WaitForCompletion(databaseDeployment);

var api = builder.AddProject<Projects.Api>(ResourceNames.Api)
    .WithReference(sqlServer)
    .WithEnvironment(ParameterNames.AIProvider, aiProviderParameter)
    .WithEnvironment(ParameterNames.EmbeddingDimensions, embeddingDimensionsParameter)
    .WithEnvironment(ParameterNames.SqlServerExternalEmbeddingModel, sqlServerExternalEmbeddingModelParameter)
    .WaitForCompletion(databaseDeployment);

builder.AddScalarApiReference(options =>
{
    options
        .PreferHttpsEndpoint()
        .AllowSelfSignedCertificates();

    // Attempt at fix for CORS errors in functions app when access <uri>api/swagger/ui
    // https://github.com/dotnet/aspire/discussions/6989
    var proxyUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    if (!string.IsNullOrEmpty(proxyUrls))
    {
        options.Servers = [.. 
            proxyUrls
                .Split(';')
                .Select(x => new ScalarServer(x)) ];
    }
})
    .WithApiReference(api)
    .WithApiReference(ingestionFunctions);

await builder.Build().RunAsync().ConfigureAwait(true);

using Sql.SemanticSearch.AppHost.Extensions;
using Sql.SemanticSearch.AppHost.ParameterDefaults;
using Sql.SemanticSearch.Shared;

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

builder.AddAzureFunctionsProject<Projects.IngestionFunctions>(ResourceNames.IngestionFunctions)
    .WithReference(sqlServer)
    .WithEnvironment(ParameterNames.AIProvider, aiProviderParameter)
    .WithEnvironment(ParameterNames.SqlServerExternalEmbeddingModel, sqlServerExternalEmbeddingModelParameter)
    .WaitForCompletion(databaseDeployment);

builder.AddAzureFunctionsProject<Projects.SemanticSearchFunctions>(ResourceNames.QueryFunctions)
    .WithReference(sqlServer)
    .WithEnvironment(ParameterNames.AIProvider, aiProviderParameter)
    .WithEnvironment(ParameterNames.SqlServerExternalEmbeddingModel, sqlServerExternalEmbeddingModelParameter)
    .WithEnvironment(ParameterNames.EmbeddingDimensions, embeddingDimensionsParameter)
    .WaitForCompletion(databaseDeployment);

await builder.Build().RunAsync().ConfigureAwait(true);

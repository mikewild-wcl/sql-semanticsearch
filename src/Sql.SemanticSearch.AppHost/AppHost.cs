using Sql.SemanticSearch.AppHost.Extensions;
using Sql.SemanticSearch.AppHost.ParameterDefaults;
using Sql.SemanticSearch.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var embeddingModelParameter = builder.AddParameter(ParameterNames.EmbeddingModel);
var embeddingDimensionsParameter = builder.AddParameter(ParameterNames.EmbeddingDimensions);
var sqlServerPortParameter = builder.AddParameter(ParameterNames.SqlServerPort, value: new EmptyParameterDefault());
var sqlPasswordParameter = builder.AddParameter(ParameterNames.SqlServerPassword, value: new EmptyParameterDefault(), secret: true);
var gpuVendorParameter = builder.AddParameter(ParameterNames.GpuVendor, value: new BooleanParameterDefault(false));

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
    .WithEnvironment(ParameterNames.EmbeddingDimensions, embeddingDimensionsParameter)
    .WithEnvironment(ParameterNames.EmbeddingModel, embeddingModelParameter)
    .WaitFor(devTunnel)
    .WaitFor(sqlServer);

builder.AddAzureFunctionsProject<Projects.SemanticFunctions>(ResourceNames.SemanticFunctions)
    .WithReference(sqlServer)
    .WaitForCompletion(databaseDeployment);

await builder.Build().RunAsync().ConfigureAwait(true);

# Aspire

In my last article I showed a project that used an Azure function, an ASP.NET Web API, an LLM embedding model hosted by Ollama, and a dev tunnel that exposed Ollama over https. There's a lot of plumbing there, but it was pretty easy to put together using Aspire.

- SQL Server database
- an Azure function
- an ASP.NET Web API
- an LLM embedding model hosted by Ollama
- a dev tunnel that exposed Ollama over https
- Scalar, an API Client for OpenAPI
 
- There's a lot of plumbing involved, but it was pretty easy to put together using Aspire.





???
The API calls here mean we aren't completely local-first, but that's fine for this initial version. In future a workaround can be created, e.g. a fake api that provides test data.
I've included a script that drops all the tables in the repo - sql/clean_documents_database.sql.
???

## Prerequisites

The application is written for .NET 10 so that's a definite prerequisite. Everything here should work on non-Windows machines, but I haven't tested it.

A container solution such as Docker or Podman must be installed on your machine. I use Docker Desktop on Windows.

## Creating the basic application

Create an empty Aspire application. I like to use centrally managed nuget packages, add a shared project with constants so I can remove "magic strings" and set up Directory.build.props but those are all optional. I won't go into detail in this article.

## Aspire AppHost

We'll need a few nuget packages for Azure functions, SQL Server, Ollama, and DevTunnels - we'll need this last one so that SQL Server can talk to Ollama over https. I've also added Scalar to 

```
Aspire.Hosting.Azure.Functions
Aspire.Hosting.DevTunnels
Aspire.Hosting.SqlServer
CommunityToolkit.Aspire.Hosting.Ollama
Scalar.Aspire
```

I've created a shared project which has constants that can be used in place of the magic strings that Aspire templates have given us, and set shortened project names to simplify the AppHost code. Note the additional Aspire attributes in my project file, `AspireProjectMetadataTypeName` to change the names and `IsAspireProjectResource` for the Shared class library project:
```
  <ItemGroup>
    <ProjectReference Include="..\Sql.SemanticSearch.Api\Sql.SemanticSearch.Api.csproj" AspireProjectMetadataTypeName="Api" />
    <ProjectReference Include="..\Sql.SemanticSearch.DatabaseDeployment\Sql.SemanticSearch.DatabaseDeployment.csproj" AspireProjectMetadataTypeName="DatabaseDeployment" />
    <ProjectReference Include="..\Sql.SemanticSearch.Ingestion.Functions\Sql.SemanticSearch.Ingestion.Functions.csproj" AspireProjectMetadataTypeName="IngestionFunctions" />
    <ProjectReference Include="..\Sql.SemanticSearch.Shared\Sql.SemanticSearch.Shared.csproj" IsAspireProjectResource="false" />
  </ItemGroup>
```
  
Parameters are defined in `appsettings.json` and can be overridden in user secrets. :
```
"Parameters": {
  "AIProvider": "Ollama",
  "EmbeddingModel": "nomic-embed-text",
  "EmbeddingDimensions": 768,
  "SqlServerPort": "",
  "SqlServerPassword": "",
  "OllamaGpuVendor": "",
  "SqlServerExternalEmbeddingModel": "SemanticSearchEmbeddingModel"
},
```

The SQL Server port and password are useful if you want a connection string for SSMS - if you don't provide them then Aspire will generate different values every time.

  - Ollama with optional GPU support - this is controlled with a parameter which needs to be added to your secrets. It defaults to false.
  - The embedding model name and dimensionality is defined in the parameters - we are using `nomic-embed-text`. The number of dimensions needs to be set so we can set up the database correctly.
  - An external model will be created in SQL Server so the correct name needs to be provided in the parameters.
  - SQL Server. This has a persistent lifetime and a data volume so it we don't need to set it up every time. Note the image has to be set because the default in Aspire is sql-2022 - this will no doubt be fixed in a future release. There are parameters for a default port and password; if provided this makes it easier to query the database from Sql Management Studio because the connection string won't change.

Here's the code:

```
using Scalar.Aspire;
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
})
    .WithApiReference(api);

await builder.Build().RunAsync().ConfigureAwait(true);
```

# Ollama

Ollama is an open-source framework that hosts large language models (LLMs) locally. Aspire creates the Ollama instance and we add the embedding model to it.

A future version might change to use Azure OpenAI, but for now Ollama keeps things simple.

## Dev tunnels


## Database deployment

### DbUp

The `Sql.SemanticSearch.DatabaseDeployment` uses **DbUp** to deploy the database from scripts embedded into the assembly.

Note for future changes: https://elanderson.net/2020/08/always-run-migrations-with-dbup/

It sets up some variables for use in the scripts:
```
Dictionary<string, string> variables = new()
{
    { EnvironmentVariables.AIProvider, aiProvider.ToUpperInvariant() },
    { EnvironmentVariables.AIEndpoint, endpoint },
    //{ EnvironmentVariables.AIClientKey, Env.GetString("OPENAI_KEY")},
    { EnvironmentVariables.EmbeddingModel, embeddingModel! },
    { EnvironmentVariables.EmbeddingDimensions, embeddingDimensions.ToString("D", CultureInfo.InvariantCulture) },
    { EnvironmentVariables.ExternalEmbeddingModel, externalEmbeddingModel! }
};
```

It then deploys in three stages: 
1. scripts that have "server-configuration" in the name. These scripts fail if run inside a transaction so I've separated them.
2. The main deployment scripts that create tables, schemas, seed data etc.
3. scripts with "always-run" in the name. These will be run on every deployment as they aren't added to the deployment state in the database (`.JournalTo(new NullJournal())`). I needed this because the dev tunnel port changed unexpectedly, so I decided to recreate the Ollama model if details had changed..

```
const string AlwaysRunTag = "always-run";
const string ServerConfigurationTag = "server-configuration";
```

```
var result = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithVariables(variables)
    .WithScriptsEmbeddedInAssembly(
            typeof(Program).Assembly,
            /* Server configuration scripts that have to run outside of a transaction */
            f => f.Contains(ServerConfigurationTag, StringComparison.InvariantCultureIgnoreCase))
    .AddLoggerFromServiceProvider(serviceProvider)
    .Build()
    .PerformUpgrade();

if (result.Successful)
{
    result = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithVariables(variables)
    .WithScriptsEmbeddedInAssembly(
            typeof(Program).Assembly,
            f => !f.Contains(AlwaysRunTag, StringComparison.InvariantCultureIgnoreCase))
    .WithTransaction()
    .AddLoggerFromServiceProvider(serviceProvider)
    .Build()
    .PerformUpgrade();
}

if (result.Successful)
{
    result = DeployChanges.To
        .SqlDatabase(connectionString)
        .WithVariables(variables)
        .WithScriptsEmbeddedInAssembly(
            typeof(Program).Assembly,
            f => f.Contains(AlwaysRunTag, StringComparison.InvariantCultureIgnoreCase))
        .JournalTo(new NullJournal())
        .WithTransaction()
        .AddLoggerFromServiceProvider(serviceProvider)
        .Build()
        .PerformUpgrade();
}
```


## Function app

Add a function app to the solution. If you select the checkbox for enrolling in the Aspire orchestration it should add everything, but it didn't work when I did it. I had to add a reference to the ServiceDefaults project and call builder.AddServiceDefaults() in Program.cs, then remove the app insights because it will be managed via service defaults:
```csharp
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
```


## API project

This is a standard ASP.NET Web API project.

## Scalar
I added `Scalar.Aspire` to the AppHost. Scalar provides a front end for OpenAPI and replaces Swagger. I've only added Scalar for the API project because I couldn't get it working for the functions app. 



## Fake arXiv API

This solution isn't completely "local only" because it calls the arXiv API. I think that's reasonable, but in some projects I've added another project that works as a fake API that returns data for local testing. It can work well when you want to avoid making expensive or slow calls to external APIs. 


**Screenshot**
![Application host running in a browser.](./images/aspire_screenshot.png)


## Conclusion

### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)


## References

[Dev Tunnels integration](https://aspire.dev/integrations/devtools/dev-tunnels/)
[Scalar API Reference for .NET Aspire](https://scalar.com/products/api-references/integrations/aspire)

[Ollama integration](https://aspire.dev/integrations/ai/ollama/)
[Get started with the SQL Server Entity Framework Core integrations](https://aspire.dev/integrations/databases/efcore/sql-server/sql-server-get-started/)
[]()
[]()

SQL Server integration

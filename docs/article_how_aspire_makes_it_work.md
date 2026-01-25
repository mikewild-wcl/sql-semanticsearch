# Aspire


## Overview

- Aspire AppHost and ServiceExtensons
  - Ollama for simplicity and local development  
  - SQL Server in Docker
  - DBUp for deployment
  - Azure functions as the work layer 

## Creating the basic application

Create an empty Aspire application. I used my own template to set this up with some default files.

Because this uses ManagePackageVersionsCentrally, you might need to update the references in projects that you add - make sure they are in `Directory.Packages.props` and remove the version from the `.csproj' files.

Add a reference to the ServiceDefaults project and call builder.AddServiceDefaults() in Program.cs.

Remove app insights because it will be managed via service defaults:
```csharp
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
```

When adding projects you can remove these lines from the `'csproj` files because they are in Directory.build.props:
```csharp
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
```

## Aspire AppHost

We'll need a few nuget packages for Azure functions, SQL Server, Ollama, and DevTunnels - we'll need this last one so that SQL Server can talk to Ollama over https.
```
Aspire.Hosting.Azure.Functions
Aspire.Hosting.DevTunnels
Aspire.Hosting.SqlServer
CommunityToolkit.Aspire.Hosting.Ollama
```

I've created a shared project which has constants that can be used in place of the magic strings that Aspire templates have given us, and set shortened project names to siumplify the AppHost code. Note the additional Aspire attributs in my project file:
```
<ProjectReference Include="..\Sql.SemanticSearch.Ingestion.Functions\Sql.SemanticSearch.Ingestion.Functions.csproj" AspireProjectMetadataTypeName="SemanticFunctions" />
<ProjectReference Include="..\Sql.SemanticSearch.Shared\Sql.SemanticSearch.Shared.csproj" IsAspireProjectResource="false" />
```

Set up
  - Ollama with optional GPU support - this is controlled with a parameter which needs to be added to your secrets. It defaults to false.
  - The embedding model name is defined in the parameters - we are using nomic-embed-text. The number of dimensions needs to be set so we can set up the database correctly.
  - An external model will be created in SQL Server so the correct name needs to be provided in the parameters.
  - SQL Server. This has a persistent lifetime and a data volume so it we don't need to set it up every time. Note the image has to be set because the default in Aspire is sql-2022 - this will no doubt be fixed in a future release. There are parameters for a default port and password; if provided this makes it easier to query the database from Sql Management Studio because the connection string won't change.
  
Parameters:
```
"Parameters": {
  "EmbeddingModel": "nomic-embed-text",
  "EmbeddingDimensions": 768,
  "SqlServerExternalEmbeddingModel": "SemanticSearchOllamaEmbeddingModel"
  "SqlServerPort": "",
  "SqlServerPassword": "",
  "OllamaGpuVendor": ""
},
```

## Database deployment

### DbUp
Note for future changes: https://elanderson.net/2020/08/always-run-migrations-with-dbup/

## Fake arXiv API
Last time I mentioned that we aren't completely local-first because the functions call an exteral API to get metadata and (in future) download files. We can work around this by setting up a local test fake version of the API. That's pretty easy so let's do that now.

## API project

Added `Scalar.Aspire` to the AppHost to provide a front end for OpenAPI.
```
using Scalar.Aspire;
...
builder.AddScalarApiReference()
    .WithApiReference(api);
```

### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)

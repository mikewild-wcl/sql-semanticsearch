# Sql.SemanticSearch

Sql.SemanticSearch is a library that enables semantic search capabilities within SQL databases. 
It leverages advanced natural language processing techniques to enhance search functionality, allowing users to find relevant 
information based on meaning rather than just keywords.

Uses SQL Server 2025 and Azure SQL vector capabilities to ingest and embed documents.

## Project structure

sql-semanticsearch/
├── Sql.Vector.SemanticSearch.AppHost
├── Sql.SemanticSearch.Api
├── Sql.Vector.SemanticSearch.Core
├── Sql.Vector.SemanticSearch.DatabaseDeploymentService
├── Sql.Vector.SemanticSearch.Ingestion.Functions
├── Sql.Vector.SemanticSearch.Query.Functions
├── Sql.Vector.SemanticSearch.Shared
│   Tests
│   ├── Sql.SemanticSearch.Ingestion.Functions.UnitTests
│   ├── Sql.SemanticSearch.Query.Functions.UnitTests
│   ├── Sql.Vector.Embeddings.SemanticSearch.Core.UnitTests
│   ├── Sql.SemanticSearch.Tests.Helpers
├── .editorconfig
├── .gitignore
├── README.md
└── (other files and directories, e.g., user secrets, settings, and external SQL scripts)

 - AppHost - Console app to host the function app locally for testing
 - FunctionApp - Azure function app to ingest documents
 - DatabaseDeploymentService - DbUp deployment of the SQL database schema and external models
 - Data - Data classes
 - Ingestion.Core - ingestion application classes

## SQL database

If you want to connect to the database with SSMS you can get the connection string in the Aspire portal and paste it into the connection dialog.

Optionally you can set the SQL Server port and sa password by adding these parameters to your secrets. If the parameters aren't set then default values will be generated.
```
  "Parameters": {
    "SqlServerPassword": "<password>",
    "SqlServerPort": 14331
  }
```

The embedding model is created in the database deployment scripts. The external model name is defined in the AppHost parameters as
```
  "Parameters": {
    "SqlServerExternalEmbeddingModel": "SemanticSearchOllamaEmbeddingModel"
  }
```

## Call function app

The function app expects an array called `uris` with an array of uris pointing to pdf files. 

There is a test http script in the functions folder that can be used to call it. Alternatively call from a command line using curl:
```
curl -X POST http://localhost:7131/api/index-documents/ -H "Content-Type: application/json" -d '{"ids": ["1409.0473", "2510.04950" ] }'
```

## Aspire hosting and deployment

 - See [Function integration](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-aspire-integration).

## AI model clients

The AI model provider needs to be set in the AppHost parameters. The options are `Ollama` or `OpenAI`.
```
  "Parameters": {
    "AIModelProvider": "Ollama"
  }
```

If you are using Ollama and have a GPU, include a parameter `OllamaGpuVendor` in the AppHost appsettings or secrets. The value can be `Nvidia` or `AMD` (or any future values from `Aspire.Hosting.OllamaGpuVendor`).
This is added via an extension `WithGPUSupportIfVendorParameterProvided` and should match your system.
```
  "Parameters": {
    "OllamaGpuVendor": "Nvidia"
  }
```

## ArXiv data

The ingestion function uses the [arXiv API](https://info.arxiv.org/help/api/index.html) to load document metadata and then to download documents.
Thank you to arXiv for use of its open access interoperability.

## API

The API project contains a simple web api to query the database. It has a single endpoint `/search` that takes a query string parameter and returns the top 10 results from the database.

The API uses Scalar to expose OpenAPI information.

Scalar has been added to the AppHost - see [Scalar API Reference for .NET Aspire](https://scalar.com/products/api-references/integrations/aspire).

OpenApi has been implemented for the web application and enabled when running in development mode.
To see the OpenApi specification browse to https://localhost:7185/openapi/v1.json

Scalar has also been included, and can be used to test the API at https://<uri>>:<port>>/scalar/v1. 




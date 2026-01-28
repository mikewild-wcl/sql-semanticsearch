# Sql.SemanticSearch

Sql.SemanticSearch is a library that enables semantic search capabilities within SQL databases. 
It leverages advanced natural language processing techniques to enhance search functionality, allowing users to find relevant 
information based on meaning rather than just keywords.

Uses SQL Server 2025 and Azure SQL vector capabilities to ingest and embed documents.

## Project structure

sql-semanticsearch/
├── Sql.SemanticSearch.AppHost        # Aspire orchestrator for local development
├── Sql.SemanticSearch.Api            # ASP.NET minimal API for search queries
├── Sql.SemanticSearch.Core           # Core library with shared models and services
├── Sql.SemanticSearch.DatabaseDeployment  # DbUp deployment of SQL schema and external models
├── Sql.SemanticSearch.Ingestion.Functions # Azure Functions for document ingestion
├── Sql.SemanticSearch.ServiceDefaults     # Shared Aspire service configuration
├── Sql.SemanticSearch.Shared              # Shared utilities
├── .editorconfig
├── .gitignore
├── README.md
└── docs/                             # Documentation and articles

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

## Call function app and API

The ingestion function accepts a list of arXiv document IDs. The function fetches metadata from the arXiv API, saves documents to SQL Server, and generates vector embeddings using `AI_GENERATE_EMBEDDINGS`.

There is a test script `Functions.http` in the functions folder that can be used to call it. If you have a lot of requests the call could timeout easily, so I recommend increasing the timeout. In Visual Studio go to Tools..Options, search for `REST advanced` and you will see a timeout - set to something more than the default 20 seconds.
![Where to set REST request timeout.](./images/vs_rest_timeout.png)

Alternatively call from a command line using curl.
```
curl -X POST http://localhost:7131/api/index-documents/ -H "Content-Type: application/json" -d '{"ids": ["1409.0473", "2510.04950"]}'
```

> [!Tip]
> Console script for extracting arXiv links from a page - `console.log(Array.from($$("a")).filter(url => url.href.toLowerCase().includes("/arxiv.org/")).map(url => url.href).join(" "));` 
> Console script for extracting arXiv ids from a page - `console.log(Array.from($$("a")).filter(url => url.href.toLowerCase().includes("/arxiv.org/")).map(url => url.href).map(href => { const match = href.match(/\/\/arxiv\.org\/pdf\/(.+?)\.pdf(?:$|[?#])/i); return match?.[1] ?? null; }).filter(Boolean).join(" "));` 

To call the API use the script in `Sql.SemanticSearch.Api.http` or call from curl:
```
curl -X POST https://sql-semanticsearch-api-sql_semanticsearch.dev.localhost:7253/api/search -H "Content-Type: application/json" -d '{"query": "Find papers on Gen AI", "top_k": 5}'
```

## arXiv ids

Valid arxiv ids look like `1409.0473` or `hep-th/9901001` (pre-2007). For more see [Understanding the arXiv identifier](https://info.arxiv.org/help/arxiv_identifier.html).
There is minimal validation and de-duplication of the ids in the code. I added a regex:
``` csharp
var arxivRegex = new Regex(
    @"^(?:\d{4}\.\d{4,5}|[a-z\-]+(?:\.[A-Z]{2})?/\d{7})(?:v\d+)?$",
    RegexOptions.IgnoreCase);
```
Breakdown:
```
^
(?:                              # Either:
  \d{4}\.\d{4,5}                  #   New-style: YYMM.NNNN or YYMM.NNNNN
  |                               #   OR
  [a-z\-]+(?:\.[A-Z]{2})?/\d{7}   #   Old-style: archive(.SUB)/YYMMNNN
)
(?:v\d+)?                         # Optional version suffix
$
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

The API project is an ASP.NET Core minimal API for search queries. It uses `AI_GENERATE_EMBEDDINGS` to create an embedding for the search query, then compares it against stored embeddings using `VECTOR_DISTANCE` (cosine distance).

The `/api/search` endpoint accepts a POST request with a query and returns the top results from the database.

Scalar has been added to the AppHost for OpenAPI documentation - see [Scalar API Reference for .NET Aspire](https://scalar.com/products/api-references/integrations/aspire).

OpenAPI is enabled when running in development mode. Scalar can be used to test the API at `https://<uri>:<port>/scalar/v1`.

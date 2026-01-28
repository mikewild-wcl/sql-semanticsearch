# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Sql.SemanticSearch is a library that enables semantic search capabilities within SQL databases using SQL Server 2025 and Azure SQL vector capabilities for document ingestion and embedding. The project uses .NET Aspire for orchestration and Azure Functions for document processing.

## Architecture

The solution consists of these main projects:

- **Sql.SemanticSearch.AppHost**: Aspire orchestrator that coordinates all services including SQL Server, Ollama, the ingestion function, and search API.

- **Sql.SemanticSearch.ServiceDefaults**: Shared library providing common Aspire service configuration (OpenTelemetry, service discovery, resilience, health checks). Contains `AddServiceDefaults()` extension method.

- **Sql.SemanticSearch.Ingestion.Functions**: Azure Functions v4 app for document ingestion. Uses isolated worker model with ASP.NET Core integration. Contains `IndexArxivDocuments` HTTP-triggered function that accepts arXiv document IDs, fetches metadata from arXiv API, saves to SQL Server, and generates embeddings using `AI_GENERATE_EMBEDDINGS`.

- **Sql.SemanticSearch.Api**: ASP.NET Core minimal API for search queries. Uses `AI_GENERATE_EMBEDDINGS` to create query embeddings and `VECTOR_DISTANCE` (cosine) to find similar documents.

- **Sql.SemanticSearch.DatabaseDeployment**: DbUp-based deployment of SQL schema including vector tables and external embedding model configuration.

- **Sql.SemanticSearch.Core**: Core library containing shared models, services, and database access logic.

- **Sql.SemanticSearch.Shared**: Shared utilities across projects.

- **Unit Test Projects**: Using xUnit v3, Shouldly, and NSubstitute.

### Key Architectural Patterns

- **Central Package Management**: Package versions managed in `Directory.Packages.props` with `ManagePackageVersionsCentrally` enabled. Never specify versions in `.csproj` files.

- **Service Defaults Pattern**: Call `builder.AddServiceDefaults()` in service `Program.cs` files. For ASP.NET Core apps, also call `app.MapDefaultEndpoints()`.

- **Primary Constructors**: Use C# 13 primary constructors. Do NOT replace them when refactoring.

- **File-Scoped Namespaces**: Use file-scoped namespace declarations.

- **Code Quality Enforcement**: `Directory.Build.props` enables strict analysis with SonarAnalyzer treating code analysis warnings as errors (`CodeAnalysisTreatWarningsAsErrors=true`).

- **ArXiv Integration**: The ingestion function uses the arXiv API to load document metadata and download PDFs.

## Build and Run Commands

Build the main solution:
```
dotnet build Sql.SemanticSearch.slnx
```

Build tests only:
```
dotnet build Sql.SemanticSearch.Test.slnx
```

Run tests:
```
dotnet test Sql.SemanticSearch.Test.slnx
```

Run the AppHost (starts Aspire dashboard):
```
dotnet run --project src/Sql.SemanticSearch.AppHost
```

Call the ingestion function (when running locally):
```
curl -X POST http://localhost:7131/api/index-documents/ -H "Content-Type: application/json" -d '{"ids": ["1409.0473"]}'
```

Call the search API (when running locally):
```
curl -X POST http://localhost:5266/api/search -H "Content-Type: application/json" -d '{"query": "neural networks", "top_k": 5}'
```

## SQL Database Configuration

Connect to SQL Server via SSMS using the connection string from the Aspire portal.

Optionally configure SQL Server via user secrets:
```json
{
  "Parameters": {
    "SqlServerPassword": "<password>",
    "SqlServerPort": 14331
  }
}
```

## AI Model Configuration

For Ollama with GPU support, add to AppHost appsettings or secrets:
```json
{
  "Parameters": {
    "OllamaGpuVendor": "Nvidia"
  }
}
```

Supported values: `Nvidia`, `AMD` (or future `Aspire.Hosting.OllamaGpuVendor` values).

## Testing Standards

**REQUIRED**: Use xUnit v3, Shouldly for assertions, and NSubstitute for mocking. Do NOT use xUnit Assert or Moq.

Test naming convention: `MethodName_Scenario_ExpectedBehavior`

Example:
```csharp
public class MyServiceTests
{
    [Fact]
    public void DoSomething_WithValidInput_ShouldReturnExpectedResult()
    {
        // Arrange
        var dependency = Substitute.For<IDependency>();
        dependency.GetData().Returns("test");
        var sut = new MyService(dependency);

        // Act
        var result = sut.DoSomething();

        // Assert
        result.ShouldBe("expected");
        dependency.Received(1).GetData();
    }
}
```

## Code Quality Standards

- Target framework: `net10.0`
- Nullable reference types enabled
- Implicit usings enabled
- Use collection expression for array where possible 
- Latest analysis level with "All" mode
- DO NOT add XML comments to public APIs
- Use async/await for I/O and database operations
- Use dependency injection for services and configuration
- EditorConfig at root defines coding standards

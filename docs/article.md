Document indexing and semantic search with an Azure Function App and SQL Server 2025




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

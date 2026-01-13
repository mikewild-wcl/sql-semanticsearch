using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Sql.SemanticSearch.Shared;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

var connectionString = builder.Configuration.GetConnectionString(ResourceNames.SqlDatabase);

await builder.Build().RunAsync().ConfigureAwait(true);

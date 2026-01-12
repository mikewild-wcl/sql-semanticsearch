var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.Sql_SemanticSearch_Ingestion_Functions>("ingestion-functions")
    //.WithReference(sql)
    //.WaitForCompletion(databaseDeployment)
    ;

await builder.Build().RunAsync().ConfigureAwait(true);

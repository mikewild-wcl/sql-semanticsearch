# Document indexing and semantic search with SQL Server 2025 and Azure Functions

In November 2025 Microsoft released SQL Server 2025 with a major focus on AI features, including a native vector data type and the ability to call external large language models to create vector embeddings. This opens up new ways of working with SQL Server for AI applications. Azure SQL already supported vectors, but SQL Server 2025 brings these capabilities fully on-premises for the first time.

Vector embeddings are numerical representations of text or images that capture their meaning. Vector search calculates vectors using an AI embedding model, and by comparing vectors we can find content that is semantically similar to a query. In modern GenAI applications, embeddings are used for semantic search, RAG pipelines, product matching, data classification, and more. 

Specialized vector databases are available, but if you're already using SQL Server, these new capabilities let you generate vector embeddings directly in the database. You don't need to write custom embedding code or introduce another data store.

This article walks through a proof-of-concept showing how to ingest documents and generate embeddings using only SQL Server 2025 and a Function App, and then adds a simple API that performs semantic search. It's a .NET C# application that uses an Aspire AppHost to set up the database and embedding model locally as well as starting the function and API, all without needing any cloud resources or setup scripts. I'm not going into detail on how that works this time; maybe in another post.

All the code is available in my GitHub - link at the end.

## Architecture Overview

At a high level, the system consists of:

- An Azure Function App for ingestion
- SQL Server 2025 for storage and vector search
- Database deployment using DbUp 
- Ollama for embedding generation
- arXiv API as the document source
- An ASP.NET minimal API for search queries

**Azure Functions** are serverless applications that let us run code on demand. They are a good match for document ingestion because they run only when needed. In this case the function will be triggered by a user request, but in a production application there could be a process that looks for new articles and sends a Service Bus message to another function that does the embedding work. SQL triggers are another possible approach and I’ve linked a good overview in the References section.

**Ollama** is an open-source tool for running large language models (LLMs) locally. It avoids the need to use paid models like OpenAI or Gemini, although those can be used instead.

**arXiv** is a free, open-access repository of scientific papers. It has an easy-to-use API and is a good source of interesting documents.

The application flow is:

1.  A client sends a list of arXiv IDs to an Azure Function
2.  The Function fetches metadata from the arXiv API
3.  Documents are saved into SQL Server
4.  SQL Server generates vector embeddings via `AI_GENERATE_EMBEDDINGS`
5.  A search API generates a query embedding and uses `VECTOR_DISTANCE` to rank results

![Architecture overview diagram, showing the main components described above.](./images/architecture_overview.png)

## Embedding model

For this project I've chosen `nomic-embed-text` as the embedding model. If you want to dig into the details see [Nomic](https://www.nomic.ai/news/nomic-embed-text-v1), or go to [Hugging Face](https://huggingface.co/nomic-ai/nomic-embed-text-v1.5). It's a large-context-length text encoder that performs well compared to OpenAI’s `text-embedding-ada-002` and `text-embedding-3-small`, but if I was using OpenAI in a production environment I'd probably go for `text-embedding-3-large`. 

It's important to use the same model for all embedding tasks; this model creates vectors with 768 dimensions by default and we'll need to set up our SQL tables to match. 

## SQL Server 2025 Schema and Embedding model setup

Even with the latest version of SQL Server, some vector features are still in preview, so the first thing to do is to turn on preview features:

```sql
ALTER DATABASE SCOPED CONFIGURATION
SET PREVIEW_FEATURES = ON;
```

Then make sure server allows external REST endpoints, which is required for calling the embedding model: 

``` sql
EXEC sp_configure 'external rest endpoint enabled', 1;
RECONFIGURE WITH OVERRIDE;
```

We have a table for documents which includes summary text and metadata as a JSON column containing authors and tags, plus an index for it. JSON columns are another new SQL 2025 feature that makes working with document data much easier.

``` sql
CREATE TABLE dbo.Documents
(
    [Id] INT IDENTITY CONSTRAINT [PK_Documents] PRIMARY KEY,
    [ArxivId] NVARCHAR(50) NULL,
    [Title] NVARCHAR(300) NOT NULL,
    [Summary] NVARCHAR(MAX) NULL,
    [Comments] NVARCHAR(MAX) NULL,
    [Metadata] JSON NULL,
    [PdfUri] NVARCHAR(1000) NOT NULL,
    [Published] DATETIME2(0) NOT NULL,
    [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_Documents_Created DEFAULT (SYSUTCDATETIME()),
    [Updated] DATETIME2(7) NULL,
    [LastUpdatedOn] DATETIME2(7) NULL)
GO
CREATE JSON INDEX IX_Documents_Metadata ON dbo.Documents(Metadata) FOR ('$');
GO
```

Summary and Metadata are going to be turned into vector embeddings. Microsoft recommends using a separate table for each embedding (link in the references section) so there are two tables. We need to use EXEC because `$EMBEDDING_DIMENSIONS$` is a parameter passed in by the setup code which has the number of dimensions (768).

``` sql
EXEC('CREATE TABLE dbo.DocumentSummaryEmbeddings (
          [Id] INT NOT NULL,
          [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
          [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentSummaryEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
          CONSTRAINT FK_DocumentSummaryEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))')

EXEC('CREATE TABLE dbo.DocumentMetadataEmbeddings (
        [Id] INT NOT NULL,
        [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentMetadataEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_DocumentMetadataEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))')
```

Finally we need to set up the Ollama embedding model. If we were using OpenAI then there would be some security configuration needed, but we can ignore that for this proof-of-concept.

Ollama only exposes an http endpoint and if you look at other articles or books they'll tell you to set up an https proxy first. Aspire simplifies by letting us create a dev tunnel; the https uri of the dev tunnel is passed in as a parameter, as well as the embedding model name and dimension, and the model is created by the script below. I drop and recreate it if the model name or dev tunnel uri has changed.

``` sql
IF EXISTS (SELECT * FROM sys.external_models 
           WHERE [Name] = '$EXTERNAL_EMBEDDING_MODEL$' 
             AND ([Location] <> '$AI_CLIENT_ENDPOINT$'
              OR  [Model] <> '$EMBEDDING_MODEL$'))
BEGIN
    EXEC('DROP EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$')
END

EXEC('CREATE EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$
      WITH (
        LOCATION = ''$AI_CLIENT_ENDPOINT$'',
        API_FORMAT = ''OLLAMA'',
        MODEL_TYPE = EMBEDDINGS,
        MODEL = ''$EMBEDDING_MODEL$'')')
```

You can check that the model was deployed by running this in your SQL tool of choice:
```
SELECT [external_model_id], [name], [api_format], model_type_desc, [model], [location]
FROM sys.external_models
```
![External models in SSMS.](./images/external_models.png)

## Azure Function: Insert Pipeline 

The Azure function has a simple HTTP trigger that takes a list of arXiv document ids. There is some minimal checking in the application so only valid ids are used; invalid ids are thrown away but in a production scenario there would need to be better logging and reporting. The ids are passed through to a service that calls arXiv APIs - here are some highlights from the ingestion code:

``` csharp
builder.Services.AddHttpClient<IArxivApiClient, ArxivApiClient>(client =>
{
    client.BaseAddress = new("http://export.arxiv.org/api/");
});
```

``` csharp
string query = $"query?id_list={idList}&start={start}&max_results={maxResults}";
var response = await _httpClient.GetAsync(query, cancellationToken);

string xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
var doc = XDocument.Parse(xmlContent);
var entries = doc.Descendants(ArxivNamespace.Atom + "entry").ToList();
```

``` csharp
extension(XElement? entry)
{
    public ArxivPaper? ToArxivPaper()
    {
        if (entry is null) return null;

        var entryId = entry.Element(ArxivNamespace.Atom + "id")?.Value;

        if (string.IsNullOrEmpty(entryId)) return null;

        var id = entryId.ToShortId();

        var pdfLink = entry.Descendants(ArxivNamespace.Atom + "link")
            .FirstOrDefault(l => l.Attribute("title")?.Value == "pdf");

        var pdfUrl = pdfLink?.Attribute("href")?.Value;
        var published = DateTime.TryParse(entry.Element(ArxivNamespace.Atom + "published")?.Value, out var publishedDate) ? publishedDate : DateTime.MinValue;

        return new ArxivPaper(id, entry.Element(ArxivNamespace.Atom + "title")?.Value?.Trim())
        {
            PdfUri = pdfUrl is not null ? new Uri(pdfUrl) : null,
            Summary = entry.Element(ArxivNamespace.Atom + "summary")?.Value?.Trim() ?? string.Empty,
            Comments = entry.Element(ArxivNamespace.Atom + "comment")?.Value?.Trim(),
            Published = published,
            Authors = entry.Descendants(ArxivNamespace.Atom + "author")
                .Select(a => a.Element(ArxivNamespace.Atom + "name")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name!)
                .ToList()
                ?? [],
            Categories = entry.Descendants(ArxivNamespace.Atom + "category")
                .Select(c => c.Attribute("term")?.Value)
                    .Where(term => !string.IsNullOrEmpty(term))
                    .Select(term => term!)
                    .ToList()
                    ?? []
        };
    }
}
```

Once we have the paper it gets saved by a series of calls to the database, all wrapped in a transaction. I'm using Dapper and all the SQL is embedded in my C# code. You might think stored procedures would be better, and you might be right, but for early development I find this way much easier.

``` csharp
using var connection = _databaseConnection.CreateConnection();
connection.Open();
using var transaction = connection.BeginTransaction();

await DeleteExistingDocumentIfExists(paper.Id, transaction);

var documentId = await SaveDocument(paper, transaction);
await SaveDocumentSummaryEmbeddings(documentId, transaction);
await SaveDocumentMetadataEmbeddings(documentId, transaction);

transaction.Commit();
```

``` csharp
private async Task<int> DeleteExistingDocumentIfExists(string arxivId, System.Data.IDbTransaction transaction) =>
    await _databaseConnection.ExecuteAsync(
        """
        DELETE FROM DocumentSummaryEmbeddings
        WHERE [Id] IN (SELECT [Id] FROM Documents WHERE [ArxivId] = @ArxivId);
        
        DELETE FROM DocumentMetadataEmbeddings
        WHERE [Id] IN (SELECT [Id] FROM Documents WHERE [ArxivId] = @ArxivId);

        DELETE FROM dbo.Documents
        WHERE [ArxivId] = @ArxivId;            
        """,
        new { ArxivId = arxivId },
        transaction: transaction);

private async Task<int> SaveDocument(ArxivPaper paper, System.Data.IDbTransaction transaction) =>
    await _databaseConnection.ExecuteScalarAsync<int>(
        """
        INSERT INTO dbo.Documents ([ArxivId], [Title], [Summary], [Comments], [Metadata], [PdfUri], [Published])                            
        VALUES (@ArxivId, @Title, @Summary, @Comments, @Metadata, @PdfUri, @Published);

        SELECT CAST(SCOPE_IDENTITY() as int);
        """,
        new
        {
            ArxivId = paper.Id,
            paper.Title,
            paper.Summary,
            paper.Comments,
            Metadata = paper.MetadataString,
            PdfUri = paper.PdfUri?.ToString(),
            paper.Published
        },
        transaction: transaction);

/* Note: Embedding model is *NOT* a SQL injection risk, it must be hard-coded so we have to use the settings value. */
private async Task SaveDocumentSummaryEmbeddings(int documentId, System.Data.IDbTransaction transaction) =>
    await _databaseConnection.ExecuteAsync(
        $"""
        INSERT INTO dbo.DocumentSummaryEmbeddings ([Id], [Embedding])
        SELECT @Id,
                AI_GENERATE_EMBEDDINGS(d.[Summary] USE MODEL {_aiSettings.ExternalEmbeddingModel})
        FROM dbo.Documents d
        WHERE d.[Id] = @Id
            AND d.[Summary] IS NOT NULL;
        """,
        new { Id = documentId },
        transaction: transaction);

private async Task SaveDocumentMetadataEmbeddings(int documentId, System.Data.IDbTransaction transaction) =>
        await _databaseConnection.ExecuteAsync(
            $"""
            INSERT INTO dbo.DocumentMetadataEmbeddings ([Id], [Embedding])
            SELECT @Id,
                    AI_GENERATE_EMBEDDINGS(CAST(d.[Metadata] AS NVARCHAR(MAX)) USE MODEL {_aiSettings.ExternalEmbeddingModel})
            FROM dbo.Documents d
            WHERE d.[Id] = @Id
                AND d.[Metadata] IS NOT NULL;
            """,
        new { Id = documentId },
        transaction: transaction);
```

The most interesting thing here is AI_GENERATE_EMBEDDINGS which tells SQL Server to make a call to the external server to generate the embedding. 

### Testing the ingestion

To test the function we just need to issue a POST request using the Functions.http script in the functions project: 

```
@Functions_HostAddress = http://localhost:7131

POST {{Functions_HostAddress}}/api/index-documents/
Content-Type: application/json
Accept: application/json

{
  "ids": [
    "1207.0580",
...
    "physics/0702069"
  ]
}
```

or use Postman or a curl command:
```
curl -X POST http://localhost:7131/api/index-documents/ -H "Content-Type: application/json" -d '{"ids": ["1409.0473", "2510.04950"]}'
```

This can run for a while, so I don't recommend sending large requests. I had to increase the REST API timeout in Visual Studio (go to Tools..Options, search for "REST advanced") so I could send a reasonably large request.

## Querying with the Search API

I decided to use an ASP.NET Core API project with a minimal API for the search. The API will be available for users who want a quick response, and Azure functions can be slow to start (depending on the plan). An API is the way to go. 

The search uses AI_GENERATE_EMBEDDINGS to create an embedding on the search query, then compares that against the embeddings we added to the embedding table during ingestion. The function used is VECTOR_DISTANCE and I'm calculating the cosine distance here. I won't go into the maths but that's one of the most common ways of comparing vectors. SQL Server also supports dot-product and Euclidean distance.

```csharp
var results = (await _databaseConnection.QueryAsync<SearchResultItem>(
    $"""
    DECLARE @vector VECTOR({_aiSettings.EmbeddingModelDimensions});
    
    SELECT @vector = AI_GENERATE_EMBEDDINGS(@query USE MODEL {_aiSettings.ExternalEmbeddingModel});
    
    SELECT TOP(@k) [ArxivId],
                   [Title],
                   [Summary],
                   [Comments],
                   [Metadata],
                   [PdfUri],
                   [Published],
                   VECTOR_DISTANCE('cosine', ds.embedding, @vector) AS [Distance]
    FROM dbo.Documents d
    INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id
    ORDER BY Distance ASC;
    """,
    new
    {
        searchRequest.Query,
        @k = searchRequest.Top
    })
    ).ToList();
```

The query takes parameters with the query text and a "top k" value to limit the search, then uses VECTOR_DISTANCE. The smaller the distance the more relevant the result.

A couple of gotchas if you're looking at the code - Metadata comes back as a string of json, and PdfUri is a string that needs to be converted to a Uri. There are Dapper SQL mapper type handlers for both of these conversions, which keeps the query code clean.

## Calling the search API

To use the search, issue a POST request using the Sql.SemanticSearch.Api.http script in the API project: 

```
@Sql.SemanticSearch.Api_HostAddress = http://localhost:5266

POST {{Sql.SemanticSearch.Api_HostAddress}}/api/search/
Content-Type: application/json
Accept: application/json

{
  "query": "I am looking for information on Gradient Descent",
  "top_k": 3
}
```

or curl

```
curl -X POST https://sql-semanticsearch-api-sql_semanticsearch.dev.localhost:7253/api/search -H "Content-Type: application/json" -d '{"query": "Find papers on Gen AI", "top_k": 5}'
```

![Search with results.](./images/search_results.png)

There were some interesting results. One was a paper with a title of "Can apparent superluminal neutrino speeds be explained as a quantum weak measurement?" and a summary "Probably not.". That isn't going to work well with this search. Maybe we need a Title vector index or a combined vector index that includes the title with the summary:
```
AI_GENERATE_EMBEDDINGS(FORMATMESSAGE('Title: %s. Summary: %s', d.[Title], d.[Summary]) USE MODEL SemanticSearchEmbeddingModel)
```

Alternatively add an embedding table for the Title column and use a LEAST query that gets the closest match of either the title or the summary, like this:
```
SELECT TOP(@k)  
    LEAST(
        VECTOR_DISTANCE('cosine', ds.embedding, @vector),
        VECTOR_DISTANCE('cosine', dt.embedding, @vector)
    ) as Distance,
    [ArxivId],
    [Title],
    [Summary],
    [Comments],
    [Metadata],
    [PdfUri],
    [Published]               
FROM dbo.Documents d
INNER JOIN dbo.DocumentTitleEmbeddings dt ON dt.id = d.id
INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id
ORDER BY [Distance] ASC;
```

Either of the above approaches will find the neutrino paper for a query "Find papers on neutrinos".

I also did some tests with the metadata index, such as a LEAST query that took the closest results from either the summary or the metadata. However, this didn't return very meaningful results and it might be better to have a specialized search query when looking for authors or categories, or maybe those both need their own indexes.

## Vector indexing

I haven't created any index on the vectors. SQL Server 2025 supports approximate nearest neighbour (ANN) vector indexes, but adding it makes the table read-only which isn't appropriate for this proof-of-concept. It makes searches much faster, but as you can tell by the name it only returns approximate results, which is fine in a Gen AI scenario. For now, in my small database I'm sticking with VECTOR_DISTANCE and accepting the slightly slower performance. In a production scenario, vectors could be created in a staging table or separate partition then swapped in and the vector index recreated to work around the limitation.

I've linked information on DiskANN in the References below, Microsoft says the read-only limitation will be removed soon and I look forward to revisiting this when they do.

## Conclusions

I set out to show that the new AI capabilities in SQL Server 2025 are a good fit for semantic search applications, and I'm very happy with the results. Letting the database take care of creating embeddings simplifies the application, and if you're already using SQL Server there's no need to bring in yet another database to store vectors. 

There's a lot that can be built on here. Right now, vector search is limited to the document summaries; a full RAG system would need to download PDFs, split them into chunks, and generate embeddings for them. Other possible improvements include title embeddings or combined title + summary embeddings, vector index improvements, metadata search, or text search using keywords.

Aspire was also instrumental in making this work locally without cloud setup or scripts. I’ll go into more detail on that in a future post.

### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)

## References

- [Vector search and vector indexes in the SQL Database Engine](https://learn.microsoft.com/en-us/sql/sql-server/ai/vectors?view=sql-server-ver17)
- [Efficiently and Elegantly Modeling Embeddings in Azure SQL and SQL Server](https://devblogs.microsoft.com/azure-sql/efficiently-and-elegantly-modeling-embeddings-in-azure-sql-and-sql-server/) by Davide Mauri is an excellent place to start.
- [Database and AI: solutions for keeping embeddings updated](https://devblogs.microsoft.com/azure-sql/database-and-ai-solutions-for-keeping-embeddings-updated/) talks about how to use an Azure Functions Sql Trigger binding
- [DiskANN: Vector Search for All](https://www.microsoft.com/en-us/research/project/project-akupara-approximate-nearest-neighbor-search-for-large-scale-semantic-search/)

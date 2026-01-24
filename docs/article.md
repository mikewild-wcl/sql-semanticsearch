# Document indexing and semantic search with an Azure Function App and SQL Server 2025

In November 2025 Microsoft release SQL Server 2025, which, like everything else from Microsoft reently, is heavily AI-focussed. It introduces a native vector data type and the ability to call external large language models. This opens up new ways of searcing information in SQL Server, as well as in Azure Sql which already had the new types.

Vector search is important for building intelligent applications, allowing semantic search, RAG pipelines, product
matching, data classification, and more. 

Specialized vector databases are available, but for SQL Server users that meant a more complicated architectire,  generating vector embeddings in code and saving them into inefficient structures. Now everything is much simpler.

**Azure Functions** are a simple, cloud-native way to add semantic intelligence to any application.

???????????????????
This article looks at how to use Azure functions to 
 - process document from arXiv into a SQL Server database 
 - generate vector embeddings for the summary
 - allow search

We'll use Aspire for hosting and running locally - what I think of as "localfirst - cloud native". It lets us run a database, functions, and an embedding model locally without needing to set up resources on Azure or running scripts. The embedding model will be run in Ollama, again because it can run locally. I'm not going into detail on how that works; that can come another day.

 This is intended as an initial proof of concept, so it isn't a full RAG solution. We just want to show how to add embeddings for semantic search; a future iteration could download files, chunk them into the database, and add a chat capability.

![Architecture overview diagram, showing the main components described above.](./images/architecture_overview.png)

## Embedding model

For this project I've chosen `nomic-embed-text` as the embedding model. If you want to dig into the details see [Nomic](https://www.nomic.ai/news/nomic-embed-text-v1), or go to [Hugging Face](https://huggingface.co/nomic-ai/nomic-embed-text-v1.5).
It's a large context length text encoder that performs better than OpenAI `text-embedding-ada-002` and `text-embedding-3-small`, but if I migrate to OpenAI in a production environment I'd probably go for `text-embedding-3-small`. The model isn't important but what IS important is using the same model for all embedding tasks,

The model has 768 dimensions by default and we'll need to remember that when creating our SQL tables.

e don't need a chat model at this stage since we are only searching, but it might be added later.


## SQL Server 2025 Schema and Embedding model setup

The SQL Server tables are fairly simple.

First make sure server allows external REST endpoints: 

``` sql
EXEC sp_configure 'external rest endpoint enabled', 1;
RECONFIGURE WITH OVERRIDE;
```

We have a table for documents which includes summary text and metadata as a JSON column containing authors and tags, plus an index for it.
**Link to Davide M article**

``` sql
CREATE TABLE dbo.Documents
(
    [Id] INT IDENTITY CONSTRAINT PK_Documents primary key,
    [ArxivId] NVARCHAR(50) NULL,
    [Title] nvarchar(300) NOT NULL,
    [Summary] nvarchar(max) NULL,
    [Comments] nvarchar(max) NULL,
    [Metadata] JSON NULL,
    [PdfUri] NVARCHAR(1000) NOT NULL,
    [Published] DATETIME2(0) NOT NULL,
    [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_Documents_Created DEFAULT (SYSUTCDATETIME()),
    [Updated] DATETIME2(7) NULL,
    [LastUpdatedOn] datetime2(0) NULL
)
GO
CREATE JSON INDEX IX_Documents_Metadata ON dbo.Documents(Metadata) FOR ('$');
GO
```

Summary and Metadata are going to be turned into vector embeddings so we have a table for each of those - Microsoft recommends doing it this way. Note the `$EMBEDDING_DIMENSIONS$` which has the number of dimensions (768).

``` sql
EXEC('CREATE TABLE dbo.DocumentSummaryEmbeddings (
    [Id] INT NOT NULL,
    [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
    [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentSummaryEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_DocumentSummaryEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))')
```

``` sql
EXEC('CREATE TABLE dbo.DocumentMetadataEmbeddings (
    [Id] INT NOT NULL,
    [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
    [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentMetadataEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_DocumentMetadataEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))')
```

Finally we need to set up the Ollama embedding model. If we were using OpenAI then there some security settings need to configured, but those aren't needed for Ollama.

One problem with using Ollama is that it only exposes an http endpoint. If you look at other articles r books they'll tell you to set up an https proxy of one sort or another, but because we're using Aspire a dev tunnel can do the job. The uri for that needs to be passed into the setup script below. I drop and recreate it each time because I've been bitten by Aspire changing the dev tunnel uri and port.
``` sql
IF EXISTS (SELECT * FROM sys.external_models WHERE name = '$EXTERNAL_EMBEDDING_MODEL$')
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

You can check that the model was deployed by running this in SSMS:
```
SELECT [external_model_id], [name], [api_format], model_type_desc, [model], [location]
FROM sys.external_models
```
![External models in SSMS.](./images/external_models.png)

## Azure Function: Insert Pipeline 
  - http-triggered function that takes a list of arXiv document ids and queries the details
  - insert and create embedding
  - simple approach; in production it might be better to send a message to another function via a queue or ServiceBus and do the embedding there
    - also mention article on SQL trigger that will do the embedding for us
  - search query - takes a string and returns search reasults. Can test using curl or Postman for simplicity, so no need for an extra web or console app

``` csharp
```

## 7. Azure Function: Query Pipeline

Generate an embedding for the user query, then let SQL Server do vector
search:

``` csharp
var cmd = new SqlCommand(@"
    SELECT TOP 10 Id, Title, Content,
    VECTOR_DISTANCE(Embedding, @Query, COSINE) AS Score
    FROM Documents
    ORDER BY Score", conn);

cmd.Parameters.AddWithValue("@Query", queryEmbedding);
```

 ## Vector index

I haven't done this because as soon as a vecttor index is created, the table becomes read-only.
  
? -   Built-in approximate nearest neighbor (ANN) vector indexes\
-   Fast similarity functions (cosine, dot-product, Euclidean)\
-   Hardware acceleration for vector math

In a production scenario, vectors could be created in a staging table or separate partition then swapped in and the vector index recreated. That's too much for a proof-of-concept like this! Microsoft says the read-only limitation will be removed soon so heopefully a proper index will be acheivable then.




I've included a script that drops all the tables in the repo - sql/clean_documents_database.sql.


## Getting data from arXiv

The function calls a service which calls a client class that gets papers from arXiv. It has methods to:
 - Get paper information from arXiv API by paper ID
 - Download the PDF file and returns it as a MemoryStream   
 - Complete workflow: Get paper info and download PDF to memory stream 
 
 We'll only use the first one for now. The others will come in handy when it's time to pull down the files and save into our database.

 
The API calls here mean we aren't completely local-first, but that's fine for this intial version. In future a workaround can be created, e.g. a fake api that provides test data.

## Testing

Test the function using a POST request - use the test http script in the functions project, add a Postman request or simply use curl:
```
curl -X POST http://localhost:7131/api/index-documents/ -H "Content-Type: application/json" -d '{"ids": ["1409.0473", "2510.04950" ] }'
```

## Future improvements

- add an MCP server as a function and show how it can be used from a client like GitHub Copilot
- add a notebook showing the difference between PdfPig in C# and Docling in Python for extracting chunking PDF files
- implement the chunking in another function
- deploy to Azure

---
### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)


## References

    https://devblogs.microsoft.com/azure-sql/efficiently-and-elegantly-modeling-embeddings-in-azure-sql-and-sql-server/

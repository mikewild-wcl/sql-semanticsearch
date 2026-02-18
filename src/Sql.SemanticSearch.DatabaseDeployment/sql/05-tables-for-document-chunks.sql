
IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentChunks')
    CREATE TABLE dbo.DocumentChunks
    (
        [Id] INT IDENTITY CONSTRAINT [PK_DocumentChunks] PRIMARY KEY,
        [DocumentId] INT NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [PageNumber] INT NULL,
        [IndexOnPage] INT NULL,
        [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentChunks_Created DEFAULT (SYSUTCDATETIME()),
        [LastUpdatedOn] DATETIME2(7) NULL
        CONSTRAINT FK_DocumentChunks_Documents 
            FOREIGN KEY (DocumentId) REFERENCES Documents(Id) 
            ON DELETE CASCADE 
            ON UPDATE CASCADE
    )
GO

PRINT FORMATMESSAGE('Creating embedding tables with %s vector dimensions', $EMBEDDING_DIMENSIONS$)
GO

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentChunkEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentChunkEmbeddings (
            [Id] INT NOT NULL CONSTRAINT [PK_DocumentChunkEmbeddings] PRIMARY KEY,
            [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
            [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentChunkEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT FK_DocumentChunkEmbeddings_DocumentChunks FOREIGN KEY (Id) REFERENCES DocumentChunks(Id))')
GO

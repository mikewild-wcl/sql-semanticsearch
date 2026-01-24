
IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'Documents')
    CREATE TABLE dbo.Documents
    (
        [Id] INT IDENTITY CONSTRAINT [PK_Documents] PRIMARY KEY,
        [ArxivId] NVARCHAR(50) NULL,
        [Title] NVARCHAR(300) NOT NULL,
        [Summary] NVARCHAR(MAX) NULL,
        [CommentS] NVARCHAR(MAX) NULL,
        [Metadata] JSON NULL,
        [PdfUri] NVARCHAR(1000) NOT NULL,
        [Published] DATETIME2(0) NOT NULL,
        [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_Documents_Created DEFAULT (SYSUTCDATETIME()),
        [Updated] DATETIME2(7) NULL,
        [LastUpdatedOn] datetime2(0) NULL
    )
GO

IF NOT EXISTS(SELECT * FROM sys.indexes where name = 'IX_Documents_Metadata' AND type_desc = 'JSON')
    CREATE JSON INDEX IX_Documents_Metadata ON dbo.Documents(Metadata) FOR ('$');
GO

PRINT FORMATMESSAGE('Creating embedding tables with %s vector dimensions', $EMBEDDING_DIMENSIONS$)
GO

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentSummaryEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentSummaryEmbeddings (
        [Id] INT NOT NULL [PK_DocumentSummaryEmbeddings] PRIMARY KEY,
        [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentSummaryEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_DocumentSummaryEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))')
GO

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentMetadataEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentMetadataEmbeddings (
        [Id] INT NOT NULL [PK_DocumentMetadataEmbeddings] PRIMARY KEY,
        [Embedding] VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentMetadataEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_DocumentMetadataEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))')
GO

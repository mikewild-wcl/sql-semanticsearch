DECLARE @aiProvider NVARCHAR(20) = '$AI_PROVIDER$'

PRINT 'Creating external embedding model with provider=$AI_PROVIDER$, endpoint=$AI_CLIENT_ENDPOINT$, model=$EMBEDDING_MODEL$' 

IF NOT EXISTS (SELECT * FROM sys.tables WHERE [Name] = 'OllamaDebugging')
        CREATE TABLE dbo.OllamaDebugging
        (
            [Id] INT IDENTITY CONSTRAINT PK_OllamaDebugging PRIMARY KEY,
            [RequestTime] DATETIME2(7) NOT NULL CONSTRAINT DF_OllamaDebugging_RequestTime DEFAULT (SYSUTCDATETIME()),
            [Message] NVARCHAR(MAX) NOT NULL
        );

INSERT INTO dbo.OllamaDebugging (Message) VALUES ('Checking Ollama model')

--The location can change when running in Aspire , so we need to check and update if necessary
IF EXISTS (SELECT * FROM sys.external_models WHERE [Name] = '$EXTERNAL_EMBEDDING_MODEL$' AND [Location] <> '$AI_CLIENT_ENDPOINT$')
BEGIN
    INSERT INTO dbo.OllamaDebugging (Message) VALUES ('Updating Ollama model')
    EXEC('ALTER EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$
          SET
          (
            LOCATION = ''$AI_CLIENT_ENDPOINT$''
          );')
    INSERT INTO dbo.OllamaDebugging (Message) VALUES ('Updated Ollama model $EXTERNAL_EMBEDDING_MODEL$ to $AI_CLIENT_ENDPOINT$')
END

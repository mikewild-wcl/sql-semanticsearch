--Get parameters from environment
DECLARE @aiProvider NVARCHAR(20) = '$AI_PROVIDER$'
DECLARE @externalModelName NVARCHAR(50)

PRINT 'Creating external embedding model with provider=$AI_PROVIDER$, endpoint=$AI_CLIENT_ENDPOINT$, model=$EMBEDDING_MODEL$' 

IF (@aiProvider = 'OLLAMA')
BEGIN
    PRINT 'Using OLLAMA as AI provider'
    SET @externalModelName = 'SqlVectorSampleOllamaEmbeddingModel'

    IF EXISTS (SELECT * FROM sys.external_models WHERE name = @externalModelName)
    BEGIN
        PRINT FORMATMESSAGE('Dropping existing external model ''%s''', @externalModelName)
        EXEC('DROP EXTERNAL MODEL ' + @externalModelName)
    END

    DECLARE @cmd NVARCHAR(4000) 
    SET @cmd = 'CREATE EXTERNAL MODEL ' + @externalModelName + ' 
          WITH (
            LOCATION = ''$AI_CLIENT_ENDPOINT$'',
            API_FORMAT = ''OLLAMA'',
            MODEL_TYPE = EMBEDDINGS,
            MODEL = ''$EMBEDDING_MODEL$'')'

    EXEC('CREATE EXTERNAL MODEL ' + @externalModelName + ' 
          WITH (
            LOCATION = ''$AI_CLIENT_ENDPOINT$'',
            API_FORMAT = ''OLLAMA'',
            MODEL_TYPE = EMBEDDINGS,
            MODEL = ''$EMBEDDING_MODEL$'')')
END
ELSE IF (@aiProvider = 'AZUREOPENAI')
BEGIN
    PRINT 'Using Azure OpenAI as AI provider'
    --TODO: Add Azure OpenAI external model creation code here
END
ELSE
BEGIN
    PRINT 'No valid AI provider'
END
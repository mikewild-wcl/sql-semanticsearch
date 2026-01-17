DECLARE @aiProvider NVARCHAR(20) = '$AI_PROVIDER$'

PRINT 'Creating external embedding model with provider=$AI_PROVIDER$, endpoint=$AI_CLIENT_ENDPOINT$, model=$EMBEDDING_MODEL$' 

IF (@aiProvider = 'OLLAMA')
BEGIN
    PRINT 'Using OLLAMA as AI provider'

    IF EXISTS (SELECT * FROM sys.external_models WHERE name = '$EXTERNAL_EMBEDDING_MODEL$')
    BEGIN
        PRINT 'Dropping existing external model $EXTERNAL_EMBEDDING_MODEL$'
        EXEC('DROP EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$')
    END

    EXEC('CREATE EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$
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
/* 
Always run this during deployment in case the model location has changed, which can happen with dev tunnels.
*/

DECLARE @aiProvider NVARCHAR(20) = '$AI_PROVIDER$'

PRINT 'Creating external embedding model with provider=$AI_PROVIDER$, endpoint=$AI_CLIENT_ENDPOINT$, model=$EMBEDDING_MODEL$' 

IF (@aiProvider = 'OLLAMA')
BEGIN
    PRINT 'Using OLLAMA as AI provider'

    --Drop existing external model if location has changed
    IF EXISTS (SELECT * FROM sys.external_models WHERE [Name] = '$EXTERNAL_EMBEDDING_MODEL$' AND [Location] <> '$AI_CLIENT_ENDPOINT$')
    BEGIN
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
/* 
Always run this during deployment in case the model location has changed, which can happen with dev tunnels.
*/

DECLARE @aiProvider NVARCHAR(20) = '$AI_PROVIDER$'

PRINT 'Creating external embedding model with provider=$AI_PROVIDER$, endpoint=$AI_CLIENT_ENDPOINT$, model=$EMBEDDING_MODEL$'
PRINT 'NOTES: If the external model exists but has a different location or model name, it will be dropped then created with the new values.'
PRINT '       If the external model already exists with the same name, location and model name, no changes will be made.'

IF (@aiProvider = 'OLLAMA')
BEGIN
    PRINT 'Using OLLAMA as AI provider'

    --Drop existing external model if anything has changed
    IF EXISTS (SELECT * FROM sys.external_models 
               WHERE [Name] = '$EXTERNAL_EMBEDDING_MODEL$' 
                 AND ([Location] <> '$AI_CLIENT_ENDPOINT$'
                  OR  [Model] <> '$EMBEDDING_MODEL$'))
    BEGIN
        EXEC('DROP EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$')
    END

    IF EXISTS (SELECT * FROM sys.external_models 
               WHERE [Name] = '$EXTERNAL_EMBEDDING_MODEL$' )
    BEGIN
        PRINT 'External model $EXTERNAL_EMBEDDING_MODEL$ already exists'
    END
    ELSE BEGIN
        EXEC('CREATE EXTERNAL MODEL $EXTERNAL_EMBEDDING_MODEL$
              WITH (
                LOCATION = ''$AI_CLIENT_ENDPOINT$'',
                API_FORMAT = ''OLLAMA'',
                MODEL_TYPE = EMBEDDINGS,
                MODEL = ''$EMBEDDING_MODEL$'')')

        PRINT 'Created external model $EXTERNAL_EMBEDDING_MODEL$'
    END
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
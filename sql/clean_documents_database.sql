/*
Drops all the tables from the Documents database.
Also drops the DbUp journal file so the deployment can be rerun.

WARNING: This removes everything! If you just want to remove document data use this:

DELETE dbo.DocumentSummaryEmbeddings
--DELETE dbo.DocumentCommentEmbeddings
DELETE dbo.DocumentMetadataEmbeddings
--DELETE dbo.DocumentChunkEmbeddings
--DELETE dbo.DocumentChunks
DELETE dbo.Documents

*/

IF EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'Documents')
	DROP INDEX IF EXISTS IX_Documents_Metadata ON dbo.Documents;

IF EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentChunks')
	ALTER TABLE dbo.DocumentChunks DROP CONSTRAINT IF EXISTS FK_DocumentChunks_Documents
IF EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentSummaryEmbeddings')
	ALTER TABLE dbo.DocumentSummaryEmbeddings DROP CONSTRAINT IF EXISTS FK_DocumentSummaryEmbeddings_Documents
IF EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentCommentEmbeddings')
	ALTER TABLE dbo.DocumentCommentEmbeddings DROP CONSTRAINT IF EXISTS FK_DocumentCommentEmbeddings_Documents
IF EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentMetadataEmbeddings')
	ALTER TABLE dbo.DocumentMetadataEmbeddings DROP CONSTRAINT IF EXISTS FK_DocumentMetadataEmbeddings_Documents
IF EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentChunkEmbeddings')
	ALTER TABLE dbo.DocumentChunkEmbeddings DROP CONSTRAINT IF EXISTS FK_DocumentChunkEmbeddings_DocumentChunks

DROP TABLE IF EXISTS dbo.DocumentSummaryEmbeddings
DROP TABLE IF EXISTS dbo.DocumentCommentEmbeddings
DROP TABLE IF EXISTS dbo.DocumentMetadataEmbeddings
DROP TABLE IF EXISTS dbo.DocumentChunkEmbeddings

DROP TABLE IF EXISTS dbo.DocumentChunks
DROP TABLE IF EXISTS dbo.Documents

--Drop DbUp tables if they exist. SchemaVersions is the default name, but some projects use $__dbup_journal
DROP TABLE IF EXISTS dbo.SchemaVersions
DROP TABLE IF EXISTS dbo.[$__dbup_journal]

--DROP SCHEMA IF EXISTS [test];

IF EXISTS (SELECT * FROM sys.external_models WHERE name = 'SqlVectorSampleOllamaEmbeddingModel')
	DROP EXTERNAL MODEL SqlVectorSampleOllamaEmbeddingModel



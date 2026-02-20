namespace Sql.SemanticSearch.Core.Chunking.Interfaces;

public interface IDocumentChunkingService
{
    Task IndexDocument(DatabaseDocument document, CancellationToken cancellationToken = default);
}

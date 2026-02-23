using Microsoft.Extensions.DataIngestion;

namespace Sql.SemanticSearch.Core.Chunking.Interfaces;

public interface IDocumentReader
{
    Task<IngestionDocument> Read(Uri uri, string documentIdentifier, string? mediaType = null, CancellationToken cancellationToken = default);
}

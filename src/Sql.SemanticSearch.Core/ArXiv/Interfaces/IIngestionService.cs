using Sql.SemanticSearch.Core.Messages;

namespace Sql.SemanticSearch.Core.ArXiv.Interfaces;

public interface IIngestionService
{
    Task ProcessIndexingRequest(IndexingRequest indexingRequest, CancellationToken cancellationToken = default);
}

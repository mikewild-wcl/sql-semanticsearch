using Sql.SemanticSearch.Core.Requests;

namespace Sql.SemanticSearch.Core.ArXiv.Interfaces;

public interface IIngestionService
{
    Task ProcessIndexingRequest(IndexingRequest indexingRequest);
}

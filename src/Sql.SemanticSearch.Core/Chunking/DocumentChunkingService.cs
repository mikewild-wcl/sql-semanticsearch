using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.Chunking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sql.SemanticSearch.Core.Chunking;

public class DocumentChunkingService(
    ILogger<IDocumentChunkingService> logger) : IDocumentChunkingService
{
    public ILogger<IDocumentChunkingService> Logger { get; } = logger;

    public async Task IndexDocument(DatabaseDocument document)
    {

    }
}

using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sql.SemanticSearch.Core.Chunking.Interfaces;

public interface IDocumentChunkingService
{
    Task IndexDocument(DatabaseDocument document);
}

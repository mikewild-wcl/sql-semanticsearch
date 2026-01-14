using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Core.ArXiv.Exceptions;

[ExcludeFromCodeCoverage(Justification = "No need to cover exception types in tests.")]
public sealed class ArxivPaperNotFoundException : InvalidOperationException
{
    public ArxivPaperNotFoundException()
    {
    }

    public ArxivPaperNotFoundException(string arxivId)
        : base($"Paper with arXiv ID '{arxivId}' was not found in arXiv.")
    {
    }

    public ArxivPaperNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

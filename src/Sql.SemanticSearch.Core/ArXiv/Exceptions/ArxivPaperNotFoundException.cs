namespace Sql.SemanticSearch.Core.ArXiv.Exceptions;

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

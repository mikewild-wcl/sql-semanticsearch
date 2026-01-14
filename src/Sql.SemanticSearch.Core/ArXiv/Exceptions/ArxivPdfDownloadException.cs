namespace Sql.SemanticSearch.Core.ArXiv.Exceptions;

public sealed class ArxivPdfDownloadException : Exception
{
    public ArxivPdfDownloadException()
    {
    }

    public ArxivPdfDownloadException(string? message) : base(message)
    {
    }

    public ArxivPdfDownloadException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

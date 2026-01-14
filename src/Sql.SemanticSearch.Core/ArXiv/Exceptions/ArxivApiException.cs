namespace Sql.SemanticSearch.Core.ArXiv.Exceptions;

public sealed class ArxivApiException : Exception
{
    public ArxivApiException()
    {
    }

    public ArxivApiException(string? message) : base(message)
    {
    }

    public ArxivApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
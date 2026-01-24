using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Core.ArXiv.Exceptions;

[ExcludeFromCodeCoverage(Justification = "No need to cover exception types in tests.")]
public sealed class ArxivPdfDownloadException : Exception
{
    public ArxivPdfDownloadException()
    {
    }

    public ArxivPdfDownloadException(string? message) : base(message)
    {
    }

    public ArxivPdfDownloadException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}

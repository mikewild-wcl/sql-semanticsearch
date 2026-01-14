using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Core.ArXiv.Exceptions;

[ExcludeFromCodeCoverage(Justification = "No need to cover exception types in tests.")]
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
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Sql.SemanticSearch.Ingestion.Functions.UnitTests.Builders;

internal static class HttpRequestBuilder
{
    internal static HttpRequest Build(string method, string path, string? query = null, string? body = null, string contentType = "text/plain")
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = method;
        request.Path = path;

        if (!string.IsNullOrEmpty(query))
        {
            request.QueryString = new QueryString(query);
        }

        if (!string.IsNullOrEmpty(body))
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            request.Body = new MemoryStream(bytes);
            request.ContentLength = bytes.Length;
            request.ContentType = contentType;
        }
        return request;
    }
}

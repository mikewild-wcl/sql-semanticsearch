/*
To run with the default "./badfile.md":
dotnet run .\ingestion-markdown-error-repro-in-mem.cs 
*/

#pragma warning disable S3903

#:package Microsoft.Extensions.DataIngestion
#:package Microsoft.Extensions.DataIngestion.Markdig

using Microsoft.Extensions.DataIngestion;
using System.Text;

var filePath = args.Length > 0 ? args[0] : "./badfile.md";
if (!File.Exists(filePath))
{
    Console.WriteLine($"File '{filePath}' does not exist.");
    return;
}

await ReadMarkdown(
    """
    arXiv:2310.18460v1  [cs.IT]  27 Oct 2023
    # Good markdown

    Content
    """,
    "good-markdown");

await ReadMarkdown(
    """
    arXiv:2310.18460v1  [cs.IT]  27 Oct 2023
    # Bad markdown
    3
    2
    0
    2

    t
    c
    O
    7
    2

    ]
    T
    I
    .
    s
    c
    [

    1
    v
    0
    6
    4
    8
    1
    .
    0
    1
    3
    2
    :
    v
    i
    X
    r
    a

    Content
    """,
    "bad-markdown");

async static Task ReadMarkdown(string content, string identifier, string mediaType = "text/markdown")
{
    Console.WriteLine($"Reading document with identifier '{identifier}' and media type '{mediaType}'");

    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

    var reader = new DocumentReader();
    var result = await reader.ReadAsync(stream, identifier, mediaType);
    foreach (var contentItem in result.EnumerateContent())
    {
        Console.WriteLine($"Content item: page {contentItem.PageNumber} - {contentItem.Text}");
    }
}

internal sealed class DocumentReader : IngestionDocumentReader
{
    private readonly MarkdownReader _markdownReader = new();

    public async override Task<IngestionDocument> ReadAsync(Stream source, string identifier, string mediaType, CancellationToken cancellationToken = default)
    {
        return await _markdownReader.ReadAsync(source, identifier, mediaType, cancellationToken);
    }
}

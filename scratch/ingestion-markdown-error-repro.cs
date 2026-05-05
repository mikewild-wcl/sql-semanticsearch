/*
To run with the default "./badfile.md":
dotnet run .\ingestion-markdown-error-repro.cs 

Optionally, you can provide a path to a markdown file as the first argument:
dotnet run .\ingestion-markdown-error-repro.cs "./goodfile.md"
*/

#pragma warning disable S3903

#:package Microsoft.Extensions.DataIngestion
#:package Microsoft.Extensions.DataIngestion.Markdig

using Microsoft.Extensions.DataIngestion;

var filePath = args.Length > 0 ? args[0] : "./badfile.md";
if (!File.Exists(filePath))
{
    Console.WriteLine($"File '{filePath}' does not exist.");
    return;
}

var reader = new DocumentReader();

var result = await reader.ReadAsync(new FileInfo(filePath), "test-doc");
foreach (var contentItem in result.EnumerateContent())
{
    Console.WriteLine($"Content item: page {contentItem.PageNumber} - {contentItem.Text}");
}

internal sealed class DocumentReader : IngestionDocumentReader
{
    private readonly MarkdownReader _markdownReader = new();

    public async override Task<IngestionDocument> ReadAsync(Stream source, string identifier, string mediaType, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Reading document with identifier '{identifier}' and media type '{mediaType}'");
        return await _markdownReader.ReadAsync(source, identifier, mediaType, cancellationToken);
    }
}

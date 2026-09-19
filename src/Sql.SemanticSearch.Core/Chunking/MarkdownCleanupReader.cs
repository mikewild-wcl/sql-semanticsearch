using Microsoft.Extensions.DataIngestion;
using ModelContextProtocol.Client;
using Sql.SemanticSearch.Core.Chunking.Extensions;

namespace Sql.SemanticSearch.Core.Chunking;

internal sealed class MarkdownCleanupReader(Uri mcpServerUri, McpClientOptions? options = null)
    : MarkItDownMcpReader(mcpServerUri, options)
{
    public override async Task<IngestionDocument> ReadAsync(Stream source, string identifier, string mediaType,
        CancellationToken cancellationToken = new())
    {
        var ingestionDocument = await base.ReadAsync(source, identifier, mediaType, cancellationToken);

        return CleanupIngestionDocument(ingestionDocument);
    }

    public override async Task<IngestionDocument> ReadAsync(FileInfo source, string identifier, string? mediaType = null,
        CancellationToken cancellationToken = new())
    {
        var ingestionDocument = await base.ReadAsync(source, identifier, mediaType, cancellationToken);

        return CleanupIngestionDocument(ingestionDocument);
    }

    private static IngestionDocument CleanupIngestionDocument(IngestionDocument ingestionDocument)
    {
        if (ingestionDocument.Sections.Count > 0 &&
            ingestionDocument.Sections[0].Elements.Count > 0)
        {
            var fixup = ingestionDocument.Sections[0].Elements[0].Text?.FindAndReverseVixraPatterns();
            if (fixup?.Fixed == true)
            {
                ingestionDocument.Sections[0].Elements[0].Text = fixup.Value.Text;
            }
        }

        return ingestionDocument;
    }
}

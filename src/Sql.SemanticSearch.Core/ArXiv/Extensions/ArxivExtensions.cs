using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Sql.SemanticSearch.Core.ArXiv.Extensions;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Extension members don't need to be static")]
internal static class ArxivExtensions
{
    private static readonly JsonSerializerOptions CamelCaseSerialierOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    extension(string value)
    {
        public string ToShortId()
        {
            // Equivalent to: self.entry_id.split("arxiv.org/abs/")[-1]
            const string marker = "arxiv.org/abs/";
            var parts = value.Split(marker, StringSplitOptions.None);
            return parts.Length > 0
                ? parts[^1]
                : value;
        }
    }

    extension(ArxivPaper paper)
    {
        public string DisplayTitle => $"{paper.Id}: {paper.Title}";
        
        public string MetadataString => JsonSerializer.Serialize(
            new
            {
                paper.Authors,
                paper.Categories
            },
            CamelCaseSerialierOptions);
    }
}


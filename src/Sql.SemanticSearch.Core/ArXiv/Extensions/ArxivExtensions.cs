using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sql.SemanticSearch.Core.ArXiv.Extensions;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Extension members don't need to be static")]
internal static partial class ArxivExtensions
{
    private static readonly JsonSerializerOptions CamelCaseSerialierOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /* Validate arXiv IDs (both new and old style):
        ^
        (?:                              # Either:
          \d{4}\.\d{4,5}                  #   New-style: YYMM.NNNN or YYMM.NNNNN
          |                               #   OR
          [a-z\-]+(?:\.[A-Z]{2})?/\d{7}   #   Old-style: archive(.SUB)/YYMMNNN
        )
        (?:v\d+)?                         # Optional version suffix
        $
    */
    [GeneratedRegex(@"^(?:\d{4}\.\d{4,5}|[a-z\-]+(?:\.[A-Z]{2})?/\d{7})(?:v\d+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex ArxivIdRegex();

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

        public bool IsValidId()
        {
            var isMatch = ArxivIdRegex().IsMatch(value);
            return isMatch;
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


using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Sql.SemanticSearch.Core.Chunking.Extensions;

[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Extension used from other assemblies.")]
public static partial class StringExtensions
{
    [GeneratedRegex(@"(?<=\S)\[")]
    private static partial Regex FormatLeftSquareBracketsRegex();

    [GeneratedRegex(@"\](?=\S)")]
    private static partial Regex FormatRightSquareBracketsRegex();

    [GeneratedRegex(@"(\d{1,2})([A-Za-z]{3})(\d{4})")]
    private static partial Regex FormatArxivIdRegex();

    [GeneratedRegex(@"\r\n|\n|\r")]
    private static partial Regex LineEndingRegex();

    [GeneratedRegex(
    @"^\d{4}.+:vixra$",
    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex VixraRegex();

    extension(string? input)
    {
        public (bool Fixed, string? Text) FindAndReverseVixraPatterns()
        {
            if (input is null) return (false, null);

            var lines = input.Split(["\r\n", "\n"], StringSplitOptions.None);
            var replacements = new List<(int startIndex, int count, string replacement)>();

            // Try all consecutive line combinations
            for (var i = 0; i < lines.Length; i++)
            {
                for (var count = 1; count <= lines.Length - i; count++)
                {
                    var joined = string.Join("", lines.Skip(i).Take(count).Where(l => !string.IsNullOrWhiteSpace(l)));

                    // Skip if null or it doesn't match the pattern
                    if (string.IsNullOrEmpty(joined) ||
                        !VixraRegex().IsMatch(joined)) continue;

                    // Skip matches that overlap an already recorded replacement.
                    if (replacements.Any(r => i >= r.startIndex && i < r.startIndex + r.count))
                    {
                        continue;
                    }

                    // Reverse it
                    var reversed = new string(joined.Reverse().ToArray());
                    var formatted = FormatReversedVixraOutput(reversed);
                    replacements.Add((i, count, formatted));
                    break;
                }
            }

            // Apply replacements in reverse order to avoid index issues
            foreach (var (startIndex, count, reversed) in replacements.OrderByDescending(r => r.startIndex))
            {
                // Remove these lines and insert the reversed version
                var newLines = lines.Take(startIndex).Concat([reversed]).Concat(lines.Skip(startIndex + count)).ToList();
                lines = [.. newLines];
            }

            if (replacements.Any(r => r.startIndex == 0)
                && lines.Length >= 3
                && lines[1].StartsWith('#')
                && string.IsNullOrWhiteSpace(lines[2]))
            {
                lines = [.. lines.Where((_, index) => index != 2)];
            }

            return (replacements.Count > 0, string.Join("\n", lines));
        }

        public string? NormalizeLineEndings(string? newLine = "\n")
        {
            if (input is null) return null;

            // Default to system newline if not specified
            newLine ??= Environment.NewLine;

            var normalized = LineEndingRegex().Replace(input, "\n");

            // Then replace LF with the desired newline
            if (newLine != "\n")
            {
                normalized = normalized.Replace("\n", newLine, StringComparison.CurrentCulture);
            }

            return normalized;
        }
    }

    private static string FormatReversedVixraOutput(string text)
    {
        text = FormatLeftSquareBracketsRegex().Replace(text, " [");
        text = FormatRightSquareBracketsRegex().Replace(text, "] ");
        text = FormatArxivIdRegex().Replace(text, "$1 $2 $3");

        return text;
    }
}

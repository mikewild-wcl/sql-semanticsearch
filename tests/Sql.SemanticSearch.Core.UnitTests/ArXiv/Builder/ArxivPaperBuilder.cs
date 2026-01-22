using Sql.SemanticSearch.Core.ArXiv;

namespace Sql.SemanticSearch.Core.UnitTests.ArXiv.Builder;

internal static class ArxivPaperBuilder
{
    public static List<ArxivPaper> BuildDummyPapers()
    {
        return
        [
            new("id1", "Test Title 1")
            {
                Summary = "Test summary 1",
                PdfUri = new Uri("http://example.com/test_1.pdf"),
                Published = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                Authors = ["Author 1", "Author 2"],
                Categories = ["cs.CL", "cs.LG"],
                Comments = "First test comment"
            },
            new("id2", "Test Title 2")
            {
                Summary = "Test summary 2",
                PdfUri = new Uri("http://example.com/test_2.pdf"),
                Published = new DateTime(2023, 2, 1, 11, 30, 0, DateTimeKind.Utc),
                Authors = ["Author 3", "Author 4"],
                Categories = ["cs.NE", "stat.ML"],
                Comments = "Second test comment"
            }
        ];
    }
}

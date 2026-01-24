using Microsoft.Extensions.Logging.Abstractions;
using Sql.SemanticSearch.Core.ArXiv;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Requests;
using Sql.SemanticSearch.Core.UnitTests.ArXiv.Builder;
using Sql.SemanticSearch.Core.UnitTests.TestExtensions;

namespace Sql.SemanticSearch.Core.UnitTests.ArXiv;

public class IngestionServiceTests
{
    private readonly IArxivApiClient _arxivApiClientSubstitute;
    private readonly IDatabaseConnection _databaseConnection;
    private readonly IngestionService _sut;

    public IngestionServiceTests()
    {
        _arxivApiClientSubstitute = Substitute.For<IArxivApiClient>();
        _databaseConnection = Substitute.For<IDatabaseConnection>();
        _sut = new IngestionService(
            _arxivApiClientSubstitute,
            _databaseConnection,
            new AISettings("Ollama", "Test"),
            NullLogger<IngestionService>.Instance);
    }

    [Fact]
    public async Task ProcessIndexingRequest_CallsArxivApiClient_ForMultipleId()
    {
        // Arrange
        var request = new IndexingRequest { Ids = ["id1", "id2", "id3"] };

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(
            Arg.Is<IReadOnlyCollection<string>>(p =>
                p.Count == 3 &&
                p.Contains("id1") &&
                p.Contains("id2") &&
                p.Contains("id3")),
            Arg.Any<int>())
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessIndexingRequest_CallsArxivApiClient_ForSingleId()
    {
        // Arrange
        var request = new IndexingRequest { Ids = ["single-id"] };

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(
            Arg.Is<IReadOnlyCollection<string>>(p =>
                p.Count == 1 &&
                p.Contains("single-id")),
            Arg.Any<int>())
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessIndexingRequest_DoesNotCallArxivApiClient_WhenIdsIsEmpty()
    {
        // Arrange
        var request = new IndexingRequest { Ids = [] };

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(
            Arg.Is<IReadOnlyCollection<string>>(p => p.Count == 0),
            Arg.Any<int>())
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessIndexingRequest_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        IndexingRequest? request = null;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _sut.ProcessIndexingRequest(request!));

        exception.ParamName.ShouldBe("indexingRequest");
    }

    [Fact]
    public async Task ProcessIndexingRequest_CompletesSuccessfully_WithMultipleIds()
    {
        // Arrange
        var request = new IndexingRequest { Ids = ["id1", "id2"] };

        var papers = ArxivPaperBuilder.BuildDummyPapers();
        var mockAsyncEnumerableRecords = TestMocks.MockAsyncEnumerable(papers);

        _arxivApiClientSubstitute.GetPaperInfo(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<int>())
            .Returns(mockAsyncEnumerableRecords);

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(
            Arg.Is<IReadOnlyCollection<string>>(p =>
                p.Count == 2 &&
                p.Contains("id1") &&
                p.Contains("id2")),
            Arg.Any<int>())
            .ToListAsync(TestContext.Current.CancellationToken);
    }
}

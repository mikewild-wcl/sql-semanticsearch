using Microsoft.Extensions.Logging.Abstractions;
using Sql.SemanticSearch.Core.ArXiv;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Requests;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
            NullLogger<IngestionService>.Instance);
    }

    [Fact]
    public async Task ProcessIndexingRequest_CallsArxivApiClient_ForEachId()
    {
        // Arrange
        var request = new IndexingRequest { Ids = ["id1", "id2", "id3"] };

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        ///https://www.tumblr.com/petesit/173058234308/checking-async-calls-received-in-nsubstitute
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(["id1"]).FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        Received.InOrder(async () =>
        {
            await _arxivApiClientSubstitute.Received(1).GetPaperInfo(["id1"]).FirstOrDefaultAsync();
            //await um.CreateUserAsync(Arg.Is<User>(a => a.UserName == "b")); 
        });

        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(["id1"]).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(["id2"]).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(["id3"]).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessIndexingRequest_CallsArxivApiClient_ForSingleId()
    {
        // Arrange
        var request = new IndexingRequest { Ids = ["single-id"] };

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.Received(1).GetPaperInfo(["single-id"]).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessIndexingRequest_DoesNotCallArxivApiClient_WhenIdsIsEmpty()
    {
        // Arrange
        var request = new IndexingRequest { Ids = [] };

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.DidNotReceive().GetPaperInfo(Arg.Any<IEnumerable<string>>()).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
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
        var paper = new ArxivPaper("test-id", "Test Title")
        {
            Summary = "Test summary",
            PdfUri = new Uri("http://example.com/test.pdf")
        };
        _arxivApiClientSubstitute.GetPaperInfo(Arg.Any<IEnumerable<string>>()).FirstOrDefaultAsync(TestContext.Current.CancellationToken).Returns(paper);

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        await _arxivApiClientSubstitute.Received(2).GetPaperInfo(Arg.Any<IEnumerable<string>>()).ToListAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessIndexingRequest_CallsGetPaperInfoInOrder_ForMultipleIds()
    {
        // Arrange
        var request = new IndexingRequest { Ids = ["first", "second", "third"] };
        var callOrder = new List<string>();

        _arxivApiClientSubstitute.GetPaperInfo(Arg.Any<IEnumerable<string>>()).FirstOrDefaultAsync(TestContext.Current.CancellationToken)
            .Returns(x =>
            {
                callOrder.Add(x.Arg<string>());
                return new ArxivPaper(x.Arg<string>(), "Title")
                {
                    Summary = "Test summary",
                    PdfUri = new Uri("http://example.com/test.pdf")
                };
            });

        // Act
        await _sut.ProcessIndexingRequest(request);

        // Assert
        callOrder.ShouldBe(["first", "second", "third"]);
    }
}

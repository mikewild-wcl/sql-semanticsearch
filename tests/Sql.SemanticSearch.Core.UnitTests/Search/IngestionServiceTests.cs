using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Registry;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search;

namespace Sql.SemanticSearch.Core.UnitTests.Search;

public class SearchServiceTests
{
    private readonly IDatabaseConnection _databaseConnection;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _databaseConnection = Substitute.For<IDatabaseConnection>();

        _resiliencePipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
        _resiliencePipelineProvider
            .GetPipeline(Arg.Any<string>())
            .Returns(ResiliencePipeline.Empty);

        _sut = new SearchService(
            _databaseConnection,
            _resiliencePipelineProvider,
            new AISettings("Ollama", "Test"),
            NullLogger<SearchService>.Instance);
    }

    [Fact]
    public async Task Search_Returns_ExpectedResults()
    {
        // Arrange
        var request = new SearchRequest { Query = "Test query" };

        // Act
        var results = await _sut.Search(request, TestContext.Current.CancellationToken);

        // Assert
        results.ShouldNotBeNull();
    }
}

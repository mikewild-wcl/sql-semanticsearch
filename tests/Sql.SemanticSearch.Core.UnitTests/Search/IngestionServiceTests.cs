using Microsoft.Extensions.Logging.Abstractions;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search;

namespace Sql.SemanticSearch.Core.UnitTests.Search;

public class SearchServiceTests
{
    private readonly IDatabaseConnection _databaseConnection;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _databaseConnection = Substitute.For<IDatabaseConnection>();
        _sut = new SearchService(
            _databaseConnection,
            new AISettings("Ollama", "Test"),
            NullLogger<SearchService>.Instance);
    }

    [Fact]
    public async Task Search_Returns_ExpectedResults()
    {
        // Arrange
        var request = new SearchRequest { Query = "Test query" };

        // Act
        var results = await _sut.Search(request);

        // Assert
        results.ShouldNotBeNull();
    }
}

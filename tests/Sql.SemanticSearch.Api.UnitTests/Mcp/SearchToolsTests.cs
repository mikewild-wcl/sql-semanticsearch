using NSubstitute.ExceptionExtensions;
using Sql.SemanticSearch.Api.Mcp;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Search.Interfaces;

namespace Sql.SemanticSearch.Api.UnitTests.Mcp;

public class SearchToolsTests
{
    private readonly ISearchService _searchServiceSubstitute;
    private readonly SearchTools _sut;

    public SearchToolsTests()
    {
        _searchServiceSubstitute = Substitute.For<ISearchService>();
        _sut = new SearchTools(_searchServiceSubstitute);
    }

    [Fact]
    public async Task SearchDocuments_WithValidQuery_ReturnsJsonWithResults()
    {
        // Arrange
        var query = "machine learning";
        var topK = 3;
        var expectedResults = new[]
        {
            new SearchResultItem
            {
                ArxivId = "2301.00001",
                Title = "Deep Learning Fundamentals",
                Summary = "A comprehensive study of deep learning",
                Distance = 0.15f,
                Published = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SearchResultItem
            {
                ArxivId = "2302.00002",
                Title = "Neural Networks and AI",
                Summary = "Exploring neural network architectures",
                Distance = 0.22f,
                Published = new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SearchResultItem
            {
                ArxivId = "2303.00003",
                Title = "Transformer Models",
                Distance = 0.35f,
                Published = new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        _searchServiceSubstitute
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var result = await _sut.SearchDocuments(query, topK, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(expectedResults.Length);

        result[0].ArxivId.ShouldBe("2301.00001");
        result[0].Title.ShouldBe("Deep Learning Fundamentals");
        result[0].Distance.ShouldBe(0.15f);

        await _searchServiceSubstitute.Received(1).Search(
            Arg.Is<SearchRequest>(r => r.Query == query && r.Top == topK),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchDocuments_WithNoResults_ReturnsEmptyJsonArray()
    {
        // Arrange
        var query = "nonexistent query";
        var topK = 5;

        _searchServiceSubstitute
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _sut.SearchDocuments(query, topK, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();

        await _searchServiceSubstitute.Received(1).Search(
            Arg.Is<SearchRequest>(r => r.Query == query && r.Top == topK),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchDocuments_WithDefaultTopK_UsesDefaultValue()
    {
        // Arrange
        var query = "test query";
        var expectedResults = new[]
        {
            new SearchResultItem
            {
                ArxivId = "2401.00001",
                Title = "Test Paper",
                Distance = 0.10f
            }
        };

        _searchServiceSubstitute
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var result = await _sut.SearchDocuments(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeEmpty();

        await _searchServiceSubstitute.Received(1).Search(
            Arg.Is<SearchRequest>(r => r.Query == query && r.Top == 5), // Default topK is 5
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchDocuments_WithMetadata_IncludesMetadataInJson()
    {
        // Arrange
        var query = "quantum computing";
        var metadata = new DocumentMetadata
        {
            Authors = ["Alice Smith", "Bob Johnson"],
            Categories = ["cs.AI", "quant-ph"]
        };

        var publishedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        var expectedResults = new[]
        {
            new SearchResultItem
            {
                ArxivId = "2401.00001",
                Title = "Quantum ML Algorithms",
                Summary = "Study of quantum machine learning",
                Comments = "Accepted at NeurIPS 2024",
                Metadata = metadata,
                PdfUri = new Uri("https://arxiv.org/pdf/2401.00001"),
                Distance = 0.12f,
                Published = publishedDate
            }
        };

        _searchServiceSubstitute
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);

        // Act
        var result = await _sut.SearchDocuments(query, 1, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeEmpty();

        var item = result[0];
        item.Title.ShouldBe("Quantum ML Algorithms");
        item.Comments.ShouldBe("Accepted at NeurIPS 2024");
        item.Metadata.ShouldNotBeNull();
        item.Metadata.Authors.Count.ShouldBe(2);
        item.Metadata.Categories.Count.ShouldBe(2);
        item.PdfUri.ShouldBe(new Uri("https://arxiv.org/pdf/2401.00001"));
        item.PublishedDate.ShouldBe(publishedDate.ToString("o"));
    }

    [Fact]
    public async Task SearchDocuments_WhenServiceThrows_PropagatesException()
    {
        // Arrange
        var query = "error query";
        var exception = new InvalidOperationException("Database error");

        _searchServiceSubstitute
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.SearchDocuments(query, cancellationToken: TestContext.Current.CancellationToken));

        thrownException.Message.ShouldBe("Database error");
    }

    [Fact]
    public async Task SearchDocuments_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var query = "cancellation test";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _searchServiceSubstitute
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.SearchDocuments(query, cancellationToken: cancellationToken);

        // Assert
        await _searchServiceSubstitute.Received(1).Search(
            Arg.Any<SearchRequest>(),
            Arg.Is(cancellationToken));
    }
}

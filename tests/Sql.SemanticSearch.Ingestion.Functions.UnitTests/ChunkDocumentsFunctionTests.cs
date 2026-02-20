using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using Sql.SemanticSearch.Core.Chunking;
using Sql.SemanticSearch.Core.Chunking.Interfaces;

namespace Sql.SemanticSearch.Ingestion.Functions.UnitTests;

public class ChunkDocumentsFunctionTests
{
    private readonly IDocumentChunkingService _chunkingServiceSubstitute;
    private readonly ChunkDocumentsFunction _sut;

    public ChunkDocumentsFunctionTests()
    {
        _chunkingServiceSubstitute = Substitute.For<IDocumentChunkingService>();
        _sut = new(_chunkingServiceSubstitute, NullLogger<ChunkDocumentsFunction>.Instance);
    }

    [Fact]
    public async Task Run_WithValidChanges_CallsIndexDocumentForEachDocument()
    {
        // Arrange
        var document1 = new DatabaseDocument(1)
        {
            ArxivId = "2301.00001",
            Title = "Deep Learning Fundamentals",
            Summary = "A comprehensive study of deep learning",
            Published = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PdfUri = "https://arxiv.org/pdf/2301.00001"
        };

        var document2 = new DatabaseDocument(2)
        {
            ArxivId = "2302.00002",
            Title = "Neural Networks and AI",
            Summary = "Exploring neural network architectures",
            Published = new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            PdfUri = "https://arxiv.org/pdf/2302.00002"
        };

        var changes = new List<SqlChange<DatabaseDocument>>
        {
            new(SqlChangeOperation.Insert, document1),
            new(SqlChangeOperation.Insert, document2)
        };

        // Act
        await _sut.Run(changes, TestContext.Current.CancellationToken);

        // Assert
        await _chunkingServiceSubstitute.Received(1).IndexDocument(
            Arg.Is<DatabaseDocument>(d => 
                d.Id == 1 &&
                d.ArxivId == "2301.00001" &&
                d.Title == "Deep Learning Fundamentals"),
            Arg.Any<CancellationToken>());

        await _chunkingServiceSubstitute.Received(1).IndexDocument(
            Arg.Is<DatabaseDocument>(d =>
                d.Id == 2 &&
                d.ArxivId == "2302.00002" &&
                d.Title == "Neural Networks and AI"),
            Arg.Any<CancellationToken>());

        await _chunkingServiceSubstitute.Received(2).IndexDocument(Arg.Any<DatabaseDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WithSingleChange_CallsIndexDocumentOnce()
    {
        // Arrange
        var document = new DatabaseDocument(1)
        {
            ArxivId = "2301.00001",
            Title = "Test Paper",
            Summary = "Test summary",
            Published = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var changes = new List<SqlChange<DatabaseDocument>>
        {
            new(SqlChangeOperation.Insert, document)
        };

        // Act
        await _sut.Run(changes, TestContext.Current.CancellationToken);

        // Assert
        await _chunkingServiceSubstitute.Received(1).IndexDocument(
            Arg.Is<DatabaseDocument>(d =>
                d.Id == 1 &&
                d.Title == "Test Paper"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WithNullChanges_DoesNotCallIndexDocument()
    {
        // Act
        await _sut.Run(null!, TestContext.Current.CancellationToken);

        // Assert
        await _chunkingServiceSubstitute.DidNotReceive().IndexDocument(Arg.Any<DatabaseDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WithEmptyChangesList_DoesNotCallIndexDocument()
    {
        // Arrange
        var changes = new List<SqlChange<DatabaseDocument>>();

        // Act
        await _sut.Run(changes, TestContext.Current.CancellationToken);

        // Assert
        await _chunkingServiceSubstitute.DidNotReceive().IndexDocument(Arg.Any<DatabaseDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_PassesDocumentWithAllProperties()
    {
        // Arrange
        var document = new DatabaseDocument(42)
        {
            ArxivId = "2401.00001",
            Title = "Advanced Topics in ML",
            Summary = "Comprehensive overview",
            Comments = "Accepted at conference",
            Metadata = "{\"categories\":[\"cs.AI\",\"cs.LG\"]}",
            PdfUri = "https://arxiv.org/pdf/2401.00001",
            Published = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        var changes = new List<SqlChange<DatabaseDocument>>
        {
            new(SqlChangeOperation.Update, document)
        };

        DatabaseDocument? capturedDocument = null;

        _chunkingServiceSubstitute
            .IndexDocument(Arg.Any<DatabaseDocument>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                capturedDocument = _.Arg<DatabaseDocument>();
                return Task.CompletedTask;
            });

        // Act
        await _sut.Run(changes, TestContext.Current.CancellationToken);

        // Assert
        capturedDocument.ShouldNotBeNull();
        capturedDocument.Id.ShouldBe(42);
        capturedDocument.ArxivId.ShouldBe("2401.00001");
        capturedDocument.Title.ShouldBe("Advanced Topics in ML");
        capturedDocument.Summary.ShouldBe("Comprehensive overview");
        capturedDocument.Comments.ShouldBe("Accepted at conference");
        capturedDocument.Metadata.ShouldBe("{\"categories\":[\"cs.AI\",\"cs.LG\"]}");
        capturedDocument.PdfUri.ShouldBe("https://arxiv.org/pdf/2401.00001");
        capturedDocument.Published.ShouldBe(new DateTime(2024, 1, 15, 0,0,0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Run_WithMultipleChanges_CallsIndexDocumentForEachInOrder()
    {
        // Arrange
        var documents = Enumerable.Range(1, 5)
            .Select(i => new DatabaseDocument(i)
            {
                ArxivId = $"2024.0000{i}",
                Title = $"Paper {i}",
                Summary = $"Summary for paper {i}"
            })
            .ToList();

        var changes = documents
            .Select(d => new SqlChange<DatabaseDocument>(SqlChangeOperation.Insert, d))
            .ToList();

        // Act
        await _sut.Run(changes, TestContext.Current.CancellationToken);

        // Assert
        await _chunkingServiceSubstitute.Received(5).IndexDocument(Arg.Any<DatabaseDocument>(), Arg.Any<CancellationToken>());

        for (int i = 1; i <= 5; i++)
        {
            await _chunkingServiceSubstitute.Received(1).IndexDocument(
                Arg.Is<DatabaseDocument>(d => d.Id == i),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Run_WhenServiceThrows_PropagatesException()
    {
        // Arrange
        var document = new DatabaseDocument(1)
        {
            ArxivId = "2301.00001",
            Title = "Test Paper",
            Summary = "Test summary"
        };

        var changes = new List<SqlChange<DatabaseDocument>>
        {
            new(SqlChangeOperation.Insert, document)
        };

        var exception = new InvalidOperationException("Chunking service error");
        _chunkingServiceSubstitute
            .IndexDocument(Arg.Any<DatabaseDocument>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.Run(changes, TestContext.Current.CancellationToken));

        thrownException.Message.ShouldBe("Chunking service error");
    }

    [Fact]
    public async Task Run_PassesCancellationToken_ToService()
    {
        // Arrange
        var document = new DatabaseDocument(1)
        {
            ArxivId = "2301.00001",
            Title = "Test Paper",
            Summary = "Test summary"
        };

        var changes = new List<SqlChange<DatabaseDocument>>
        {
            new(SqlChangeOperation.Insert, document)
        };

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        await _sut.Run(changes, cancellationToken);

        // Assert
        await _chunkingServiceSubstitute.Received(1).IndexDocument(Arg.Any<DatabaseDocument>(), cancellationToken);
    }
}

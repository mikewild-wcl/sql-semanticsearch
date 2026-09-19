using Sql.SemanticSearch.Core.Chunking.Extensions;

namespace Sql.SemanticSearch.Core.UnitTests.Chunking.Extensions;

public class StringExtensionTests
{
    [Fact]
    public void FindAndReverseVixraPatterns_Returns_Null_For_Null_Inout()
    {
        // Arrange & Act
        var (wasFixed, text) = default(string?).FindAndReverseVixraPatterns();

        // Assert
        Assert.False(wasFixed);
        Assert.Null(text);
    }

    [Fact]
    public void FindAndReverseVixraPatterns_Reverses_Leading_Header_From_Single_String()
    {
        // Arrange
        const string input = "3102\nguA\n81\n]CO.htam[\n1v8983.8031:viXra\nFirefly Algorithm: Recent Advances and Applications\nXin-She Yang\nSchool of Science and Technology,\nMiddlesex University, The Burroughs, London NW4 4BT, UK.\nXingshi He\nSchool of Science, Xi’an Polytechnic University,\nNo. 19 Jinhua South Road, Xi’an 710048, P. R. China.\nAbstract\nNature-inspired metaheuristic algorithms, especially those based on swarm intelli-\ngence,haveattractedmuchattentioninthelasttenyears. Fireflyalgorithmappearedin\naboutfiveyearsago,itsliteraturehasexpandeddramaticallywithdiverseapplications.\nIn this paper, we will briefly review the fundamentals offirefly algorithmtogether with\na selection of recent publications. Then, we discuss the optimality associated with bal-\nancing explorationand exploitation, which is essentialfor allmetaheuristic algorithms.\nBy comparing with intermittent search strategy, we conclude that metaheuristics such\nas firefly algorithm are better than the optimal intermittent search strategy. We also\nanalysealgorithmsandtheirimpli...";

        // Act
        var (wasFixed, text) = input.FindAndReverseVixraPatterns();

        // Assert
        Assert.True(wasFixed);
        Assert.NotNull(text);
        Assert.StartsWith("arXiv", text, StringComparison.InvariantCulture);
        Assert.StartsWith("arXiv:1308.3898v1 [math.OC] 18 Aug 2013", text, StringComparison.InvariantCulture);
    }

    [Fact]
    public void FindAndReverseVixraPatterns_Reverses_Leading_Header()
    {
        // Arrange
        const string input =
            """
            3102
            guA
            81
            ]CO.htam[
            1v8983.8031:viXra
            
            Firefly Algorithm: Recent Advances and Applications
            """;

        // Act
        var (wasFixed, text) = input.FindAndReverseVixraPatterns();

        Assert.True(wasFixed);
        Assert.NotNull(text);
        Assert.StartsWith("arXiv", text, StringComparison.InvariantCulture);

        //Assert.StartsWith("arXiv:1308.3898v1[math.OC]18Aug2013", result.Text);
        //Assert.Equal("arXiv:1308.3898v1[math.OC]18Aug2013\nFirefly Algorithm: Recent Advances and Applications", result.Text);
        Assert.StartsWith("arXiv:1308.3898v1 [math.OC] 18 Aug 2013", text, StringComparison.InvariantCulture);
        Assert.Equal("arXiv:1308.3898v1 [math.OC] 18 Aug 2013\n\nFirefly Algorithm: Recent Advances and Applications", text);
    }

    [Fact]
    public void FindAndReverseVixraPatterns_Reverses_Pattern_With_One_Character_Per_Line()
    {
        // Arrange
        const string input =
            """
            # Header
            3
            1
            0
            2
            g
            u
            A
            8
            1
            ]
            C
            O
            .
            h
            t
            a
            m
            [
            1
            v
            8
            9
            8
            3
            .
            8
            0
            3
            1
            :
            v
            i
            X
            r
            a

            Content
            """;

        // Act
        var (wasFixed, text) = input.FindAndReverseVixraPatterns();

        // Assert
        Assert.True(wasFixed);
        Assert.NotNull(text);
        Assert.Contains("arXiv:1308.3898v1 [math.OC] 18 Aug 2013", text, StringComparison.InvariantCulture);

        Assert.Equal("# Header\narXiv:1308.3898v1 [math.OC] 18 Aug 2013\n\nContent", text);
    }

    [Fact]
    public void FindAndReverseVixraPatterns_Reverses_Pattern_With_Reversed_Pattern_After_Header()
    {
        // Arrange
        const string input =
            """
            # Header
            3102guA81]CO.htam[1v8983.8031:viXra

            Content
            """;

        // Act
        var (wasFixed, text) = input.FindAndReverseVixraPatterns();

        // Assert
        Assert.True(wasFixed);
        Assert.NotNull(text);
        Assert.Contains("arXiv:1308.3898v1 [math.OC] 18 Aug 2013", text, StringComparison.InvariantCulture);
        Assert.Equal("# Header\narXiv:1308.3898v1 [math.OC] 18 Aug 2013\n\nContent", text);
    }

    [Fact]
    public void FindAndReverseVixraPatterns_Reverses_Pattern_With_Reversed_Pattern_Before_Header()
    {
        // Arrange
        const string input =
            """
            3102guA81]CO.htam[1v8983.8031:viXra
            # Header
            
            Content
            """;

        // Act
        var (wasFixed, text) = input.FindAndReverseVixraPatterns();

        // Assert
        Assert.True(wasFixed);
        Assert.NotNull(text);
        Assert.Contains("arXiv:1308.3898v1 [math.OC] 18 Aug 2013", text, StringComparison.InvariantCulture);
        /*
         *  "# Header\narXiv:1308.38988v1 [math.OC] 18 Aug 2013"
           Not found: "arXiv:1308.3898v1 [math.OC] 18 Aug 2013"
         */
        Assert.Equal("arXiv:1308.3898v1 [math.OC] 18 Aug 2013\n# Header\nContent", text);
    }

    [Fact]
    public void FindAndReverseVixraPatterns_Keeps_Original_When_No_Match()
    {
        // Arrange
        const string input =
            """
            arXiv:1308.3898v1 [math.OC] 18 Aug 2013
            Firefly Algorithm: Recent Advances and Applications
            """;

        // Act
        var (wasFixed, text) = input.FindAndReverseVixraPatterns();

        // Assert
        Assert.False(wasFixed);
        Assert.NotNull(text);
        Assert.Equal(input.Replace("\r\n", "\n", StringComparison.InvariantCulture), text);
        Assert.StartsWith("arXiv:1308.3898v1 [math.OC] 18 Aug 2013", text, StringComparison.InvariantCulture);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("line 1\nline 2\n", "line 1\nline 2\n")]
    [InlineData("line 1\r\nline 2\r\n", "line 1\nline 2\n")]
    public void NormalizeLineEndings_Returns_Expected_Results(string? input, string? expected)
    {
        // Act
        var result = input?.NormalizeLineEndings();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, "\r\n")]
    [InlineData("hello", "hello", "\r\n")]
    [InlineData("line 1\nline 2\n", "line 1\rline 2\r", "\r")]
    [InlineData("line 1\r\nline 2\r\n", "line 1\r\nline 2\r\n", "\r\n")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "input is intentionally null in some test cases; the extension method explicitly handles null")]
    public void NormalizeLineEndings_Returns_Expected_Results_WithNonStandardEnding(string? input, string? expected, string lineEnding)
    {
        // Act
        var result = input.NormalizeLineEndings(lineEnding);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeLineEndings_Returns_Expected_String_For_Raw_String()
    {
        // Arrange
        const string input =
            """
            Line 1
            Line 2
            Line 3
            
            """;

        // Act
        var result = input.NormalizeLineEndings();

        // Assert
        Assert.Equal("Line 1\nLine 2\nLine 3\n", result);
    }
}

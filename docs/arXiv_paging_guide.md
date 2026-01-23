# arXiv API Paging with IAsyncEnumerable

Complete guide to using the arXiv API's paging parameters with C# IAsyncEnumerable for efficient streaming of results.

## arXiv API Paging Parameters

The arXiv API supports the following query parameters for paging:

### Core Paging Parameters

```
http://export.arxiv.org/api/query?id_list={ids}&start={start}&max_results={max}
```

| Parameter | Description | Default | Notes |
|-----------|-------------|---------|-------|
| `id_list` | Comma-separated arXiv IDs | - | e.g., `1706.03762,1810.04805` |
| `start` | Zero-based index of first result | 0 | Use for pagination |
| `max_results` | Results per page | 10 | Max recommended: 2000 |
| `sortBy` | Sort field | relevance | Options: relevance, lastUpdatedDate, submittedDate |
| `sortOrder` | Sort direction | descending | Options: ascending, descending |

### Response Metadata (OpenSearch namespace)

The API response includes OpenSearch elements that help with pagination:

```xml
<opensearch:totalResults>8</opensearch:totalResults>
<opensearch:startIndex>0</opensearch:startIndex>
<opensearch:itemsPerPage>3</opensearch:itemsPerPage>
```

## Why IAsyncEnumerable?

`IAsyncEnumerable<T>` is perfect for arXiv API paging because:

1. **Streaming**: Results are yielded as they arrive, not all at once
2. **Memory Efficient**: Only current page is in memory
3. **Cancellable**: Can stop early without fetching all pages
4. **LINQ Support**: Use `.Where()`, `.Take()`, `.Select()` etc.
5. **Rate Limiting**: Natural place to add delays between requests

## Usage Patterns

### Pattern 1: Query by ID List with Paging

```csharp
var arxivIds = new[] { "1706.03762", "1810.04805", "2005.14165", "1301.3781" };
string idList = string.Join(",", arxivIds);

await foreach (var paper in client.QueryByIdListAsync(idList, maxResults: 2))
{
    Console.WriteLine($"{paper.Title}");
    // Process each paper as it arrives
}
```

**Query String:**
```
?id_list=1706.03762,1810.04805,2005.14165,1301.3781&start=0&max_results=2
?id_list=1706.03762,1810.04805,2005.14165,1301.3781&start=2&max_results=2
```

### Pattern 2: Search Query with Paging

```csharp
await foreach (var paper in client.SearchAsync(
    searchQuery: "cat:cs.AI AND abs:transformer",
    maxResults: 10,
    sortBy: "submittedDate",
    sortOrder: "descending"))
{
    Console.WriteLine($"{paper.Title}");
}
```

### Pattern 3: Early Exit with Take()

```csharp
// Only get first 5 results, even if more are available
var papers = client.QueryByIdListAsync(idList, maxResults: 10)
    .Take(5);

await foreach (var paper in papers)
{
    // Process only first 5
}
```

### Pattern 4: Filtering with Where()

```csharp
// Filter results on-the-fly
var recentPapers = client.SearchAsync("cat:cs.AI", maxResults: 20)
    .Where(p => p.Published.Year >= 2020);

await foreach (var paper in recentPapers)
{
    // Only papers from 2020 onwards
}
```

### Pattern 5: Cancellation Support

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await foreach (var paper in client.QueryByIdListAsync(idList, 10, cts.Token))
    {
        Console.WriteLine(paper.Title);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled");
}
```

## Implementation Details

### The IAsyncEnumerable Method

```csharp
public async IAsyncEnumerable<ArxivPaper> QueryByIdListAsync(
    string idList,
    int maxResults = 10,
    CancellationToken cancellationToken = default)
{
    int start = 0;
    
    while (true)
    {
        // Build URL with paging params
        string url = $"{API_BASE_URL}?id_list={idList}&start={start}&max_results={maxResults}";
        
        // Fetch page
        var response = await httpClient.GetAsync(url, cancellationToken);
        string xml = await response.Content.ReadAsStringAsync();
        
        // Parse entries
        var entries = ParseXml(xml);
        
        if (!entries.Any())
            yield break; // No more results
        
        // Yield each paper
        foreach (var paper in entries)
        {
            yield return paper;
        }
        
        // Move to next page
        start += maxResults;
        
        // Rate limiting
        await Task.Delay(3000, cancellationToken);
    }
}
```

### Key Points

1. **Yield Return**: Streams results one at a time
2. **Yield Break**: Exits when no more results
3. **Rate Limiting**: 3-second delay between requests (arXiv recommendation)
4. **Cancellation**: Honors cancellation token throughout
5. **Auto-paging**: Automatically advances `start` parameter

## Search Query Syntax

arXiv supports powerful search queries:

```csharp
// Search by category
"cat:cs.AI"

// Search in abstract
"abs:neural network"

// Search in title
"ti:transformer"

// Search by author
"au:Hinton"

// Boolean operators
"cat:cs.AI AND ti:transformer"
"cat:cs.LG OR cat:stat.ML"
"ti:neural NOT ti:network"

// Wildcards
"au:Hinton*"

// Date ranges
"submittedDate:[202001010000 TO 202012312359]"
```

## Complete Example

```csharp
var client = new ArxivApiClient();

// Multiple IDs with small page size
var ids = "1706.03762,1810.04805,2005.14165,1301.3781,1409.0473";

int count = 0;
await foreach (var paper in client.QueryByIdListAsync(ids, maxResults: 2))
{
    count++;
    Console.WriteLine($"[{count}] {paper.Title}");
    Console.WriteLine($"    Authors: {string.Join(", ", paper.Authors.Take(3))}");
    Console.WriteLine($"    Published: {paper.Published:yyyy-MM-dd}");
    Console.WriteLine($"    PDF: {paper.PdfUrl}");
    Console.WriteLine();
}
```

## Rate Limiting Best Practices

arXiv API guidelines:
- **3 seconds** between requests (enforced in example code)
- **Max 2000 results** per request (but smaller is better)
- **Sequential requests** only (no parallel)
- **Respectful usage** (don't hammer the server)

## Error Handling

```csharp
try
{
    await foreach (var paper in client.QueryByIdListAsync(idList, 10))
    {
        // Process paper
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled");
}
```

## Performance Considerations

| Page Size | Pros | Cons |
|-----------|------|------|
| Small (10-50) | Fast first result, less memory | More API calls |
| Medium (100-500) | Balanced | - |
| Large (1000+) | Fewer API calls | Slow first result, more memory |

**Recommendation**: 50-100 results per page for most use cases

## OpenSearch Metadata

The response includes useful metadata:

```csharp
XNamespace opensearch = "http://a9.com/-/spec/opensearch/1.1/";

int totalResults = int.Parse(doc.Descendants(opensearch + "totalResults").First().Value);
int startIndex = int.Parse(doc.Descendants(opensearch + "startIndex").First().Value);
int itemsPerPage = int.Parse(doc.Descendants(opensearch + "itemsPerPage").First().Value);

Console.WriteLine($"Showing {startIndex + 1}-{startIndex + itemsPerPage} of {totalResults}");
```

## Advanced: Parallel Downloads

After getting paper metadata, you can download PDFs in parallel:

```csharp
var papers = new List<ArxivPaper>();

await foreach (var paper in client.QueryByIdListAsync(idList, 50))
{
    papers.Add(paper);
}

// Now download PDFs in parallel
var downloadTasks = papers.Select(async p =>
{
    var pdfStream = await DownloadPdfAsync(p.PdfUrl);
    return (p, pdfStream);
});

var results = await Task.WhenAll(downloadTasks);
```

## Common Pitfalls

1. ❌ **Forgetting rate limiting**: Will get rate limited
2. ❌ **Not handling empty results**: Causes infinite loop
3. ❌ **Too large max_results**: Slow first response
4. ❌ **Not disposing streams**: Memory leaks
5. ❌ **Parallel API calls**: Against arXiv guidelines

## Resources

- arXiv API Docs: https://arxiv.org/help/api/user-manual
- arXiv API Access: http://export.arxiv.org/api/query
- OpenSearch Spec: http://www.opensearch.org/Specifications/OpenSearch/1.1

using Microsoft.AspNetCore.Mvc;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search.Interfaces;
using System.ComponentModel;

namespace Sql.SemanticSearch.Api.Endpoints;

internal static class SearchEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapSearchEndpoints()
        {
            app.MapPost("/api/search",
                async (
                    [Description("Search for documents related to a user prompt.")]
                    [FromBody] SearchRequest searchRequest,
                        ISearchService searchService,
                        CancellationToken cancellationToken) =>
                    {
                        var results = await searchService.Search(searchRequest, cancellationToken);
                        return Results.Ok(new SearchResponse([.. results]));
                    })
                .WithSummary("Post a search prompt.")
                .WithDescription("This endpoint handles posted search requests and returns a response.")
                .WithTags("Search");

            return app;
        }
    }
}
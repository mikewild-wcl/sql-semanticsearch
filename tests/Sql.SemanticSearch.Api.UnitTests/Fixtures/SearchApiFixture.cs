using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sql.SemanticSearch.Api.Endpoints;
using Sql.SemanticSearch.Core.Search.Interfaces;

namespace Sql.SemanticSearch.Api.UnitTests.Fixtures;

public sealed class SearchApiFixture : IAsyncLifetime
{
    private WebApplication? _app;

    public ISearchService SearchService { get; } = Substitute.For<ISearchService>();

    public HttpClient CreateClient()
    {
        _app.ShouldNotBeNull();
        return _app.GetTestClient();
    }

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(SearchService);

        _app = builder.Build();
        _app.MapSearchEndpoints();

        await _app.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is null)
        {
            return;
        }

        await _app.DisposeAsync();
        _app = null;
    }
}
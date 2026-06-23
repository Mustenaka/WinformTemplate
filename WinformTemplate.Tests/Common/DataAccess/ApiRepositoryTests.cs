using NUnit.Framework;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Business.Template.Repositories;
using WinformTemplate.Common.DataAccess;

namespace WinformTemplate.Tests.Common.DataAccess;

public sealed class ApiRepositoryTests
{
    [Test]
    public async Task ProductQuery_MapsToDocumentedEndpoint()
    {
        var client = new FakeWebApiClient();
        const string expectedUrl = "/api/Template/products?page=2&pageSize=5&keyword=Key&filters.categoryId=1&sortBy=name&desc=false";
        client.SetGet(expectedUrl, ApiResponse<PagedResult<ProductModel>>.CreateSuccess(new PagedResult<ProductModel>
        {
            Items = new[] { new ProductModel { Id = 1, Name = "Keyboard" } },
            Total = 1
        }));

        var repository = new ApiProductRepository(client);
        var result = await repository.SearchProductsAsync(
            keyword: "Key",
            categoryId: 1,
            pageIndex: 2,
            pageSize: 5,
            orderBy: "name",
            ascending: true);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(client.Calls.Single(), Is.EqualTo(("GET", expectedUrl)));
    }

    [Test]
    public void ProductGetById_WhenTransportFails_ThrowsDataSourceUnavailableException()
    {
        var client = new FakeWebApiClient();
        client.SetGet("/api/Template/products/1", ApiResponse<ProductModel>.CreateTransportError("未连接后端"));
        var repository = new ApiProductRepository(client);

        var ex = Assert.ThrowsAsync<DataSourceUnavailableException>(async () => await repository.GetByIdAsync(1));
        Assert.That(ex!.ModuleKey, Is.EqualTo("Template"));
        Assert.That(ex.Endpoint, Is.EqualTo("/api/Template/products/1"));
    }

    private sealed class FakeWebApiClient : IWebApiClient
    {
        private readonly Dictionary<(string Method, string Url), object> _responses = new();

        public List<(string Method, string Url)> Calls { get; } = new();

        public Task<ApiResponse<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            Calls.Add(("GET", url));
            return Task.FromResult(GetResponse<T>("GET", url));
        }

        public Task<ApiResponse<T>> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            Calls.Add(("POST", url));
            return Task.FromResult(GetResponse<T>("POST", url));
        }

        public Task<ApiResponse<T>> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            Calls.Add(("PUT", url));
            return Task.FromResult(GetResponse<T>("PUT", url));
        }

        public Task<ApiResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            Calls.Add(("DELETE", url));
            return Task.FromResult(GetResponse<T>("DELETE", url));
        }

        public void SetBaseUrl(string baseUrl)
        {
        }

        public void SetTimeout(int seconds)
        {
        }

        public void SetDefaultHeaders(Dictionary<string, string> headers)
        {
        }

        public void SetGet<T>(string url, ApiResponse<T> response)
        {
            _responses[("GET", url)] = response;
        }

        private ApiResponse<T> GetResponse<T>(string method, string url)
        {
            return _responses.TryGetValue((method, url), out var response)
                ? (ApiResponse<T>)response
                : ApiResponse<T>.CreateError("Not configured", 404);
        }
    }
}

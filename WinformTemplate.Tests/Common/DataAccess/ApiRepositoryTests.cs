using NUnit.Framework;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Business.Demo.Repositories;
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

    [Test]
    public async Task DemoNoteQuery_MapsToDocumentedEndpoint()
    {
        var client = new FakeWebApiClient();
        const string expectedUrl = "/api/Demo/notes?page=2&pageSize=5&keyword=Key&sortBy=Title&desc=false";
        client.SetGet(expectedUrl, ApiResponse<PagedResult<DemoNote>>.CreateSuccess(new PagedResult<DemoNote>
        {
            Items = new[] { new DemoNote { Id = 1, Title = "Key note" } },
            Total = 1
        }));

        var repository = new ApiDemoNoteRepository(client);
        var result = await repository.SearchByTitleAsync(
            titleKeyword: "Key",
            pageIndex: 2,
            pageSize: 5,
            orderBy: "Title",
            ascending: true);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(client.Calls.Single(), Is.EqualTo(("GET", expectedUrl)));
    }

    [Test]
    public async Task DemoNoteCrud_MapsToDocumentedEndpoints()
    {
        var client = new FakeWebApiClient();
        var note = new DemoNote { Id = 7, Title = "Mapped", Content = "Body", Pinned = true };

        client.SetGet("/api/Demo/notes/7", ApiResponse<DemoNote>.CreateSuccess(note));
        client.SetPost("/api/Demo/notes", ApiResponse<DemoNote>.CreateSuccess(note));
        client.SetPut("/api/Demo/notes/7", ApiResponse<bool>.CreateSuccess(true));
        client.SetDelete("/api/Demo/notes/7", ApiResponse<bool>.CreateSuccess(true));

        var repository = new ApiDemoNoteRepository(client);

        Assert.That((await repository.GetByIdAsync(7))?.Title, Is.EqualTo("Mapped"));
        Assert.That((await repository.AddAsync(new DemoNote { Title = "Mapped" })).Id, Is.EqualTo(7));
        Assert.That(await repository.UpdateAsync(note), Is.True);
        Assert.That(await repository.DeleteAsync(7), Is.True);

        Assert.That(client.Calls, Is.EqualTo(new[]
        {
            ("GET", "/api/Demo/notes/7"),
            ("POST", "/api/Demo/notes"),
            ("PUT", "/api/Demo/notes/7"),
            ("DELETE", "/api/Demo/notes/7")
        }));
    }

    [Test]
    public void DemoNoteGetById_WhenTransportFails_ThrowsDataSourceUnavailableException()
    {
        var client = new FakeWebApiClient();
        client.SetGet("/api/Demo/notes/1", ApiResponse<DemoNote>.CreateTransportError("not connected"));
        var repository = new ApiDemoNoteRepository(client);

        var ex = Assert.ThrowsAsync<DataSourceUnavailableException>(async () => await repository.GetByIdAsync(1));
        Assert.That(ex!.ModuleKey, Is.EqualTo("Demo"));
        Assert.That(ex.Endpoint, Is.EqualTo("/api/Demo/notes/1"));
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

        public void SetPost<T>(string url, ApiResponse<T> response)
        {
            _responses[("POST", url)] = response;
        }

        public void SetPut<T>(string url, ApiResponse<T> response)
        {
            _responses[("PUT", url)] = response;
        }

        public void SetDelete<T>(string url, ApiResponse<T> response)
        {
            _responses[("DELETE", url)] = response;
        }

        private ApiResponse<T> GetResponse<T>(string method, string url)
        {
            return _responses.TryGetValue((method, url), out var response)
                ? (ApiResponse<T>)response
                : ApiResponse<T>.CreateError("Not configured", 404);
        }
    }
}

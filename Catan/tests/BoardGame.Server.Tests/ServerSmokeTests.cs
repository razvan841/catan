using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class ServerSmokeTests : IClassFixture<DockerServerFixture>
{
    private readonly HttpClient _client;

    public ServerSmokeTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new System.Uri("http://localhost:5000")
        };
    }

    [Fact]
    public async Task Health_Endpoint_Returns_Healthy()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("healthy", content);
    }

    [Fact]
    public async Task Root_Returns_HelloWorld()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal("Hello World", content.Trim());
    }
}

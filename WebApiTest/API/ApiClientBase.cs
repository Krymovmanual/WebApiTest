using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WebApiTest.API;

public abstract class ApiClientBase : IDisposable
{
    public const string DevBaseUrl = "https://qu-dev-qfwebapi.azurewebsites.net/";

    private readonly HttpClient client;

    protected ApiClientBase()
    {
        client = new HttpClient
        {
            BaseAddress = new Uri(DevBaseUrl)
        };
    }

    protected async Task<JObject> PostEmptyJsonAsync(string endpoint)
    {
        var token = await Authorization.DevAuthenticator.GetAccessTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var requestBody = new StringContent("{}", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(endpoint, requestBody);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"POST {endpoint} failed. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {content}");
        }

        return JObject.Parse(content);
    }

    public void Dispose()
    {
        client.Dispose();
        GC.SuppressFinalize(this);
    }
}

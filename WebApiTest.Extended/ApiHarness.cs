using System.Diagnostics;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public sealed class LiveFactAttribute : FactAttribute
{
    public LiveFactAttribute() { if (!Settings.Live) Skip = "Set WEBAPI_RUN_LIVE=1 to run Stage integration tests."; }
}
public sealed class LiveTheoryAttribute : TheoryAttribute
{
    public LiveTheoryAttribute() { if (!Settings.Live) Skip = "Set WEBAPI_RUN_LIVE=1 to run Stage integration tests."; }
}
internal static class Settings
{
    public static bool Live => Environment.GetEnvironmentVariable("WEBAPI_RUN_LIVE") == "1";
    public static Uri BaseUrl
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("WEBAPI_BASE_URL") ?? "https://qu-stage-qfwebapi.azurewebsites.net/";
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
                !string.IsNullOrEmpty(uri.UserInfo) || uri.Query.Length != 0 || uri.Fragment.Length != 0 ||
                uri.AbsolutePath != "/") throw new InvalidOperationException("WEBAPI_BASE_URL must be an HTTPS origin.");
            return uri;
        }
    }
    public static double MaxMilliseconds
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("WEBAPI_MAX_RESPONSE_MS") ?? "5000";
            if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,
                out var result) || !double.IsFinite(result) || result <= 0 || result > 30000)
                throw new InvalidOperationException("WEBAPI_MAX_RESPONSE_MS must be in (0, 30000].");
            return result;
        }
    }
}
public sealed class ApiHarness : IDisposable
{
    private readonly HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = false })
        { BaseAddress = Settings.BaseUrl, Timeout = TimeSpan.FromSeconds(30) };
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private string? token;
    private DateTime expiresAt;
    public async Task<string> GetTokenAsync()
    {
        var supplied = Environment.GetEnvironmentVariable("BEARER_TOKEN");
        if (!string.IsNullOrWhiteSpace(supplied)) return supplied.Trim();
        await tokenLock.WaitAsync();
        try
        {
            if (token != null && DateTime.UtcNow < expiresAt) return token;
            var user = Environment.GetEnvironmentVariable("STAGE_USERNAME");
            var password = Environment.GetEnvironmentVariable("STAGE_PASSWORD");
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                if (!string.IsNullOrWhiteSpace(user) || !string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("Set both STAGE_USERNAME and STAGE_PASSWORD.");
                var directory = new DirectoryInfo(AppContext.BaseDirectory);
                while (directory != null && !File.Exists(Path.Combine(directory.FullName, "stage_creds.json"))) directory = directory.Parent;
                if (directory == null) throw new InvalidOperationException("Set BEARER_TOKEN, both Stage credential variables, or stage_creds.json at the repository root.");
                var credentials = JObject.Parse(await File.ReadAllTextAsync(Path.Combine(directory.FullName, "stage_creds.json")));
                user = credentials.Value<string>("userName"); password = credentials.Value<string>("password");
            }
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Credentials must be non-empty.");
            using var body = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "password", ["userName"] = user, ["password"] = password });
            using var response = await client.PostAsync("/api/token", body);
            Assert.True(response.IsSuccessStatusCode, $"Token request returned HTTP {(int)response.StatusCode}; body omitted.");
            var data = JObject.Parse(await response.Content.ReadAsStringAsync());
            token = data.Value<string>("access_token");
            Assert.False(string.IsNullOrWhiteSpace(token), "Token response has no access_token.");
            var lifetime = data.Value<int?>("expires_in");
            // Missing expiry: do not assume the old one-hour cache is valid.
            expiresAt = DateTime.UtcNow.AddSeconds(lifetime.HasValue ? Math.Max(0, lifetime.Value - 30) : 0);
            return token!;
        }
        finally { tokenLock.Release(); }
    }
    public async Task<(int Status, string Body, double Milliseconds)> SendAsync(string method, string path, string? authorization = null)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "POST") request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        if (authorization != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
        var timer = Stopwatch.StartNew();
        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync(); timer.Stop();
        return ((int)response.StatusCode, body, timer.Elapsed.TotalMilliseconds);
    }
    internal static JObject SuccessfulJson((int Status, string Body, double Milliseconds) response)
    {
        Assert.Equal(200, response.Status);
        Assert.False(string.IsNullOrWhiteSpace(response.Body), "Empty HTTP 200 response.");
        Assert.True(response.Milliseconds <= Settings.MaxMilliseconds, $"Response exceeded {Settings.MaxMilliseconds} ms; took {response.Milliseconds:F0} ms.");
        var data = Assert.IsType<JObject>(JToken.Parse(response.Body));
        var error = data["error"];
        Assert.True(error == null || error.Type == JTokenType.Null || error.ToString() == "ok", "Provider returned an application error; value omitted.");
        Assert.NotNull(data["result"]);
        Assert.NotEqual(JTokenType.Null, data["result"]!.Type);
        return data;
    }
    public void Dispose() { client.Dispose(); tokenLock.Dispose(); }
}

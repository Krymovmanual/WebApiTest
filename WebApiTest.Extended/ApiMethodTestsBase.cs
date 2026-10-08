using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[CollectionDefinition("API methods", DisableParallelization = true)]
public sealed class ApiMethodsCollection : ICollectionFixture<ApiHarness> { }

public sealed class ApiMethodFactAttribute : FactAttribute
{
    public ApiMethodFactAttribute(string key)
    {
        if (!Settings.Live) { Skip = "Set WEBAPI_RUN_LIVE=1 or use dev.runsettings."; return; }
        // Bad configuration must fail during execution instead of being counted as a skip.
        try
        {
            var missing = ApiMethodRequests.Missing(ApiMethodRequests.Load(key));
            if (missing.Count != 0)
                Skip = $"{key}: configure {string.Join(", ", missing)} in api-requests.local.json (see api-requests.example.json).";
        }
        catch { }
    }
}

public abstract class ApiMethodTestsBase(ApiHarness api, ITestOutputHelper output)
{
    protected async Task CallAsync(string key)
    {
        var request = ApiMethodRequests.Load(key);
        var missing = ApiMethodRequests.Missing(request);
        Assert.True(missing.Count == 0, "Missing request inputs: " + string.Join(", ", missing));
        var path = request.Value<string>("path")!;
        foreach (var value in ((JObject)request["pathValues"]!).Properties())
            path = path.Replace("{" + value.Name + "}", Uri.EscapeDataString(value.Value.ToString()), StringComparison.Ordinal);
        Assert.DoesNotContain("{", path);
        var query = ((JObject)request["query"]!).Properties().Where(p => p.Value.Type != JTokenType.Null)
            .Select(p => Uri.EscapeDataString(p.Name) + "=" + Uri.EscapeDataString(p.Value.ToString()));
        var queryString = string.Join("&", query);
        if (queryString.Length != 0) path += "?" + queryString;
        var authorization = request.Value<bool>("anonymous") ? null : await TestReport.GetTokenAsync(api, output);
        var body = request["body"];
        var response = await TestReport.SendAsync(api, output, request.Value<string>("method")!, path,
            authorization, body?.Type == JTokenType.Null ? null : body?.ToString(Formatting.None));
        var diagnostic = $"{key}: HTTP {response.Status}\n{TestReport.JsonForOutput(response.Body, authorization)}";
        Assert.True(response.Status == 200, diagnostic);
        if (string.IsNullOrWhiteSpace(response.Body) || request.Value<bool>("anonymous")) return;
        JToken data;
        try { data = ApiHarness.ParseWireJson(response.Body); }
        catch (JsonException) { Assert.Fail("Expected JSON response.\n" + diagnostic); return; }
        if (data is JObject obj)
        {
            var error = obj["error"];
            Assert.True(error == null || error.Type == JTokenType.Null || error.Type == JTokenType.Boolean && !error.Value<bool>() ||
                string.Equals(error.ToString(), "ok", StringComparison.OrdinalIgnoreCase), "Provider error.\n" + diagnostic);
            var httpStatus = obj["httpStatus"];
            Assert.True(httpStatus == null || !int.TryParse(httpStatus.ToString(), out var status) || status < 400,
                "Provider reported an unsuccessful HTTP status.\n" + diagnostic);
            Assert.True(obj["isSuccess"]?.Type != JTokenType.Boolean || obj.Value<bool>("isSuccess"), "Provider reported isSuccess=false.\n" + diagnostic);
        }
        output.WriteLine("Request succeeded. Full response is above.");
    }
}

internal static class ApiMethodRequests
{
    internal static JObject Load(string key)
    {
        var catalog = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "api-methods.json")));
        var request = (JObject)(catalog[key]?.DeepClone() ?? throw new InvalidOperationException("Unknown API method: " + key));
        var configPath = Environment.GetEnvironmentVariable("WEBAPI_REQUESTS_FILE") ?? FindLocalFile();
        if (configPath != null)
        {
            var overrides = JObject.Parse(File.ReadAllText(configPath));
            if (overrides[key] is JObject configured)
                foreach (var field in new[] { "body", "pathValues", "query" })
                    if (configured[field] is JObject values && request[field] is JObject defaults)
                        defaults.Merge(values, new JsonMergeSettings { MergeNullValueHandling = MergeNullValueHandling.Merge });
        }
        SetPathFromEnvironment(request, "currency", "WEBAPI_BITOLO_CURRENCY");
        SetPathFromEnvironment(request, "blockchain", "WEBAPI_BLOCKCHAIN");
        if (key.Contains("/Bitolo/", StringComparison.Ordinal) && key.EndsWith("getTransactionStatus", StringComparison.Ordinal))
            SetBodyFromEnvironment(request, "order_id", "WEBAPI_BITOLO_ORDER_ID");
        if (key.Contains("/CoinPaymentsProvider/", StringComparison.Ordinal))
        {
            SetBodyFromEnvironment(request, "key", "WEBAPI_CP_PUBLIC_KEY_ALIAS");
            SetBodyFromEnvironment(request, "PrivateKey", "WEBAPI_CP_PRIVATE_KEY_ALIAS");
        }
        return request;
    }

    internal static List<string> Missing(JObject request)
    {
        var result = new List<string>();
        foreach (var field in request["required"]!.Values<string>())
        {
            var selector = field!.Replace("path.", "pathValues.", StringComparison.Ordinal);
            var value = request.SelectToken(selector);
            if (value == null || value.Type == JTokenType.Null || string.IsNullOrWhiteSpace(value.ToString())) result.Add(field);
        }
        return result;
    }

    private static string? FindLocalFile()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            foreach (var path in new[] { Path.Combine(directory.FullName, "api-requests.local.json"),
                Path.Combine(directory.FullName, "WebApiTest.Extended", "api-requests.local.json") })
                if (File.Exists(path)) return path;
            directory = directory.Parent;
        }
        return null;
    }

    private static void SetPathFromEnvironment(JObject request, string field, string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (request.Value<string>("path")!.Contains("{" + field + "}", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(value))
            request["pathValues"]![field] = value;
    }

    private static void SetBodyFromEnvironment(JObject request, string field, string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrWhiteSpace(value)) request["body"]![field] = value;
    }
}

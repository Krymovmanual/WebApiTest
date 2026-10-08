using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

// Print complete JSON bodies, masking credential fields without changing the assertion input.
internal static class TestReport
{
    internal static async Task<string> GetTokenAsync(ApiHarness api, ITestOutputHelper output)
    {
        output.WriteLine("Obtaining API authorization from configured token/credentials. Values omitted.");
        try { return await api.GetTokenAsync(); }
        catch (Exception ex)
        {
            output.WriteLine("API authorization acquisition failed: {0}. See test failure; no token or response body is printed.", ex.GetType().Name);
            throw;
        }
    }

    private static readonly HashSet<string> Fields = new(StringComparer.Ordinal)
    {
        "id", "_id", "account_id", "transaction_id", "order_id", "amount_net", "code", "assetId", "symbol", "type", "status", "operation", "decimals",
        "total", "balance", "available", "pending", "frozen", "lockedAmount", "staked",
        "amount", "requestedAmount", "netAmount", "fee", "networkFee", "serviceFee", "feeCurrency",
        "currency", "currencyCode", "availableBalance", "currentBalance", "totalElements", "number", "size", "errCode", "isSuccess",
        "page", "total_pages", "min", "max", "createdAt", "lastUpdated", "hiddenOnUI", "autoFuel"
    };

    internal static async Task<(int Status, string Body, double Milliseconds)> SendAsync(
        ApiHarness api, ITestOutputHelper output, string method, string path,
        string? authorization = null, string? jsonBody = null, int[]? expectedStatuses = null)
    {
        output.WriteLine("Request: {0} {1}", method, path);
        var sentBody = jsonBody ?? (method is "POST" or "PUT" or "PATCH" ? "{}" : null);
        if (sentBody != null) output.WriteLine("Request JSON:\n{0}", JsonForOutput(sentBody, authorization));
        var expected = expectedStatuses ?? [200];
        output.WriteLine("Scenario: {0}; expected HTTP {1}.",
            authorization == null ? "anonymous request" : "bearer supplied (value omitted)", string.Join(" or ", expected));
        (int Status, string Body, double Milliseconds) response;
        try { response = await api.SendAsync(method, path, authorization, jsonBody); }
        catch (Exception ex)
        {
            output.WriteLine("Request failed before a complete response: {0}. Details omitted.", ex.GetType().Name);
            throw;
        }
        output.WriteLine("HTTP {0}; elapsed {1:F0} ms; configured budget {2:F0} ms.",
            response.Status, response.Milliseconds, Settings.MaxMilliseconds);
        ReportStatus(output, response.Status, expected);
        output.WriteLine("Response body (HTTP {0}):\n{1}", response.Status, JsonForOutput(response.Body, authorization));
        return response;
    }

    internal static string JsonForOutput(string body, string? authorization = null)
    {
        if (string.IsNullOrWhiteSpace(body)) return "[empty body]";
        try
        {
            var value = ApiHarness.ParseWireJson(body);
            Redact(value, authorization);
            return value.ToString(Formatting.Indented);
        }
        catch (JsonException)
        {
            // Preserve non-JSON error diagnostics, removing embedded credentials where identifiable.
            var text = MaskEmbeddedSecrets(body, authorization);
            return "[non-JSON body]\n" + text;
        }
    }

    private static bool SecretField(string name)
    {
        var key = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        return key.Contains("password") || key.Contains("secret") || key.Contains("privatekey") ||
            key.Contains("apikey") || key.EndsWith("token", StringComparison.Ordinal) || key is "jwt" or "authorization" or
            "bearer" or "key" or "signature" or "clientassertion" or "credential" or "credentials";
    }

    private static void Redact(JToken token, string? authorization)
    {
        if (token is JObject obj)
            foreach (var property in obj.Properties().ToArray())
            {
                if (SecretField(property.Name)) property.Value = "[REDACTED]";
                else Redact(property.Value, authorization);
            }
        else if (token is JArray array)
            foreach (var child in array) Redact(child, authorization);
        else if (token is JValue { Type: JTokenType.String } scalar)
            scalar.Value = MaskEmbeddedSecrets(scalar.Value<string>()!, authorization);
    }

    private static string MaskEmbeddedSecrets(string text, string? authorization)
    {
        if (!string.IsNullOrEmpty(authorization)) text = text.Replace(authorization, "[REDACTED]", StringComparison.Ordinal);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?i)Bearer\s+[^\s""<>]+", "Bearer [REDACTED]");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\beyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\b", "[REDACTED]");
        return text;
    }

    internal static void ReportStatus(ITestOutputHelper output, int actual, int[] expected)
    {
        output.WriteLine("HTTP expectation: {0}; expected {1}; actual {2}. Body/data assertions run separately.",
            expected.Contains(actual) ? "MATCH" : "MISMATCH", string.Join(" or ", expected), actual);
        if (actual == 401 && !expected.Contains(401))
            output.WriteLine("Unexpected authorization rejection: check API environment and configured token/credentials. This is not a provider-data success.");
    }

    internal static void Describe(ITestOutputHelper output, string label, JToken token, bool names = false)
    {
        if (token is JArray array)
        {
            output.WriteLine("{0}: {1} items", label, array.Count);
            for (var i = 0; i < array.Count; i++) Describe(output, $"{label}[{i}]", array[i], names);
        }
        else if (token is JObject obj)
        {
            var fields = obj.Properties().Where(p => Fields.Contains(p.Name) || (names && p.Name == "name"))
                .Where(p => p.Value is JValue).Select(p => $"{p.Name}={p.Value.ToString(Formatting.None)}");
            output.WriteLine("{0}: {1}", label, string.Join("; ", fields));
            foreach (var property in obj.Properties().Where(p => p.Value is JArray || p.Value is JObject))
                if (property.Name is "assets" or "accounts" or "transactions" or "tokens" or "limits" or "message" or "data" or "amountInfo" or "feeInfo" or "content" or "items" or "balances" or "results" or "event")
                    Describe(output, $"{label}.{property.Name}", property.Value, names);
        }
        else output.WriteLine("{0}: {1}", label, token.Type);
    }
}

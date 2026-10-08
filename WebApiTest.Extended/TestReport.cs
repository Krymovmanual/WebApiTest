using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

// Print selected response fields, never raw bodies, request bodies, credentials or headers.
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
        "id", "_id", "account_id", "assetId", "symbol", "type", "status", "operation", "decimals",
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
        if (response.Status != 200) { output.WriteLine("Response body omitted. See assertion for expected status."); return response; }
        try
        {
            if (ApiHarness.ParseWireJson(response.Body) is not JObject body) return response;
            var error = body["error"];
            output.WriteLine("Application error: {0}.", error == null || error.Type == JTokenType.Null || error.ToString() == "ok" ? "none" : "present (text omitted)");
            var result = body["result"];
            if (path.Contains("getToken", StringComparison.Ordinal) || path.Contains("getSessionToken", StringComparison.Ordinal))
                output.WriteLine("Provider token/session response received. Secret and user fields omitted; assertions validate required fields.");
            else if (path.EndsWith("getExchangeRates", StringComparison.Ordinal) && result is JObject ratesWrapper && ratesWrapper["data"] is JObject rates)
            {
                output.WriteLine("Exchange rates: {0}", rates.Count);
                foreach (var rate in rates.Properties())
                    if (decimal.TryParse(rate.Value.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                        output.WriteLine("{0} = {1}", JsonConvert.ToString(rate.Name), rate.Value.ToString(Formatting.None));
            }
            else if (path.EndsWith("getBankAccounts", StringComparison.Ordinal))
            {
                var accounts = (result as JObject)?["data"] as JArray;
                output.WriteLine("Bank accounts: {0}. Account numbers and owner details omitted.", accounts?.Count ?? 0);
                if (accounts != null)
                    foreach (var account in accounts.OfType<JObject>())
                        output.WriteLine("Account: id={0}; currency={1}; account_type={2}",
                            SafeScalar(account["id"]), SafeScalar(account["currency"]), SafeScalar(account["account_type"]));
            }
            else if (result != null) Describe(output, "result", result, path.Contains("FireblocksProvider", StringComparison.Ordinal) || path.EndsWith("getAllCurrencyTokenPairs", StringComparison.Ordinal));
            else output.WriteLine("No result wrapper; body omitted.");
        }
        catch (JsonException) { output.WriteLine("Response is not valid JSON; body omitted."); }
        return response;
    }

    private static string SafeScalar(JToken? value) => value is JValue ? value.ToString(Formatting.None) : "[missing/structured]";

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
                if (property.Name is "assets" or "accounts" or "transactions" or "tokens" or "limits" or "message" or "data" or "amountInfo" or "feeInfo" or "content" or "items" or "balances" or "results")
                    Describe(output, $"{label}.{property.Name}", property.Value, names);
        }
        else output.WriteLine("{0}: {1}", label, token.Type);
    }
}

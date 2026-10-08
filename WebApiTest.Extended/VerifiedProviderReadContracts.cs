using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Globalization;

namespace WebApiTest.Extended;

public sealed class VerifiedReadFactAttribute : FactAttribute
{
    public VerifiedReadFactAttribute(string key)
    {
        if (!Settings.Live) { Skip = "Set WEBAPI_RUN_LIVE=1 to run API integration tests."; return; }
        try
        {
            var profile = VerifiedReadProfile.Find();
            if (profile == null) { Skip = "Missing local verified provider read profile."; return; }
            var loaded = VerifiedReadProfile.Load(profile);
            VerifiedReadProfile.ValidateCatalog(loaded);
            if (loaded["cases"]?[key] == null)
                Skip = "Missing verified body/assertions for " + key + ". Configure provider-read.local.json or WEBAPI_PROVIDER_READ_FILE.";
        }
        catch { /* Invalid configured files must fail visibly during execution, not become skips. */ }
    }
}

public abstract class VerifiedProviderReadBase(ApiHarness api, ITestOutputHelper output)
{
    protected async Task Read(string key)
    {
        var file = VerifiedReadProfile.Find();
        Assert.True(file != null, "Verified provider read configuration is missing.");
        var profile = VerifiedReadProfile.Load(file!);
        VerifiedReadProfile.ValidateCatalog(profile);
        var spec = Assert.IsType<JObject>(profile["cases"]?[key]);
        var route = VerifiedReadProfile.Route(key, spec);
        var checks = VerifiedReadProfile.Checks(spec);
        output.WriteLine("Configured read assertions: {0}. Request body and configured comparison values omitted.", checks.Count);
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", route,
            await TestReport.GetTokenAsync(api, output), spec["body"]?.ToString(Formatting.None)));
        ProviderBatchChecks.SuccessFlags(data);
        VerifiedReadChecks.Validate(data, checks);
        output.WriteLine("Verified configured response types/content/counts/identity rules. This does not prove database reconciliation or unconfigured fields.");
    }
}

internal static class VerifiedReadProfile
{
    internal static readonly Dictionary<string, (string Route, bool Body)> Catalog = new(StringComparer.Ordinal)
    {
        ["BitoloBalance"] = ("/api/Bitolo/{currency}/getBalance", false),
        ["BitoloTransactionStatus"] = ("/api/Bitolo/{currency}/getTransactionStatus", true),
        ["BitoloTransactionList"] = ("/api/Bitolo/{currency}/getTransactionList", true),
        ["FacilitaPayBankAccount"] = ("/api/FacilitaPayProvider/getBankAccount", true),
        ["FacilitaPayBankAccountStatement"] = ("/api/FacilitaPayProvider/getBankAccountStatement", true),
        ["FacilitaPayBankAccountBalance"] = ("/api/FacilitaPayProvider/getBankAccountBalance", true),
        ["FacilitaPayTransactions"] = ("/api/FacilitaPayProvider/getTransactions", true),
        ["FacilitaPayTransaction"] = ("/api/FacilitaPayProvider/getTransaction", true),
        ["OpenPaydHistory"] = ("/api/OpenPaydProvider/getHistory", true),
        ["OpenPaydTransactionInfo"] = ("/api/OpenPaydProvider/getTransactionInfo", true),
        ["OpenPaydAccount"] = ("/api/OpenPaydProvider/getAccount", true),
        ["OpenPaydAccounts"] = ("/api/OpenPaydProvider/getAccounts", true),
        ["NuveiV2PayoutStatus"] = ("/api/Nuvei_v2_Provider/getPayoutStatus", true),
        ["NuveiV2Requests"] = ("/api/Nuvei_v2_Provider/getRequests", true),
    };

    internal static string? Find()
    {
        var supplied = Environment.GetEnvironmentVariable("WEBAPI_PROVIDER_READ_FILE");
        if (!string.IsNullOrWhiteSpace(supplied))
        {
            Assert.True(Path.IsPathFullyQualified(supplied), "WEBAPI_PROVIDER_READ_FILE must be an absolute file path.");
            Assert.True(File.Exists(supplied), "Configured provider read file does not exist.");
            return supplied;
        }
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var path = Path.Combine(directory.FullName, "provider-read.local.json");
            if (File.Exists(path)) return path;
            directory = directory.Parent;
        }
        return null;
    }

    internal static JObject Load(string file)
    {
        try { return Assert.IsType<JObject>(ApiHarness.ParseWireJson(File.ReadAllText(file))); }
        catch (JsonException) { throw new InvalidOperationException("Invalid provider read JSON configuration; content omitted."); }
    }

    internal static void ValidateCatalog(JObject profile)
    {
        Assert.True(profile.Value<int?>("version") == 1, "Expected provider read profile version 1.");
        var cases = Assert.IsType<JObject>(profile["cases"]);
        foreach (var entry in cases.Properties())
            Assert.True(Catalog.ContainsKey(entry.Name), "Profile contains an unsupported operation; no request sent.");
    }

    internal static string Route(string key, JObject spec)
    {
        Assert.True(Catalog.TryGetValue(key, out var route), "Unknown read operation.");
        Assert.True(spec["path"] == null && spec["method"] == null, "Routes/methods are fixed by the read catalog.");
        if (route.Body) Assert.IsType<JObject>(spec["body"]);
        else Assert.True(spec["body"] == null, "This Swagger read operation declares no body.");
        if (!route.Route.Contains("{currency}")) return route.Route;
        var currency = ProviderReadContracts.RequiredString(spec, "currency");
        Assert.True(System.Text.RegularExpressions.Regex.IsMatch(currency, "^[A-Z]{3}$"), "Configure a three-letter uppercase Bitolo currency.");
        return route.Route.Replace("{currency}", Uri.EscapeDataString(currency));
    }

    internal static JArray Checks(JObject spec)
    {
        var checks = Assert.IsType<JArray>(spec["checks"]);
        Assert.NotEmpty(checks);
        Assert.Contains(checks, c => c.Value<string>("op") is "equals" or "number" or "count" or "unique" or "nonEmpty");
        foreach (var check in checks) VerifiedReadChecks.ValidateRule(Assert.IsType<JObject>(check));
        return checks;
    }
}

// A deliberately small assertion language, not an inferred schema or a snapshot comparison.
// JSONPath selectors must select within result; actual values are never included in failures.
internal static class VerifiedReadChecks
{
    internal static void ValidateRule(JObject rule)
    {
        var selector = ProviderReadContracts.RequiredString(rule, "select");
        Assert.True(selector == "$.result" || selector.StartsWith("$.result.", StringComparison.Ordinal) || selector.StartsWith("$.result[", StringComparison.Ordinal), "Checks must select inside $.result.");
        try { _ = new JObject().SelectTokens(selector).ToArray(); }
        catch (JsonException) { throw new InvalidOperationException("Invalid result selector; configured text omitted."); }
        var op = ProviderReadContracts.RequiredString(rule, "op");
        Assert.Contains(op, new[] { "type", "nonEmpty", "equals", "number", "count", "unique" });
        if (op == "equals") Assert.NotNull(rule["value"]);
        if (op == "type") Assert.Contains(rule.Value<string>("value"), new[] { "object", "array", "string", "boolean", "integer", "number" });
        if (op == "count")
        {
            Assert.True(rule["min"]?.Type == JTokenType.Integer && rule.Value<int>("min") >= 0, "Count needs a nonnegative integer minimum.");
            if (rule["max"] != null) Assert.True(rule["max"]!.Type == JTokenType.Integer && rule.Value<int>("max") >= rule.Value<int>("min"), "Invalid count maximum.");
        }
        if (op == "number")
            foreach (var bound in new[] { "min", "max" })
                if (rule[bound] is JToken value) Decimal(value);
        if (op == "number" && rule["min"] != null && rule["max"] != null)
            Assert.True(Decimal(rule["min"]!) <= Decimal(rule["max"]!), "Invalid numeric bounds.");
    }

    internal static void Validate(JObject response, JArray checks)
    {
        foreach (var token in checks)
        {
            var rule = Assert.IsType<JObject>(token); ValidateRule(rule);
            JToken[] values;
            try { values = response.SelectTokens(rule.Value<string>("select")!).ToArray(); }
            catch (JsonException) { throw new InvalidOperationException("Invalid result selector; configured text omitted."); }
            Assert.NotEmpty(values);
            Assert.All(values, value => Assert.NotEqual(JTokenType.Null, value.Type));
            var op = rule.Value<string>("op");
            if (op == "unique")
            {
                Assert.All(values, value => Assert.True(value is JValue && !string.IsNullOrWhiteSpace(value.ToString()), "Unique identifiers must be nonempty scalars."));
                Assert.True(values.Distinct(JToken.EqualityComparer).Count() == values.Length, "Duplicate selected identifiers; values omitted.");
                continue;
            }
            foreach (var value in values)
            {
                switch (op)
                {
                    case "type":
                        var expected = rule.Value<string>("value");
                        Assert.True(expected switch {
                            "object" => value is JObject, "array" => value is JArray,
                            "string" => value.Type == JTokenType.String, "boolean" => value.Type == JTokenType.Boolean,
                            "integer" => value.Type == JTokenType.Integer,
                            "number" => value.Type is JTokenType.Integer or JTokenType.Float, _ => false }, "Selected value has wrong JSON type; value omitted.");
                        break;
                    case "equals": Assert.True(JToken.DeepEquals(value, rule["value"]), "Selected value differs from configured expectation; values omitted."); break;
                    case "nonEmpty": Assert.True(value is JContainer c ? c.HasValues : value.Type == JTokenType.String && !string.IsNullOrWhiteSpace(value.Value<string>()), "Selected value is empty or not a string/container."); break;
                    case "number":
                        var number = Decimal(value);
                        if (rule["min"] is JToken min) Assert.True(number >= Decimal(min), "Selected number is below configured minimum.");
                        if (rule["max"] is JToken max) Assert.True(number <= Decimal(max), "Selected number exceeds configured maximum.");
                        break;
                    case "count":
                        var array = Assert.IsType<JArray>(value);
                        Assert.True(array.Count >= rule.Value<int>("min"), "Collection is smaller than configured minimum.");
                        if (rule["max"] != null) Assert.True(array.Count <= rule.Value<int>("max"), "Collection exceeds configured maximum.");
                        break;
                }
            }
        }
    }

    private static decimal Decimal(JToken value)
    {
        Assert.True(value.Type is JTokenType.String or JTokenType.Float or JTokenType.Integer, "Expected numeric scalar; value omitted.");
        Assert.True(decimal.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed), "Invalid invariant decimal; value omitted.");
        return parsed;
    }
}

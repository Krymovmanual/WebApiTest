using System.Globalization;
using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

[CollectionDefinition("Stage reads", DisableParallelization = true)]
public class StageReadCollection : ICollectionFixture<ApiHarness> { }

[Collection("Stage reads")]
public class ProviderReadContracts(ApiHarness api)
{
    public static readonly string[] Paths = [
        "/api/FireblocksProvider/getAssets", "/api/FireblocksProvider/getExchangeAccounts",
        "/api/FireblocksProvider/getFiatAccounts", "/api/FireblocksProvider/getInternalWallets",
        "/api/FireblocksProvider/getExternalWallets", "/api/FacilitaPayProvider/getExchangeRates",
        "/api/FacilitaPayProvider/getBankAccounts", "/api/KoyweProvider/getAllCurrencyTokenPairs"];
    public static IEnumerable<object[]> ReadCases => Paths.Select(p => new object[] { p });

    [LiveTheory, MemberData(nameof(ReadCases)), Trait("Suite", "Live")]
    public async Task ReadResponsesMatchKnownWrapperContract(string path)
    {
        var data = ApiHarness.SuccessfulJson(await api.SendAsync("POST", path, await api.GetTokenAsync()));
        var result = data["result"]!;
        if (path.Contains("FireblocksProvider"))
        {
            var items = Assert.IsType<JArray>(result);
            // Empty configured account/wallet collections are valid.
            if (path.EndsWith("getAssets")) Assert.NotEmpty(items);
            UniqueIds(items, "id");
            foreach (var item in items)
            {
                Assert.IsType<JObject>(item); RequiredString(item, "id"); RequiredString(item, "name");
                if (path.EndsWith("getAssets"))
                {
                    Assert.Equal(JTokenType.Integer, item["decimals"]?.Type);
                    Assert.InRange(item.Value<int>("decimals"), 0, int.MaxValue);
                    // No Ethereum-only address regex: Fireblocks also supports non-EVM assets.
                }
                else if (path.EndsWith("getExchangeAccounts"))
                {
                    RequiredString(item, "type");
                    foreach (var asset in Assert.IsType<JArray>(item["assets"]))
                    {
                        RequiredString(asset, "id");
                        foreach (var field in new[] { "total", "balance", "lockedAmount", "available" }) NonNegativeNumber(asset[field]);
                    }
                }
                else if (path.EndsWith("getInternalWallets") || path.EndsWith("getExternalWallets"))
                {
                    var assets = Assert.IsType<JArray>(item["assets"]); UniqueIds(assets, "id");
                    foreach (var asset in assets) RequiredString(asset, "id");
                    // Status enums and ID formats are not restricted to values from one Stage dataset.
                }
            }
        }
        else if (path.EndsWith("getExchangeRates"))
        {
            var rates = Assert.IsType<JObject>(result["data"]); Assert.NotEmpty(rates.Properties());
            foreach (var rate in rates.Properties()) Assert.True(NonNegativeNumber(rate.Value) > 0, "Exchange rate must be positive.");
        }
        else if (path.EndsWith("getBankAccounts"))
        {
            var accounts = Assert.IsType<JArray>(result["data"]); UniqueIds(accounts, "id");
            foreach (var account in accounts) Assert.IsType<JObject>(account);
        }
        else
        {
            var pairs = Assert.IsType<JArray>(result); Assert.NotEmpty(pairs); UniqueIds(pairs, "_id");
            foreach (var pair in pairs)
            {
                RequiredString(pair, "_id"); RequiredString(pair, "name"); RequiredString(pair, "symbol");
                var limits = Assert.IsType<JObject>(pair["limits"]);
                var min = NonNegativeNumber(limits["min"]); var max = NonNegativeNumber(limits["max"]);
                Assert.True(min <= max, "Minimum limit exceeds maximum limit.");
                var tokens = Assert.IsType<JArray>(pair["tokens"]); Assert.NotEmpty(tokens); UniqueIds(tokens, "_id");
                foreach (var token in tokens) { RequiredString(token, "_id"); RequiredString(token, "name"); RequiredString(token, "symbol"); }
            }
        }
    }

    [LiveTheory, InlineData("/api/Version"), InlineData("/api/DeployDate"), Trait("Suite", "Live")]
    public async Task PublicInfoReturnsNonEmptySuccess(string path)
    {
        var response = await api.SendAsync("GET", path);
        Assert.Equal(200, response.Status); Assert.False(string.IsNullOrWhiteSpace(response.Body));
        Assert.True(response.Milliseconds <= Settings.MaxMilliseconds, $"{path} exceeded response-time budget.");
    }

    internal static string RequiredString(JToken item, string field)
    {
        Assert.Equal(JTokenType.String, item[field]?.Type);
        var value = item.Value<string>(field); Assert.False(string.IsNullOrWhiteSpace(value), $"Empty {field}."); return value!;
    }
    internal static decimal NonNegativeNumber(JToken? token)
    {
        Assert.NotNull(token);
        Assert.True(token!.Type is JTokenType.String or JTokenType.Float or JTokenType.Integer, "Expected decimal number or numeric string.");
        var text = token.Type == JTokenType.String ? token.Value<string>()! : token.ToString(Newtonsoft.Json.Formatting.None);
        Assert.True(decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value), "Invalid invariant decimal.");
        Assert.True(value >= 0, "Negative balance/rate/limit."); return value;
    }
    internal static void UniqueIds(JArray items, string field)
    {
        var ids = items.Select(item => { Assert.IsType<JObject>(item); return RequiredString(item, field); }).ToArray();
        Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
    }
}

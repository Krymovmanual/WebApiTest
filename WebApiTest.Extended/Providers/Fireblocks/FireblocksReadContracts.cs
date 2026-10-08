using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads"), Trait("Provider", "Fireblocks"), Trait("Suite", "Live")]
public class FireblocksReadContracts(ApiHarness api, ITestOutputHelper output)
{
    public static readonly string[] Paths = ["/api/FireblocksProvider/getAssets", "/api/FireblocksProvider/getExchangeAccounts",
        "/api/FireblocksProvider/getFiatAccounts", "/api/FireblocksProvider/getInternalWallets", "/api/FireblocksProvider/getExternalWallets",
        "/api/FireblocksProvider/getVaultAccountsPaged", "/api/FireblocksProvider/getTransactions"];

    private async Task<JArray> Read(string operation)
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/FireblocksProvider/" + operation, await TestReport.GetTokenAsync(api, output)));
        var items = Assert.IsType<JArray>(data["result"]);
        output.WriteLine("{0}: {1} items", operation, items.Count);
        return items;
    }

    [LiveFact]
    public async Task GetAssetsReturnsValidMetadata()
    {
        var items = await Read("getAssets");
        output.WriteLine("Fireblocks assets: {0}", items.Count);
        foreach (var item in items)
        {
            // JSON escaping preserves one output line even for names containing newlines.
            if (item is JObject asset)
                output.WriteLine("id={0}; name={1}", asset["id"]?.ToString(Formatting.None), asset["name"]?.ToString(Formatting.None));
        }
        FireblocksResponseChecks.Assets(items);
    }

    [LiveFact]
    public async Task GetExchangeAccountsReturnsValidBalances()
        => FireblocksResponseChecks.ExchangeAccounts(await Read("getExchangeAccounts"));

    [LiveFact]
    public async Task GetFiatAccountsReturnsValidAssets()
        => FireblocksResponseChecks.FiatAccounts(await Read("getFiatAccounts"));

    [LiveFact]
    public async Task GetInternalWalletsReturnsValidAssets()
        => FireblocksResponseChecks.Wallets(await Read("getInternalWallets"), false);

    [LiveFact]
    public async Task GetExternalWalletsReturnsValidAssets()
        => FireblocksResponseChecks.Wallets(await Read("getExternalWallets"), true);
}

internal static class FireblocksResponseChecks
{
    // Wrapper invariants already established by existing tests remain required.
    // Provider-only optional fields are checked when present; no EVM-only address regex.
    private static void Optional(JToken item, string field, JTokenType type)
    {
        var value = item[field];
        if (value != null && value.Type != JTokenType.Null)
            Assert.True(value.Type == type, $"{item.Path}.{field}: expected {type}, got {value.Type}.");
    }
    private static void OptionalNumber(JToken item, string field)
    {
        var value = item[field];
        if (value != null && value.Type != JTokenType.Null) ProviderReadContracts.NonNegativeNumber(value);
    }
    private static void Identity(JToken item)
    {
        Assert.IsType<JObject>(item);
        ProviderReadContracts.RequiredString(item, "id"); ProviderReadContracts.RequiredString(item, "name");
    }
    private static JArray NestedAssets(JToken item)
    {
        var assets = Assert.IsType<JArray>(item["assets"]);
        ProviderReadContracts.UniqueIds(assets, "id"); return assets;
    }
    public static void Assets(JArray items)
    {
        Assert.NotEmpty(items); ProviderReadContracts.UniqueIds(items, "id");
        foreach (var item in items)
        {
            Identity(item); ProviderReadContracts.RequiredString(item, "type");
            Assert.Equal(JTokenType.Integer, item["decimals"]?.Type);
            Assert.InRange(item.Value<int>("decimals"), 0, int.MaxValue);
            Optional(item, "contractAddress", JTokenType.String); Optional(item, "nativeAsset", JTokenType.String);
        }
    }
    public static void ExchangeAccounts(JArray items)
    {
        ProviderReadContracts.UniqueIds(items, "id");
        foreach (var item in items)
        {
            Identity(item); ProviderReadContracts.RequiredString(item, "type");
            Optional(item, "status", JTokenType.String); Optional(item, "success", JTokenType.Boolean);
            if (item["success"]?.Type == JTokenType.Boolean)
                Assert.True(item.Value<bool>("success"), $"Exchange account {item["id"]}: provider failed to retrieve balance data.");
            Optional(item, "isSubaccount", JTokenType.Boolean); Optional(item, "mainAccountId", JTokenType.String);
            foreach (var asset in NestedAssets(item))
            {
                foreach (var field in new[] { "total", "balance", "lockedAmount", "available" })
                    ProviderReadContracts.NonNegativeNumber(asset[field]);
                foreach (var field in new[] { "assetId", "assetLegacyId", "providerSymbol", "assetSymbol" })
                    Optional(asset, field, JTokenType.String);
            }
        }
    }
    public static void FiatAccounts(JArray items)
    {
        ProviderReadContracts.UniqueIds(items, "id");
        foreach (var item in items)
        {
            Identity(item); Optional(item, "type", JTokenType.String); Optional(item, "address", JTokenType.String);
            if (item["assets"] != null && item["assets"]!.Type != JTokenType.Null)
                foreach (var asset in NestedAssets(item)) OptionalNumber(asset, "balance");
        }
    }
    public static void Wallets(JArray items, bool external)
    {
        ProviderReadContracts.UniqueIds(items, "id");
        foreach (var item in items)
        {
            Identity(item); Optional(item, "customerRefId", JTokenType.String);
            foreach (var asset in NestedAssets(item))
            {
                foreach (var field in new[] { "address", "tag", "activationTime" }) Optional(asset, field, JTokenType.String);
                if (asset["status"] != null && asset["status"]!.Type != JTokenType.Null)
                    ProviderReadContracts.RequiredString(asset, "status");
                // Status enums can evolve; the wrapper historically also returned PENDING.
                // External-wallet balances are not a reliable financial balance source.
                if (!external) { OptionalNumber(asset, "balance"); OptionalNumber(asset, "lockedAmount"); }
            }
        }
    }
}

using System.Globalization;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads"), Trait("Provider", "Fireblocks"), Trait("Suite", "Live")]
public class FireblocksVaultContracts(ApiHarness api, ITestOutputHelper output)
{
    private const int Limit = 2;
    private async Task<JObject> Page(string? after = null)
    {
        var request = new JObject { ["limit"] = Limit, ["orderBy"] = "ASC" };
        if (after != null) request["after"] = after;
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/FireblocksProvider/getVaultAccountsPaged",
            await TestReport.GetTokenAsync(api, output), request.ToString()));
        var page = Assert.IsType<JObject>(data["result"]);
        FireblocksVaultChecks.Page(page, Limit);
        output.WriteLine("Vault page: {0} accounts; next cursor present: {1}",
            ((JArray)page["accounts"]!).Count, FireblocksVaultChecks.After(page) != null);
        return page;
    }

    [LiveFact]
    public async Task GetVaultAccountsPagedHonorsLimitAndReturnsValidBalances()
    {
        _ = await Page();
    }

    [LiveFact]
    public async Task GetVaultAccountsPagedAfterCursorAdvancesWithoutRepeatingAccounts()
    {
        var first = await Page();
        var after = FireblocksVaultChecks.After(first);
        if (after == null)
        {
            output.WriteLine("Only one page is available; cursor advancement was not exercised.");
            return;
        }
        // Reuse the wrapper route and opaque cursor. Never follow the upstream nextUrl with API credentials.
        var second = await Page(after);
        FireblocksVaultChecks.Advances(first, second);
    }
}

internal static class FireblocksVaultChecks
{
    public static string? After(JObject page)
    {
        var value = page["paging"]?["after"];
        if (value == null || value.Type == JTokenType.Null) return null;
        Assert.Equal(JTokenType.String, value.Type);
        var cursor = value.Value<string>();
        Assert.False(string.IsNullOrWhiteSpace(cursor), "Empty pagination cursor.");
        return cursor;
    }
    private static void OptionalString(JToken item, string field)
    {
        if (item[field] != null && item[field]!.Type != JTokenType.Null)
            Assert.Equal(JTokenType.String, item[field]!.Type);
    }
    public static void Page(JObject page, int limit)
    {
        var accounts = Assert.IsType<JArray>(page["accounts"]);
        Assert.InRange(accounts.Count, 0, limit);
        ProviderReadContracts.UniqueIds(accounts, "id");
        foreach (var account in accounts)
        {
            ProviderReadContracts.RequiredString(account, "name");
            OptionalString(account, "customerRefId");
            foreach (var field in new[] { "hiddenOnUI", "autoFuel" })
                Assert.Equal(JTokenType.Boolean, account[field]?.Type);
            var assets = Assert.IsType<JArray>(account["assets"]);
            ProviderReadContracts.UniqueIds(assets, "id");
            foreach (var asset in assets)
            {
                foreach (var field in new[] { "total", "lockedAmount", "available", "pending", "frozen", "staked" })
                    ProviderReadContracts.NonNegativeNumber(asset[field]);
                // The observed WebAPI omits balance, and accepts numeric JSON amounts.
                if (asset["balance"] != null && asset["balance"]!.Type != JTokenType.Null)
                    ProviderReadContracts.NonNegativeNumber(asset["balance"]);
                if (asset["blockHeight"] != null && asset["blockHeight"]!.Type != JTokenType.Null)
                {
                    Assert.Equal(JTokenType.String, asset["blockHeight"]!.Type);
                    Assert.True(long.TryParse(asset.Value<string>("blockHeight"), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out var height), "Invalid blockHeight.");
                    Assert.True(height >= -1, "Unexpected blockHeight below the -1 sentinel.");
                }
            }
        }
        var paging = Assert.IsType<JObject>(page["paging"]);
        OptionalString(paging, "before"); OptionalString(paging, "after");
        OptionalString(page, "previousUrl"); OptionalString(page, "nextUrl");
        _ = After(page);
        // Links are metadata; they may refer to provider origins and are not followed.
    }
    public static void Advances(JObject first, JObject second)
    {
        var ids = ((JArray)first["accounts"]!).Select(x => x.Value<string>("id")).ToHashSet(StringComparer.Ordinal);
        foreach (var account in (JArray)second["accounts"]!)
            Assert.False(ids.Contains(account.Value<string>("id")), "Cursor repeated an account from the previous page.");
        var next = After(second);
        if (next != null) Assert.NotEqual(After(first), next);
    }
}

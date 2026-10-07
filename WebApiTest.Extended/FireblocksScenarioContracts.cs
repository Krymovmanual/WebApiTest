using System.Globalization;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads"), Trait("Suite", "Live"), Trait("Provider", "Fireblocks")]
public class FireblocksScenarioContracts(ApiHarness api, ITestOutputHelper output)
{
    private const int VaultPageSize = 2;
    private const int HistoryLimit = 25;

    private async Task<JToken> Read(string operation, JObject? body = null)
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST",
            "/api/FireblocksProvider/" + operation, await TestReport.GetTokenAsync(api, output), body?.ToString()));
        return data["result"]!;
    }

    private async Task<JArray> Vaults() => await FireblocksScenarioChecks.TraverseVaults(async cursor =>
    {
        var body = new JObject { ["limit"] = VaultPageSize, ["orderBy"] = "ASC" };
        if (cursor != null) body["after"] = cursor;
        return Assert.IsType<JObject>(await Read("getVaultAccountsPaged", body));
    }, VaultPageSize, 50, (page, count, next) => output.WriteLine(
        "Vault traversal page {0}: {1} accounts; continuation: {2} (cursor omitted).", page, count, next));

    private async Task<JObject> History(JObject? filters = null, bool descending = true)
    {
        var request = new JObject { ["limit"] = HistoryLimit, ["orderBy"] = "createdAt", ["sort"] = descending ? "DESC" : "ASC" };
        if (filters != null) foreach (var property in filters.Properties()) request[property.Name] = property.Value.DeepClone();
        var result = Assert.IsType<JObject>(await Read("getTransactions", request));
        FireblocksTransactionChecks.History(result, HistoryLimit, descending);
        return result;
    }

    [LiveFact, Trait("Coverage", "CrossMethod")]
    public async Task CompleteVaultTraversalReferencesSupportedAssetsAndHistorySource()
    {
        var catalog = Assert.IsType<JArray>(await Read("getAssets"));
        FireblocksResponseChecks.Assets(catalog);
        var accounts = await Vaults();
        Assert.NotEmpty(accounts); // A zero-row DEV dataset cannot exercise relationships.
        var assetCount = FireblocksScenarioChecks.VaultAssetReferences(accounts, catalog);
        Assert.True(assetCount > 0, "Test-data prerequisite: at least one vault asset is required.");
        var history = await History();
        var transactions = (JArray)history["transactions"]!;
        Assert.NotEmpty(transactions);
        FireblocksScenarioChecks.TransactionAssetReferences(transactions, catalog);
        var anchor = transactions.FirstOrDefault(tx => tx["source"]?.Value<string>("type") == "VAULT_ACCOUNT" &&
            accounts.Any(account => account.Value<string>("id") == tx["source"]?.Value<string>("id") &&
                ((JArray)account["assets"]!).Any(asset => asset.Value<string>("id") == tx.Value<string>("assetId"))));
        Assert.True(anchor != null,
            "Test-data prerequisite: recent history must contain a transaction whose source vault and asset still exist. Historical deleted entities are not treated as API defects.");
        output.WriteLine("Relationships verified: {0} catalog assets; {1} vault accounts; {2} vault assets; {3} history transactions.",
            catalog.Count, accounts.Count, assetCount, transactions.Count);
        output.WriteLine("Linked transaction id={0}; source vault id={1}; assetId={2}. Historical amounts are not compared with today's balance.",
            anchor!["id"]!.ToString(Newtonsoft.Json.Formatting.None), anchor["source"]!["id"]!.ToString(Newtonsoft.Json.Formatting.None),
            anchor["assetId"]!.ToString(Newtonsoft.Json.Formatting.None));
    }

    [LiveTheory, Trait("Coverage", "WrapperFilterCandidate")]
    [InlineData("assets")]
    [InlineData("source")]
    [InlineData("status")]
    [InlineData("txHash")]
    [InlineData("timeWindow")]
    public async Task HistoryFiltersReturnKnownAnchorAndOnlyMatchingTransactions(string filter)
    {
        output.WriteLine("Provider-documented filter candidate: {0}. Wrapper forwarding requires this live check; not previously verified.", filter);
        var history = await History();
        var candidates = (JArray)history["transactions"]!;
        // Completed transactions provide a stable status and immutable anchor during repeated reads.
        var anchor = candidates.FirstOrDefault(tx => tx.Value<string>("status") == "COMPLETED" &&
            (filter != "txHash" || !string.IsNullOrWhiteSpace(tx.Value<string>("txHash"))) &&
            (filter != "source" || !string.IsNullOrWhiteSpace(tx["source"]?.Value<string>("id"))) &&
            candidates.Any(other => FireblocksScenarioChecks.Contrasts(tx, other, filter)));
        Assert.True(anchor != null, "Test-data prerequisite: a recent completed transaction plus a contrasting record is required to detect an ignored filter.");
        var request = FireblocksScenarioChecks.AnchorFilter(anchor!, filter, candidates);
        var filtered = (JArray)(await History(request))["transactions"]!;
        FireblocksScenarioChecks.FilterMatches(filtered, anchor!, filter, request);
        output.WriteLine("Filter {0}: {1} results; known completed transaction retained; every result matches the requested filter.", filter, filtered.Count);
    }

    [LiveFact, Trait("Coverage", "WrapperCursorCandidate")]
    public async Task HistoryNextPageDoesNotRepeatTransactionsOrReverseOrder()
    {
        output.WriteLine("Transaction next-cursor forwarding is a provider-based wrapper candidate pending live verification.");
        var first = await History();
        var continuation = FireblocksScenarioChecks.HistoryContinuation(first);
        Assert.True(continuation != null, "Test-data prerequisite: history must have a second page to exercise cursor forwarding.");
        var second = await History(continuation);
        FireblocksScenarioChecks.HistoryAdvances(first, second);
        output.WriteLine("History cursor advanced; page 1 and page 2 have distinct IDs and continuous createdAt DESC order.");
    }

    [LiveTheory, InlineData(true), InlineData(false), Trait("Coverage", "Ordering")]
    public async Task HistorySortDirectionIsAppliedWithMultipleTransactions(bool descending)
    {
        var result = await History(descending: descending);
        var rows = (JArray)result["transactions"]!;
        Assert.True(rows.Count >= 2, "Test-data prerequisite: at least two transactions are needed to verify ordering.");
        Assert.True(rows.Select(tx => tx.Value<long>("createdAt")).Distinct().Count() >= 2,
            "Test-data prerequisite: equal timestamps cannot distinguish ASC from DESC.");
        output.WriteLine("createdAt {0} ordering verified across {1} transactions.", descending ? "DESC" : "ASC", rows.Count);
    }
}

internal static class FireblocksScenarioChecks
{
    public static async Task<JArray> TraverseVaults(Func<string?, Task<JObject>> fetch, int limit, int maxPages,
        Action<int, int, bool>? report = null)
    {
        Assert.True(limit > 0 && maxPages > 0);
        var accounts = new JArray();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var seenCursors = new HashSet<string>(StringComparer.Ordinal);
        string? cursor = null;
        for (var number = 1; number <= maxPages; number++)
        {
            var page = await fetch(cursor);
            FireblocksVaultChecks.Page(page, limit);
            var rows = (JArray)page["accounts"]!;
            foreach (var account in rows)
            {
                var id = ProviderReadContracts.RequiredString(account, "id");
                Assert.True(seenIds.Add(id), "Vault pagination repeated an account, including nonadjacent pages.");
                accounts.Add(account.DeepClone());
            }
            var next = FireblocksVaultChecks.After(page);
            report?.Invoke(number, rows.Count, next != null);
            if (next == null) return accounts;
            Assert.True(rows.Count > 0, "Empty vault page returned a continuation cursor; traversal is incomplete.");
            Assert.True(seenCursors.Add(next), "Vault pagination cursor cycle detected; cursor value omitted.");
            cursor = next;
        }
        Assert.Fail($"Vault traversal did not reach the final page within {maxPages} requests. Partial coverage is not counted as success.");
        return accounts;
    }

    public static int VaultAssetReferences(JArray accounts, JArray catalog)
    {
        var supported = catalog.Select(a => ProviderReadContracts.RequiredString(a, "id")).ToHashSet(StringComparer.Ordinal);
        var count = 0;
        foreach (var account in accounts)
            foreach (var asset in Assert.IsType<JArray>(account["assets"]))
            {
                var id = ProviderReadContracts.RequiredString(asset, "id");
                Assert.True(supported.Contains(id), "Vault asset missing from supported-asset catalog; inspect test output for IDs.");
                count++;
            }
        return count;
    }

    public static void TransactionAssetReferences(JArray transactions, JArray catalog)
    {
        var supported = catalog.Select(a => ProviderReadContracts.RequiredString(a, "id")).ToHashSet(StringComparer.Ordinal);
        foreach (var tx in transactions)
            Assert.True(supported.Contains(ProviderReadContracts.RequiredString(tx, "assetId")),
                "History asset missing from supported-asset catalog; inspect output. Check retired-asset behavior before classifying this as a defect.");
    }

    public static JObject? HistoryContinuation(JObject page)
    {
        var value = page["pageDetails"]?["nextPage"];
        if (value == null || value.Type == JTokenType.Null || value.Value<string>() == "") return null;
        Assert.Equal(JTokenType.String, value.Type);
        Assert.True(Uri.TryCreate(value.Value<string>(), UriKind.Absolute, out var uri) && uri.Scheme == "https" &&
            uri.Host == "api.fireblocks.io" && uri.AbsolutePath == "/v1/transactions" && uri.UserInfo == "" && uri.Fragment == "",
            "Unexpected provider pagination URL. URL is metadata and will not be followed.");
        var request = new JObject();
        foreach (var segment in uri!.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0].Replace("+", " "));
            var decoded = Uri.UnescapeDataString(parts.Length == 2 ? parts[1].Replace("+", " ") : "");
            // Only preserve pagination state; base request retains its verified limit/order/sort.
            if (key is "next" or "before" or "after")
            {
                Assert.True(request[key] == null, "Ambiguous duplicate pagination parameter.");
                request[key] = decoded;
            }
        }
        Assert.False(string.IsNullOrWhiteSpace(request.Value<string>("next")), "Next-page metadata lacks a usable cursor.");
        return request;
    }

    public static void HistoryAdvances(JObject first, JObject second)
    {
        var firstRows = Assert.IsType<JArray>(first["transactions"]);
        var secondRows = Assert.IsType<JArray>(second["transactions"]);
        Assert.NotEmpty(firstRows); Assert.NotEmpty(secondRows);
        var ids = firstRows.Select(tx => ProviderReadContracts.RequiredString(tx, "id")).ToHashSet(StringComparer.Ordinal);
        Assert.All(secondRows, tx => Assert.False(ids.Contains(ProviderReadContracts.RequiredString(tx, "id")),
            "History pagination repeated a transaction from the first page."));
        Assert.True(secondRows[0].Value<long>("createdAt") <= firstRows.Last!.Value<long>("createdAt"),
            "History page boundary violates createdAt DESC ordering.");
    }

    public static bool Contrasts(JToken anchor, JToken other, string filter) => filter switch
    {
        "assets" => other.Value<string>("assetId") != anchor.Value<string>("assetId"),
        "source" => other["source"]?.Value<string>("id") != anchor["source"]?.Value<string>("id") ||
            other["source"]?.Value<string>("type") != anchor["source"]?.Value<string>("type"),
        "status" => other.Value<string>("status") != "COMPLETED",
        "txHash" => other.Value<string>("txHash") != anchor.Value<string>("txHash"),
        "timeWindow" => Math.Abs((decimal)other.Value<long>("createdAt") - anchor.Value<long>("createdAt")) > 1,
        _ => throw new ArgumentOutOfRangeException(nameof(filter))
    };

    public static JObject AnchorFilter(JToken anchor, string filter, JArray candidates)
    {
        var created = anchor.Value<long>("createdAt");
        Assert.True(created > 0 && created < long.MaxValue, "Invalid anchor timestamp for a bounded history query.");
        // Non-time filters retain the contrasting seed records in the time range, so ignoring
        // the requested filter cannot pass merely because the range contained only the anchor.
        var lower = filter == "timeWindow" ? created : candidates.Min(tx => tx.Value<long>("createdAt"));
        var upper = filter == "timeWindow" ? created : candidates.Max(tx => tx.Value<long>("createdAt"));
        Assert.True(lower > 0 && upper < long.MaxValue);
        var request = new JObject { ["after"] = (lower - 1).ToString(CultureInfo.InvariantCulture),
            ["before"] = (upper + 1).ToString(CultureInfo.InvariantCulture) };
        switch (filter)
        {
            case "assets": request["assets"] = ProviderReadContracts.RequiredString(anchor, "assetId"); break;
            case "source":
                request["sourceType"] = ProviderReadContracts.RequiredString(anchor["source"]!, "type");
                request["sourceId"] = ProviderReadContracts.RequiredString(anchor["source"]!, "id"); break;
            case "status": request["status"] = "COMPLETED"; break;
            case "txHash": request["txHash"] = ProviderReadContracts.RequiredString(anchor, "txHash"); break;
            case "timeWindow": break;
            default: throw new ArgumentOutOfRangeException(nameof(filter));
        }
        return request;
    }

    public static void FilterMatches(JArray rows, JToken anchor, string filter, JObject request)
    {
        Assert.NotEmpty(rows);
        Assert.Contains(rows, tx => tx.Value<string>("id") == anchor.Value<string>("id"));
        var after = long.Parse(request.Value<string>("after")!, CultureInfo.InvariantCulture);
        var before = long.Parse(request.Value<string>("before")!, CultureInfo.InvariantCulture);
        foreach (var tx in rows)
        {
            Assert.True(tx.Value<long>("createdAt") > after && tx.Value<long>("createdAt") < before,
                "History timestamp filter was ignored.");
            switch (filter)
            {
                case "assets": Assert.Equal(anchor.Value<string>("assetId"), tx.Value<string>("assetId")); break;
                case "source":
                    Assert.Equal(anchor["source"]!.Value<string>("type"), tx["source"]?.Value<string>("type"));
                    Assert.Equal(anchor["source"]!.Value<string>("id"), tx["source"]?.Value<string>("id")); break;
                case "status": Assert.Equal("COMPLETED", tx.Value<string>("status")); break;
                case "txHash": Assert.Equal(anchor.Value<string>("txHash"), tx.Value<string>("txHash")); break;
                case "timeWindow": break;
                default: throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }
    }
}

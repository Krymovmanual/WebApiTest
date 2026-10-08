using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class CoinPaymentsReadContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JToken> Read(string method, JObject? body = null)
    {
        // These are backend credential aliases supplied in working DEV requests, not actual keys.
        var request = body ?? new JObject();
        request["key"] = Environment.GetEnvironmentVariable("WEBAPI_CP_PUBLIC_KEY_ALIAS") ?? "mCoinPublicKey";
        request["PrivateKey"] = Environment.GetEnvironmentVariable("WEBAPI_CP_PRIVATE_KEY_ALIAS") ?? "mCoinPrivateKey";
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST",
            "/api/CoinPaymentsProvider/" + method, await TestReport.GetTokenAsync(api, output), request.ToString(Formatting.None)));
        ProviderBatchChecks.SuccessFlags(data);
        return data["result"]!;
    }

    private async Task<JArray> History(int start, int limit) => CoinPaymentsChecks.History(
        await Read("getWithdrawalHistory", new JObject { ["Limit"] = limit, ["Start"] = start, ["Newer"] = 0 }), limit);

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "CoinPayments")]
    public async Task BalancesHaveValidNumericFields()
    {
        var balances = CoinPaymentsChecks.Balances(await Read("getBalances", new JObject { ["all"] = 0 }));
        output.WriteLine("Validated {0} currency balances; amounts are not pinned to historical values.", balances.Count);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "CoinPayments")]
    public async Task ExchangeRatesHaveValidNumericStrings()
    {
        var rates = CoinPaymentsChecks.Rates(await Read("getExchangeRates"));
        output.WriteLine("Validated {0} rates, fiat flags, timestamps and fees. No market-feed reconciliation.", rates.Count);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "CoinPayments")]
    public async Task WithdrawalHistoryHasUniqueIdsAndValidAmounts()
    {
        var history = await History(0, 25);
        Assert.NotEmpty(history);
        output.WriteLine("Validated {0} withdrawal records, unique IDs, amounts and descending creation times.", history.Count);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "CoinPayments"), Trait("Coverage", "Pagination")]
    public async Task OffsetReturnsDistinctConsecutivePages()
    {
        // Snapshot first 50 records to detect concurrent inserts before interpreting offset overlap.
        var before = await History(0, 50);
        Assert.True(before.Count > 25, "Pagination prerequisite: more than 25 withdrawals required.");
        var first = await History(0, 25);
        var second = await History(25, 25);
        var after = await History(0, 50);
        Assert.True(CoinPaymentsChecks.Ids(before).SequenceEqual(CoinPaymentsChecks.Ids(after)),
            "History changed during pagination check; rerun against stable data before reporting an API defect.");
        CoinPaymentsChecks.Pages(before, first, second);
        output.WriteLine("Offsets 0 and 25 match a stable 50-record snapshot, with no duplicate IDs across pages.");
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "CoinPayments"), Trait("Coverage", "CrossMethod")]
    public async Task CompletedWithdrawalDetailsMatchHistory()
    {
        var history = await History(0, 25);
        var selected = history.OfType<JObject>().FirstOrDefault(x => x.Value<int>("status") == 2);
        Assert.True(selected != null, "Prerequisite: a completed withdrawal in the first history page.");
        var id = ProviderReadContracts.RequiredString(selected!, "id");
        var detail = Assert.IsType<JObject>(await Read("getWithdrawalInformation", new JObject { ["id"] = id }));
        CoinPaymentsChecks.Withdrawal(detail);
        CoinPaymentsChecks.SameWithdrawal(selected!, detail);
        output.WriteLine("Completed withdrawal selected from history matches detail by coin, amounts, creation time, status, destination and txid. Sensitive comparison values omitted; detail does not require an id field.");
    }
}

internal static class CoinPaymentsChecks
{
    internal static decimal Number(JToken? value, bool text = false)
    {
        Assert.NotNull(value);
        Assert.True(text ? value!.Type == JTokenType.String : value!.Type is JTokenType.Integer or JTokenType.Float,
            "Unexpected numeric field type.");
        Assert.True(decimal.TryParse(value!.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n),
            "Invalid number or unsupported decimal range.");
        Assert.True(n >= 0, "Amount/rate/fee cannot be negative.");
        return n;
    }
    internal static JObject Balances(JToken result)
    {
        var values = Assert.IsType<JObject>(result); Assert.NotEmpty(values.Properties());
        foreach (var item in values.Properties())
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Name)); var row = Assert.IsType<JObject>(item.Value);
            Assert.Equal(JTokenType.Integer, row["balance"]?.Type); Number(row["balance"]); Number(row["balancef"]);
            ProviderReadContracts.RequiredString(row, "status"); ProviderReadContracts.RequiredString(row, "coin_status");
        }
        return values;
    }
    internal static JObject Rates(JToken result)
    {
        var values = Assert.IsType<JObject>(result); Assert.NotEmpty(values.Properties());
        foreach (var item in values.Properties())
        {
            var row = Assert.IsType<JObject>(item.Value);
            Assert.Equal(JTokenType.Integer, row["is_fiat"]?.Type);
            Assert.True(row.Value<int>("is_fiat") is 0 or 1, "is_fiat must be 0 or 1.");
            Assert.True(Number(row["rate_btc"], true) > 0, "Rate must be positive.");
            Number(row["tx_fee"], true);
            var time = ProviderReadContracts.RequiredString(row, "last_update");
            Assert.True(long.TryParse(time, out var stamp) && stamp >= 0, "last_update must be a nonnegative integer string.");
            // BTC uses 2147483647: do not impose freshness on this sentinel.
            ProviderReadContracts.RequiredString(row, "status");
        }
        return values;
    }
    internal static void Withdrawal(JObject row)
    {
        foreach (var field in new[] { "time_created", "status", "amount" }) Assert.Equal(JTokenType.Integer, row[field]?.Type);
        Assert.True(row.Value<long>("time_created") >= 0); Number(row["amount"]); Number(row["amountf"]);
        foreach (var field in new[] { "status_text", "coin", "send_address" }) ProviderReadContracts.RequiredString(row, field);
        if (row.Value<int>("status") == 2) ProviderReadContracts.RequiredString(row, "send_txid");
        if (row["send_txid"] is JToken tx && tx.Type != JTokenType.Null) Assert.Equal(JTokenType.String, tx.Type);
        // Do not infer a universal scale from ETH/USDC examples or a status enum from completed records.
    }
    internal static JArray History(JToken result, int limit)
    {
        var rows = Assert.IsType<JArray>(result); Assert.True(rows.Count <= limit, "History exceeded requested Limit.");
        var ids = new HashSet<string>(StringComparer.Ordinal); long previous = long.MaxValue;
        foreach (var token in rows)
        {
            var row = Assert.IsType<JObject>(token); Withdrawal(row);
            Assert.True(ids.Add(ProviderReadContracts.RequiredString(row, "id")), "Duplicate withdrawal ID.");
            var time = row.Value<long>("time_created"); Assert.True(time <= previous, "History is not newest-first."); previous = time;
        }
        return rows;
    }
    internal static IEnumerable<string> Ids(JArray rows) => rows.Select(x => x.Value<string>("id")!);
    internal static void Pages(JArray snapshot, JArray first, JArray second)
    {
        Assert.NotEmpty(first); Assert.NotEmpty(second);
        Assert.True(Ids(first).SequenceEqual(Ids(snapshot).Take(25)), "First page differs from snapshot.");
        Assert.True(Ids(second).SequenceEqual(Ids(snapshot).Skip(25).Take(25)), "Offset page differs from expected snapshot slice.");
        Assert.Empty(Ids(first).Intersect(Ids(second), StringComparer.Ordinal));
    }
    internal static void SameWithdrawal(JObject history, JObject detail)
    {
        foreach (var field in new[] { "time_created", "status", "status_text", "coin", "amount", "amountf", "send_address", "send_txid" })
            Assert.True(JToken.DeepEquals(history[field], detail[field]), "Withdrawal detail differs from history in " + field + "; values omitted.");
    }
}

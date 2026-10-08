using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class KoyweReadContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JToken> Read(string method, JObject? body = null)
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST",
            "/api/KoyweProvider/" + method, await TestReport.GetTokenAsync(api, output),
            (body ?? new JObject()).ToString(Formatting.None)));
        ProviderBatchChecks.SuccessFlags(data);
        return data["result"]!;
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task CurrencyDetailMatchesListedCurrency()
    {
        var catalog = Assert.IsType<JArray>(await Read("getAllCurrencyTokenPairs"));
        Assert.NotEmpty(catalog);
        foreach (var row in catalog) KoyweChecks.Currency(row);
        var selected = catalog[0];
        var detail = await Read("getCurrencyTokenPairs", new JObject { ["currency"] = selected.Value<string>("symbol") });
        KoyweChecks.Currency(detail);
        foreach (var field in new[] { "_id", "symbol", "decimals" })
            Assert.True(JToken.DeepEquals(selected[field], detail[field]), "Currency detail differs in " + field);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task TokenLookupReturnsRequestedSymbol()
    {
        var catalog = Assert.IsType<JArray>(await Read("getAllTokenCurrenciesPairs"));
        Assert.NotEmpty(catalog);
        foreach (var row in catalog) KoyweChecks.Asset(row);
        var selected = catalog[0];
        var symbol = ProviderReadContracts.RequiredString(selected, "symbol");
        var matches = Assert.IsType<JArray>(await Read("getTokenCurrenciesPairs", new JObject { ["coin"] = symbol }));
        Assert.NotEmpty(matches);
        foreach (var row in matches) { KoyweChecks.Asset(row); Assert.Equal(symbol, row.Value<string>("symbol")); }
        Assert.Contains(matches, x => JToken.DeepEquals(x["_id"], selected["_id"]));
        // Network variants may share symbols; do not invent symbol uniqueness.
    }

    [LiveTheory, InlineData("getPaymentMethods"), InlineData("getPayoutMethods"), Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task MethodsHaveIdsNamesAndNonnegativeFees(string method)
    {
        var rows = Assert.IsType<JArray>(await Read(method, new JObject { ["symbol"] = "CLP", ["clientId"] = null }));
        Assert.NotEmpty(rows); ProviderReadContracts.UniqueIds(rows, "_id");
        foreach (var row in rows)
        {
            ProviderReadContracts.RequiredString(row, "name"); KoyweChecks.Number(row["fee"]);
            foreach (var field in new[] { "description", "details", "image" }) Assert.Equal(JTokenType.String, row[field]?.Type);
        }
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task HistoryHasValidOrdersAndPagination()
    {
        var from = Environment.GetEnvironmentVariable("WEBAPI_KOYWE_FROM") ?? "2026-09-01T00:00:00Z";
        var to = Environment.GetEnvironmentVariable("WEBAPI_KOYWE_TO") ?? "2026-09-30T23:59:59Z";
        var result = Assert.IsType<JObject>(await Read("getHistory", new JObject
        { ["pageSize"] = 50, ["pageNumber"] = 1, ["initDate"] = from, ["endDate"] = to }));
        KoyweChecks.History(result);
        output.WriteLine("Validated {0} history records; empty periods are allowed. Status is not pinned to REJECTED.", ((JArray)result["data"]!).Count);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task BalanceReportAllowsEmptyCountryCurrencyRows()
    {
        var result = Assert.IsType<JObject>(await Read("getBalance", new JObject
        { ["startDate"] = "2026-09-01", ["endDate"] = "2026-09-30" }));
        Assert.IsType<JArray>(result["reportByCountryAndCurrency"]);
        KoyweChecks.Date(result["dateOfBalance"]);
        Assert.NotNull(result["warning"]);
        Assert.True(result["warning"]!.Type is JTokenType.Null or JTokenType.String);
        output.WriteLine("Validated report envelope; supplied sample had no balance rows, so row schema is not asserted.");
    }
}

internal static class KoyweChecks
{
    internal static void Number(JToken? value)
    {
        Assert.NotNull(value); Assert.True(value!.Type is JTokenType.Integer or JTokenType.Float);
        ProviderReadContracts.NonNegativeNumber(value);
    }
    internal static void Date(JToken? value)
    {
        Assert.NotNull(value); Assert.Equal(JTokenType.String, value!.Type);
        Assert.True(DateTimeOffset.TryParse(value.Value<string>(), CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _), "Invalid date string.");
    }
    internal static void Asset(JToken row)
    {
        Assert.IsType<JObject>(row);
        foreach (var field in new[] { "_id", "name", "symbol" }) ProviderReadContracts.RequiredString(row, field);
        Assert.Equal(JTokenType.Integer, row["decimals"]?.Type); Assert.True(row.Value<int>("decimals") >= 0);
    }
    internal static void Currency(JToken row)
    {
        Asset(row); var limits = Assert.IsType<JObject>(row["limits"]);
        Number(limits["min"]); Number(limits["max"]);
        Assert.True(ProviderReadContracts.NonNegativeNumber(limits["min"]) <= ProviderReadContracts.NonNegativeNumber(limits["max"]));
        var tokens = Assert.IsType<JArray>(row["tokens"]); Assert.NotEmpty(tokens);
        foreach (var token in tokens) Asset(token);
    }
    internal static void History(JObject result)
    {
        var rows = Assert.IsType<JArray>(result["data"]); Assert.True(rows.Count <= 50);
        ProviderReadContracts.UniqueIds(rows, "orderId");
        var pagination = Assert.IsType<JObject>(result["pagination"]);
        foreach (var field in new[] { "pageNumber", "pageSize", "totalcount" }) Assert.Equal(JTokenType.Integer, pagination[field]?.Type);
        Assert.Equal(1, pagination.Value<int>("pageNumber")); Assert.Equal(50, pagination.Value<int>("pageSize"));
        Assert.True(pagination.Value<int>("totalcount") >= rows.Count);
        foreach (var row in rows)
        {
            Assert.True(Guid.TryParse(ProviderReadContracts.RequiredString(row, "orderId"), out _));
            foreach (var field in new[] { "_id", "orderType", "status", "symbolIn", "symbolOut" }) ProviderReadContracts.RequiredString(row, field);
            foreach (var field in new[] { "amountIn", "amountOut", "exchangeRate", "koyweFee", "networkFee" }) Number(row[field]);
            var dates = Assert.IsType<JObject>(row["dates"]);
            Date(dates["confirmationDate"]);
            foreach (var field in new[] { "deliveryDate", "executionDate", "paymentDate" })
            { Assert.NotNull(dates[field]); if (dates[field]!.Type != JTokenType.Null) Date(dates[field]); }
        }
    }
}

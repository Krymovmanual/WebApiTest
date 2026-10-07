using System.Globalization;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads"), Trait("Suite", "Live"), Trait("Provider", "Fireblocks")]
public class FireblocksTransactionContracts(ApiHarness api, ITestOutputHelper output)
{
    [LiveFact]
    public async Task GetTransactionsHonorsLimitAndCreatedAtDescendingOrder()
    {
        var body = new JObject { ["limit"] = 2, ["orderBy"] = "createdAt", ["sort"] = "DESC" };
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/FireblocksProvider/getTransactions",
            await TestReport.GetTokenAsync(api, output), body.ToString()));
        var result = Assert.IsType<JObject>(data["result"]);
        FireblocksTransactionChecks.History(result, 2);
        output.WriteLine("Transactions: {0}", ((JArray)result["transactions"]!).Count);
        foreach (var tx in (JArray)result["transactions"]!)
            output.WriteLine("id={0}; assetId={1}; status={2}", tx["id"], tx["assetId"], tx["status"]);
        // Notes, addresses and provider pagination URLs are not logged or followed.
    }
}

internal static class FireblocksTransactionChecks
{
    private static decimal Number(JToken? value)
    {
        Assert.NotNull(value);
        Assert.True(value!.Type is JTokenType.Integer or JTokenType.Float or JTokenType.String, "Expected numeric amount.");
        var text = value.Type == JTokenType.String ? value.Value<string>() : value.ToString(Newtonsoft.Json.Formatting.None);
        Assert.True(decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number), "Invalid amount.");
        return number;
    }
    public static void History(JObject result, int limit)
    {
        var transactions = Assert.IsType<JArray>(result["transactions"]);
        Assert.InRange(transactions.Count, 0, limit); ProviderReadContracts.UniqueIds(transactions, "id");
        long? previous = null;
        foreach (var tx in transactions)
        {
            foreach (var field in new[] { "assetId", "status", "operation" }) ProviderReadContracts.RequiredString(tx, field);
            foreach (var field in new[] { "createdAt", "lastUpdated" })
            {
                Assert.Equal(JTokenType.Integer, tx[field]?.Type); Assert.True(tx.Value<long>(field) >= 0);
            }
            var created = tx.Value<long>("createdAt");
            if (previous != null) Assert.True(created <= previous, "Transactions are not ordered by createdAt DESC.");
            previous = created;
            foreach (var field in new[] { "source", "destination" })
                ProviderReadContracts.RequiredString(Assert.IsType<JObject>(tx[field]), "type");
            ProviderReadContracts.NonNegativeNumber(tx["amount"]); ProviderReadContracts.NonNegativeNumber(tx["requestedAmount"]);
            var completed = tx.Value<string>("status") == "COMPLETED";
            foreach (var field in new[] { "fee", "networkFee", "netAmount", "serviceFee" })
            {
                var number = Number(tx[field]);
                // Our wrapper uses exactly -1 for unavailable values on noncompleted transactions.
                Assert.True(number >= 0 || (!completed && number == -1), $"Unexpected {field} for transaction status.");
            }
            var amountInfo = Assert.IsType<JObject>(tx["amountInfo"]);
            foreach (var field in new[] { "amount", "requestedAmount", "netAmount", "amountUSD" })
                if (amountInfo[field] != null && amountInfo[field]!.Type != JTokenType.Null)
                    ProviderReadContracts.NonNegativeNumber(amountInfo[field]);
            var feeInfo = Assert.IsType<JObject>(tx["feeInfo"]);
            foreach (var field in new[] { "networkFee", "serviceFee" })
                if (feeInfo[field] != null && feeInfo[field]!.Type != JTokenType.Null)
                    ProviderReadContracts.NonNegativeNumber(feeInfo[field]);
            Assert.IsType<JObject>(tx["blockInfo"]); // Failed transactions may have null height/hash and empty txHash.
        }
        var pages = Assert.IsType<JObject>(result["pageDetails"]);
        foreach (var field in new[] { "prevPage", "nextPage" })
            if (pages[field] != null && pages[field]!.Type != JTokenType.Null) Assert.Equal(JTokenType.String, pages[field]!.Type);
    }
}

[Trait("Suite", "Offline"), Trait("Provider", "Fireblocks")]
public class FireblocksTransactionCheckTests(ITestOutputHelper output)
{
    private static JObject Failed() => JObject.Parse("""
    {"transactions":[{"id":"test-id","createdAt":2,"lastUpdated":3,"assetId":"ETH_TEST5","status":"FAILED","operation":"TRANSFER",
    "source":{"type":"VAULT_ACCOUNT"},"destination":{"type":"ONE_TIME_ADDRESS"},"requestedAmount":"0.000001","amount":0.000001,
    "fee":-1,"networkFee":-1,"netAmount":-1,"serviceFee":0,"amountInfo":{"amount":"0.000001","netAmount":null},
    "feeInfo":{"networkFee":null,"serviceFee":null},"blockInfo":{"blockHeight":null,"blockHash":null}}],
    "pageDetails":{"prevPage":"","nextPage":""}}
    """);
    [Fact]
    public void FailedTransactionAcceptsUnavailableFeeSentinels()
    {
        output.WriteLine("Offline fixture check: FailedTransactionAcceptsUnavailableFeeSentinels. No live API request.");
        FireblocksTransactionChecks.History(Failed(), 2);
    }
    [Fact]
    public void CompletedTransactionRejectsUnavailableFeeSentinels()
    {
        output.WriteLine("Offline fixture check: CompletedTransactionRejectsUnavailableFeeSentinels. No live API request.");
        var data = Failed(); data["transactions"]![0]!["status"] = "COMPLETED";
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksTransactionChecks.History(data, 2));
    }
    [Fact]
    public void InvalidNegativeFeeFails()
    {
        output.WriteLine("Offline fixture check: InvalidNegativeFeeFails. No live API request.");
        var data = Failed(); data["transactions"]![0]!["fee"] = -2;
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksTransactionChecks.History(data, 2));
    }
    [Fact]
    public void IgnoredDescendingSortFails()
    {
        output.WriteLine("Offline fixture check: IgnoredDescendingSortFails. No live API request.");
        var data = Failed(); var newer = data["transactions"]![0]!.DeepClone(); newer["id"] = "newer"; newer["createdAt"] = 4;
        ((JArray)data["transactions"]!).Add(newer);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksTransactionChecks.History(data, 2));
    }
}

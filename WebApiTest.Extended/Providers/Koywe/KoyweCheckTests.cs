using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public class KoyweCheckTests
{
    private static JObject History() => (JObject)ApiHarness.ParseWireJson("""
    {"data":[{"_id":"fixture","orderId":"710ca829-1139-4909-973e-b87460b11903",
    "orderType":"currency-crypto","status":"REJECTED","symbolIn":"COP","symbolOut":"ETH",
    "amountIn":63000,"amountOut":0.003469,"exchangeRate":16967999.9798,"koyweFee":3203,"networkFee":924,
    "dates":{"confirmationDate":"2026-09-21T09:51:57.149Z","deliveryDate":null,"executionDate":null,"paymentDate":null}}],
    "pagination":{"pageNumber":1,"pageSize":50,"totalcount":1}}
    """);

    [Fact, Trait("Suite", "Offline"), Trait("Provider", "Koywe")]
    public void RejectsNeitherRejectedStatusNorNullLifecycleDates() => KoyweChecks.History(History());

    [Fact, Trait("Suite", "Offline"), Trait("Provider", "Koywe")]
    public void EmptyHistoryIsValid()
    {
        var value = History(); ((JArray)value["data"]!).Clear(); value["pagination"]!["totalcount"] = 0;
        KoyweChecks.History(value);
    }

    [Theory, InlineData("amountIn"), InlineData("amountOut"), InlineData("networkFee"), Trait("Suite", "Offline"), Trait("Provider", "Koywe")]
    public void RejectsNegativeAmounts(string field)
    {
        var value = History(); value["data"]![0]![field] = -1;
        Assert.ThrowsAny<Exception>(() => KoyweChecks.History(value));
    }

    [Fact, Trait("Suite", "Offline"), Trait("Provider", "Koywe")]
    public void RejectsInvalidOrderGuid()
    {
        var value = History(); value["data"]![0]!["orderId"] = "65a1f3c2b4d5e6f708192a3b";
        Assert.ThrowsAny<Exception>(() => KoyweChecks.History(value));
    }

    [Fact, Trait("Suite", "Offline"), Trait("Provider", "Koywe")]
    public void RejectsDuplicateHistoryOrders()
    {
        var value = History(); var rows = (JArray)value["data"]!; rows.Add(rows[0].DeepClone());
        Assert.ThrowsAny<Exception>(() => KoyweChecks.History(value));
    }

    [Fact, Trait("Suite", "Offline"), Trait("Provider", "Koywe")]
    public void RejectsInvertedCurrencyLimits()
    {
        var row = JObject.Parse("""{"_id":"fixture","name":"Peso","symbol":"CLP","decimals":0,"limits":{"min":2,"max":1},"tokens":[{"_id":"token","name":"Ether","symbol":"ETH","decimals":18}]}""");
        Assert.ThrowsAny<Exception>(() => KoyweChecks.Currency(row));
    }
}

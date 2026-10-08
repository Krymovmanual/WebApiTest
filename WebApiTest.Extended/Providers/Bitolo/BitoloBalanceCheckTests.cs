using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public class BitoloBalanceCheckTests
{
    private static JObject Valid() => JObject.Parse("""
        {"error":"ok","result":{"account_id":174,"balance":678710364.115,"currency":"ARS"}}
        """);

    [Fact, Trait("Suite", "Offline")]
    public void SuppliedDevShapePassesWithoutPinningChangingAmountOrAccount()
    {
        var data = Valid(); ProviderBatchChecks.BitoloBalance(data, "ARS");
        data["result"]!["account_id"] = 999;
        data["result"]!["balance"] = 0;
        ProviderBatchChecks.BitoloBalance(data, "ARS");
        data["result"]!["balance"] = -1;
        ProviderBatchChecks.BitoloBalance(data, "ARS");
    }

    [Theory, Trait("Suite", "Offline")]
    [InlineData("account_id", "0")]
    [InlineData("account_id", "-1")]
    [InlineData("account_id", "\"174\"")]
    [InlineData("account_id", "174.5")]
    [InlineData("balance", "\"678710364.115\"")]
    [InlineData("balance", "true")]
    [InlineData("balance", "null")]
    [InlineData("currency", "\"USD\"")]
    [InlineData("currency", "null")]
    public void WrongTypesInvalidAccountAndMismatchedCurrencyFail(string field, string json)
    {
        var data = Valid(); data["result"]![field] = ApiHarness.ParseWireJson(json);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.BitoloBalance(data, "ARS"));
    }

    [Theory, Trait("Suite", "Offline")]
    [InlineData("account_id")]
    [InlineData("balance")]
    [InlineData("currency")]
    public void MissingRequiredFieldsFail(string field)
    {
        var data = Valid(); ((JObject)data["result"]!).Remove(field);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.BitoloBalance(data, "ARS"));
    }
}

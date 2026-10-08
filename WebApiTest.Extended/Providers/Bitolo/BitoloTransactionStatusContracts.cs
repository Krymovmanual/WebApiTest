using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

public sealed class BitoloStatusFactAttribute : FactAttribute
{
    public BitoloStatusFactAttribute()
    {
        if (!Settings.Live || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBAPI_BITOLO_CURRENCY")) ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBAPI_BITOLO_ORDER_ID")))
            Skip = "Set WEBAPI_RUN_LIVE=1, WEBAPI_BITOLO_CURRENCY and WEBAPI_BITOLO_ORDER_ID to a known DEV order.";
    }
}

[Collection("Stage reads")]
public class BitoloTransactionStatusContracts(ApiHarness api, ITestOutputHelper output)
{
    [BitoloStatusFact, Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ReadContract")]
    public async Task StatusReturnsRequestedOrderAndCurrencyWithValidTransactionFields()
    {
        var currency = Environment.GetEnvironmentVariable("WEBAPI_BITOLO_CURRENCY")!;
        var orderId = Environment.GetEnvironmentVariable("WEBAPI_BITOLO_ORDER_ID")!;
        var expectedStatus = Environment.GetEnvironmentVariable("WEBAPI_BITOLO_EXPECTED_STATUS");
        var body = new JObject { ["order_id"] = orderId };
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST",
            "/api/Bitolo/" + Uri.EscapeDataString(currency) + "/getTransactionStatus",
            await TestReport.GetTokenAsync(api, output), body.ToString(Formatting.None)));
        BitoloStatusChecks.Validate(data, currency, orderId, expectedStatus);
        output.WriteLine("Bitolo status: returned order_id and currency match the request; account/transaction identifiers, numeric amounts and event code/message structure verified.");
        output.WriteLine(string.IsNullOrWhiteSpace(expectedStatus)
            ? "Expected lifecycle status not configured: status presence checked, completion is not asserted."
            : "Configured expected lifecycle status verified.");
        output.WriteLine("Amounts are not pinned to historical values. This does not reconcile accounting or prove the full withdrawal lifecycle.");
    }
}

internal static class BitoloStatusChecks
{
    internal static void Validate(JObject data, string currency, string orderId, string? expectedStatus = null)
    {
        ProviderBatchChecks.SuccessFlags(data);
        var result = Assert.IsType<JObject>(data["result"]);
        Assert.True(result["account_id"]?.Type == JTokenType.Integer && result.Value<long>("account_id") > 0,
            "Bitolo account_id must be a positive integer.");
        ProviderReadContracts.RequiredString(result, "transaction_id");
        Assert.True(ProviderReadContracts.RequiredString(result, "order_id") == orderId, "Bitolo returned a different order_id; values omitted.");
        Assert.True(ProviderReadContracts.RequiredString(result, "currency") == currency, "Bitolo returned a different currency; values omitted.");
        foreach (var field in new[] { "amount", "amount_net" })
        {
            Assert.True(result[field]?.Type is JTokenType.Integer or JTokenType.Float, "Bitolo transaction amount must be a JSON number.");
            Assert.True(decimal.TryParse(result[field]!.ToString(Formatting.None), NumberStyles.Float, CultureInfo.InvariantCulture, out _),
                "Bitolo transaction amount is outside supported decimal precision/range.");
        }
        var status = ProviderReadContracts.RequiredString(result, "status");
        if (!string.IsNullOrWhiteSpace(expectedStatus)) Assert.True(status == expectedStatus, "Bitolo status differs from the configured expected status; values omitted.");
        ProviderReadContracts.RequiredString(result, "type");
        var providerEvent = Assert.IsType<JObject>(result["event"]);
        ProviderReadContracts.RequiredString(providerEvent, "code");
        ProviderReadContracts.RequiredString(providerEvent, "message");
        if (result["transaction_specific"] is JToken specific && specific.Type != JTokenType.Null)
            Assert.IsType<JObject>(specific);
        // No inferred status enum, gross/net fee equality, nonnegative amount rule or tax/name requirement.
    }
}

public class BitoloTransactionStatusCheckTests
{
    private static JObject Valid() => JObject.Parse("""
        {"error":"ok","result":{"account_id":174,"transaction_id":"fixture-tx","amount":97635,"amount_net":97635,"currency":"ARS","event":{"code":"BW1004","message":"Withdraw successful"},"order_id":"fixture-order","status":"SUCCESS","transaction_specific":{"e2e_id":null,"name":"NA","tax_id":"NA"},"type":"WITHDRAW"}}
        """);

    [Fact, Trait("Suite", "Offline")]
    public void SuppliedShapeAndNullableTransactionDetailsPass()
    {
        var data = Valid(); BitoloStatusChecks.Validate(data, "ARS", "fixture-order", "SUCCESS");
        data["result"]!["transaction_specific"] = JValue.CreateNull();
        data["result"]!["amount_net"] = 97634.9m;
        BitoloStatusChecks.Validate(data, "ARS", "fixture-order", "SUCCESS");
    }

    [Theory, Trait("Suite", "Offline")]
    [InlineData("order_id", "\"other-order\"")]
    [InlineData("currency", "\"USD\"")]
    [InlineData("account_id", "0")]
    [InlineData("transaction_id", "null")]
    [InlineData("amount", "\"97635\"")]
    [InlineData("amount_net", "null")]
    [InlineData("event", "[]")]
    [InlineData("type", "\"\"")]
    public void IncorrectIdentityMissingMetadataAndWrongTypesFail(string field, string json)
    {
        var data = Valid(); data["result"]![field] = ApiHarness.ParseWireJson(json);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => BitoloStatusChecks.Validate(data, "ARS", "fixture-order", "SUCCESS"));
    }

    [Fact, Trait("Suite", "Offline")]
    public void ExpectedStatusChecksCompletionOnlyWhenConfigured()
    {
        var data = Valid(); data["result"]!["status"] = "PENDING";
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => BitoloStatusChecks.Validate(data, "ARS", "fixture-order", "SUCCESS"));
        BitoloStatusChecks.Validate(data, "ARS", "fixture-order");
    }

    [Theory, Trait("Suite", "Offline")]
    [InlineData("code")]
    [InlineData("message")]
    public void MissingEventFieldsFail(string field)
    {
        var data = Valid(); ((JObject)data["result"]!["event"]!).Remove(field);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => BitoloStatusChecks.Validate(data, "ARS", "fixture-order"));
    }
}

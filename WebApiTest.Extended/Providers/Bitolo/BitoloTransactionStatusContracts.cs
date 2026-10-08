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

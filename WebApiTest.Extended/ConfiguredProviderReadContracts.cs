using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

public sealed class BitoloReadFactAttribute : FactAttribute
{
    public BitoloReadFactAttribute()
    {
        if (!Settings.Live || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBAPI_BITOLO_CURRENCY")))
            Skip = "Set WEBAPI_RUN_LIVE=1 and WEBAPI_BITOLO_CURRENCY to a configured provider currency.";
    }
}

public sealed class InfuraReadFactAttribute : FactAttribute
{
    public InfuraReadFactAttribute()
    {
        if (!Settings.Live || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBAPI_BLOCKCHAIN")))
            Skip = "Set WEBAPI_RUN_LIVE=1 and WEBAPI_BLOCKCHAIN to a supported DEV chain identifier.";
    }
}

// Swagger declares no request body for these read operations. Provider-specific result
// schemas are absent: validate only HTTP/application success and a structured result.
[Collection("Stage reads")]
public class ConfiguredProviderReadContracts(ApiHarness api, ITestOutputHelper output)
{
    [BitoloReadFact, Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ReadContract")]
    public Task BitoloBalanceReturnsSuccessfulStructuredResult() =>
        new ProviderBatchContracts(api, output).BitoloBalanceReturnsSuccessfulStructuredResult();

    [InfuraReadFact, Trait("Suite", "Live"), Trait("Provider", "Infura"), Trait("Coverage", "ReadEnvelope")]
    public Task InfuraAccountsReturnsSuccessfulStructuredResult() => Read(
        "/api/Infura/" + Uri.EscapeDataString(Environment.GetEnvironmentVariable("WEBAPI_BLOCKCHAIN")!) + "/getAccounts");

    private async Task Read(string path)
    {
        output.WriteLine("Read-envelope coverage only: provider-specific fields and balance correctness are not yet verified.");
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", path,
            await TestReport.GetTokenAsync(api, output)));
        Assert.True(data["result"] is JObject or JArray, "Expected a structured provider result; values omitted.");
        output.WriteLine("HTTP/application success and structured result verified. Empty collections are permitted.");
    }
}

public class ConfiguredReadCatalogContracts
{
    [Theory, Trait("Suite", "Offline")]
    [InlineData("/api/Bitolo/{currency}/getBalance")]
    [InlineData("/api/Infura/{blockchain}/getAccounts")]
    public void ReadRoutesExistWithoutBodyParameters(string path)
    {
        var operation = ApiSpecification.Snapshot["paths"]![path]!["post"]!;
        Assert.NotNull(operation);
        Assert.DoesNotContain(operation["parameters"] ?? new JArray(), p => p.Value<string>("in") == "body");
    }
}

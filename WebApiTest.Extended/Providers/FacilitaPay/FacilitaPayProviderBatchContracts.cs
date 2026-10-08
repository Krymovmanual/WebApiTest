using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class FacilitaPayProviderBatchContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JObject> Read(string route) => ApiHarness.SuccessfulJson(
        await TestReport.SendAsync(api, output, "POST", route, await TestReport.GetTokenAsync(api, output)));

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay")]
    public async Task FacilitaPayTokenReturnsRequiredIdentityAndTokenFields()
    {
        var data = await Read("/api/FacilitaPayProvider/getToken");
        ProviderBatchChecks.SuccessFlags(data);
        var result = Assert.IsType<JObject>(data["result"]);
        foreach (var field in new[] { "username", "name", "jwt" }) ProviderReadContracts.RequiredString(result, field);
        output.WriteLine("FacilitaPay: username/name/jwt present. Identity and token values omitted.");
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay")]
    public async Task FacilitaPayExchangeRatesContainPositiveNumericValues()
    {
        var data = await Read("/api/FacilitaPayProvider/getExchangeRates");
        ProviderBatchChecks.SuccessFlags(data);
        var rates = Assert.IsType<JObject>(data["result"]?["data"]);
        Assert.NotEmpty(rates.Properties());
        foreach (var rate in rates.Properties())
            Assert.True(ProviderReadContracts.NonNegativeNumber(rate.Value) > 0, "Exchange rate must be positive.");
        output.WriteLine("Verified {0} positive exchange rates. This does not verify rates against a market feed.", rates.Count);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay")]
    public async Task FacilitaPayBankAccountsHaveUniqueIdsAndValidOptionalMetadata()
    {
        var data = await Read("/api/FacilitaPayProvider/getBankAccounts");
        ProviderBatchChecks.BankAccounts(data);
        output.WriteLine("Bank account IDs are unique; optional bank/owner metadata has the expected types. Account numbers, IBANs and owner details omitted.");
    }
}

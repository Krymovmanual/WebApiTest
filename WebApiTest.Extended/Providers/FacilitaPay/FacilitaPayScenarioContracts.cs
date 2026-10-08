using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class FacilitaPayScenarioContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JObject> Read(string method, JObject? body = null) => ApiHarness.SuccessfulJson(
        await TestReport.SendAsync(api, output, "POST", "/api/FacilitaPayProvider/" + method,
            await TestReport.GetTokenAsync(api, output), (body ?? new JObject()).ToString(Formatting.None)));

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "CrossMethod")]
    public async Task ListedBankAccountCanBeReadByItsActualId()
    {
        var list = await Read("getBankAccounts");
        ProviderBatchChecks.BankAccounts(list);
        var accounts = Assert.IsType<JArray>(list["result"]?["data"]);
        var selected = Assert.IsType<JObject>(accounts[0]);
        var id = ProviderReadContracts.RequiredString(selected, "id");
        var detail = await Read("getBankAccount", new JObject { ["id"] = id });
        ProviderBatchChecks.SuccessFlags(detail);
        // Candidate response contract: list uses result.data; detail shape needs DEV confirmation.
        var account = Assert.IsType<JObject>(detail["result"]?["data"]);
        FacilitaPayScenarioChecks.SameAccount(selected, account);
        output.WriteLine("Actual account from list resolved by ID; identity and listed currency verified. No hardcoded account or balance. Detail result.data shape still requires first DEV confirmation.");
    }
}

internal static class FacilitaPayScenarioChecks
{
    internal static void SameAccount(JObject listed, JObject detail)
    {
        Assert.True(ProviderReadContracts.RequiredString(listed, "id") ==
            ProviderReadContracts.RequiredString(detail, "id"), "Detail account ID differs from the selected list account; values omitted.");
        if (listed["currency"]?.Type == JTokenType.String)
            Assert.True(ProviderReadContracts.RequiredString(listed, "currency") ==
                ProviderReadContracts.RequiredString(detail, "currency"), "Account currency differs between list and detail; values omitted.");
    }
}

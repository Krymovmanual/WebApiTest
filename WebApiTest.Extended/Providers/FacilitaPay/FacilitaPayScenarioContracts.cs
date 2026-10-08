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
        // Detail result.data confirmed by successful DEV sample.
        var account = Assert.IsType<JObject>(detail["result"]?["data"]);
        FacilitaPayScenarioChecks.SameAccount(selected, account);
        output.WriteLine("Actual account from list resolved by ID; identity and listed currency verified. No hardcoded account or balance. Detail result.data shape confirmed by supplied DEV capture.");
    }
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "CrossMethod")]
    public async Task ListedAccountBalanceHasMatchingCurrencyAndConsistentTotals()
    {
        var list = await Read("getBankAccounts");
        ProviderBatchChecks.BankAccounts(list);
        var selected = Assert.IsType<JObject>(Assert.IsType<JArray>(list["result"]?["data"])[0]);
        var id = ProviderReadContracts.RequiredString(selected, "id");
        var response = await Read("getBankAccountBalance", new JObject { ["id"] = id });
        ProviderBatchChecks.SuccessFlags(response);
        FacilitaPayReadChecks.Balance(Assert.IsType<JObject>(response["result"]?["data"]),
            ProviderReadContracts.RequiredString(selected, "currency"));
        output.WriteLine("Balance currency matches selected account; numeric totals/counts and credit minus debit equals balance verified. No fixed historical balance.");
    }

    private static JObject TransactionRequest() => new()
    {
        ["from"] = Environment.GetEnvironmentVariable("WEBAPI_FACILITA_FROM") ?? "2026-09-01T00:00:00Z",
        ["to"] = Environment.GetEnvironmentVariable("WEBAPI_FACILITA_TO") ?? "2026-09-03T23:59:59Z",
        ["complete"] = true, ["last_id"] = JValue.CreateNull(), ["per_page"] = "100"
    };

    private async Task<JArray> Transactions()
    {
        var request = TransactionRequest();
        var response = await Read("getTransactions", request);
        ProviderBatchChecks.SuccessFlags(response);
        return FacilitaPayReadChecks.Transactions(Assert.IsType<JArray>(response["result"]?["data"]),
            request.Value<string>("from")!, request.Value<string>("to")!);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay")]
    public async Task TransactionsHaveUniqueIdsAndRespectRequestedDateRange()
    {
        var rows = await Transactions();
        output.WriteLine("Validated {0} transactions, unique IDs, numeric amounts, flags and inserted_at in requested range. complete=true does not imply successful status.", rows.Count);
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "CrossMethod")]
    public async Task TransactionDetailMatchesSelectedListRecord()
    {
        var rows = await Transactions(); var selected = Assert.IsType<JObject>(rows[0]);
        var id = ProviderReadContracts.RequiredString(selected, "id");
        var response = await Read("getTransaction", new JObject { ["id"] = id });
        ProviderBatchChecks.SuccessFlags(response);
        var detail = Assert.IsType<JObject>(response["result"]?["data"]);
        FacilitaPayReadChecks.Transaction(detail);
        FacilitaPayReadChecks.SameTransaction(selected, detail);
        output.WriteLine("Transaction ID, value, currency, exchange values, inserted_at and bank account IDs agree. Enrichment fields subject/meta/owner_company are not compared; mutable status is validated but not pinned.");
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

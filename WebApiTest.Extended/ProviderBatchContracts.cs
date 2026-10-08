using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Text.RegularExpressions;

namespace WebApiTest.Extended;

// Known wrapper contracts from existing tests, with Nuvei's documented session response.
// These checks do not infer request bodies for undocumented read operations.
[Collection("Stage reads")]
public class ProviderBatchContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JObject> Read(string route) => ApiHarness.SuccessfulJson(
        await TestReport.SendAsync(api, output, "POST", route, await TestReport.GetTokenAsync(api, output)));

    [BitoloReadFact, Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ReadEnvelope")]
    public async Task BitoloBalanceReturnsSuccessfulStructuredResult()
    {
        var currency = Environment.GetEnvironmentVariable("WEBAPI_BITOLO_CURRENCY")!;
        var data = await Read("/api/Bitolo/" + Uri.EscapeDataString(currency) + "/getBalance");
        ProviderBatchChecks.SuccessFlags(data);
        Assert.True(data["result"] is JObject or JArray, "Expected a structured Bitolo balance result.");
        output.WriteLine("Bitolo: HTTP/application success and structured balance result verified. Currency identity and provider-specific balance fields require confirmed response data.");
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Nuvei_v2")]
    public async Task NuveiV2SessionReturnsSuccessfulProviderSession()
    {
        var data = await Read("/api/Nuvei_v2_Provider/getSessionToken");
        ProviderBatchChecks.NuveiSession(data);
        output.WriteLine("Nuvei_v2: sessionToken present; merchant identifiers present; status SUCCESS; errCode 0. Values of secrets/merchant identifiers omitted.");
    }

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "OpenPayd")]
    public async Task OpenPaydTokenReturnsUsableAuthenticationMetadata()
    {
        var data = await Read("/api/OpenPaydProvider/getToken");
        ProviderBatchChecks.OpenPaydToken(data);
        output.WriteLine("OpenPayd: access_token present. Optional token_type/expiry validated when returned. Token omitted; issuing a token does not prove account/history access.");
    }

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

internal static class ProviderBatchChecks
{
    internal static void SuccessFlags(JObject data)
    {
        foreach (var obj in new[] { data, data["result"] as JObject }.OfType<JObject>())
        {
            foreach (var field in new[] { "isSuccess", "IsSuccess", "success" })
                if (obj[field] is JToken value)
                {
                    Assert.Equal(JTokenType.Boolean, value.Type);
                    Assert.True(value.Value<bool>(), "Provider response explicitly reports failure; values omitted.");
                }
        }
    }

    internal static void NuveiSession(JObject data)
    {
        SuccessFlags(data);
        var result = Assert.IsType<JObject>(data["result"]);
        foreach (var field in new[] { "sessionToken", "merchantId", "merchantSiteId" })
            ProviderReadContracts.RequiredString(result, field);
        Assert.True(result.Value<string>("status") == "SUCCESS", "Nuvei session status was not SUCCESS.");
        Assert.True(result["errCode"]?.Type is JTokenType.Integer or JTokenType.String,
            "Nuvei session must have an integer/numeric-string errCode.");
        Assert.True(result["errCode"]!.ToString() == "0", "Nuvei session reports a nonzero error code.");
        if (result["version"] is JToken version) Assert.True(version.Type == JTokenType.String && version.Value<string>() == "1.0", "Unexpected Nuvei REST session version.");
    }

    internal static void OpenPaydToken(JObject data)
    {
        SuccessFlags(data);
        var result = Assert.IsType<JObject>(data["result"]);
        ProviderReadContracts.RequiredString(result, "access_token");
        if (result["token_type"] is JToken type)
            Assert.True(type.Type == JTokenType.String && string.Equals(type.Value<string>(), "Bearer", StringComparison.OrdinalIgnoreCase), "Unexpected token_type.");
        if (result["expires_in"] is JToken expiry)
            Assert.True(ProviderReadContracts.NonNegativeNumber(expiry) > 0, "Token expiry must be positive when returned.");
    }

    internal static void BankAccounts(JObject data)
    {
        SuccessFlags(data);
        var accounts = Assert.IsType<JArray>(data["result"]?["data"]);
        Assert.NotEmpty(accounts); // DEV prerequisite, matching the original repository's expectation.
        ProviderReadContracts.UniqueIds(accounts, "id");
        foreach (var account in accounts)
        {
            foreach (var field in new[] { "account_type", "account_number", "branch_number", "owner_document_number", "owner_document_type", "owner_name", "branch_country", "currency", "iban", "routing_number" })
                OptionalText(account, field);
            if (account["currency"] is JValue currency && currency.Type != JTokenType.Null)
                Assert.True(Regex.IsMatch(currency.Value<string>()!, "^[A-Z]{3}$"), "Account currency must be a three-letter uppercase code.");
            if (account["owner_company"] is JToken owner && owner.Type != JTokenType.Null)
            {
                Assert.IsType<JObject>(owner);
                foreach (var field in new[] { "social_name", "document_type", "document_number" }) OptionalText(owner, field);
            }
            if (account["bank"] is JToken bank && bank.Type != JTokenType.Null)
            {
                Assert.IsType<JObject>(bank);
                foreach (var field in new[] { "id", "code", "name", "swift" }) OptionalText(bank, field);
            }
        }
    }

    private static void OptionalText(JToken item, string field)
    {
        if (item[field] is JToken value && value.Type != JTokenType.Null)
            ProviderReadContracts.RequiredString(item, field);
    }
}

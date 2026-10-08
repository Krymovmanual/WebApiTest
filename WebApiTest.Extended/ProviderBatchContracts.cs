using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WebApiTest.Extended;


internal static class ProviderBatchChecks
{
    // Wrapper shape supplied from a successful DEV /api/Bitolo/ARS/getBalance response.
    internal static void BitoloBalance(JObject data, string expectedCurrency)
    {
        SuccessFlags(data);
        var result = Assert.IsType<JObject>(data["result"]);
        Assert.True(result["account_id"]?.Type == JTokenType.Integer, "Bitolo account_id must be an integer.");
        Assert.True(result.Value<long>("account_id") > 0, "Bitolo account_id must be positive.");
        Assert.True(result["balance"]?.Type is JTokenType.Integer or JTokenType.Float, "Bitolo balance must be a JSON number.");
        Assert.True(decimal.TryParse(result["balance"]!.ToString(Newtonsoft.Json.Formatting.None), NumberStyles.Float,
            CultureInfo.InvariantCulture, out _), "Bitolo balance is outside supported decimal precision/range.");
        var currency = ProviderReadContracts.RequiredString(result, "currency");
        Assert.True(currency == expectedCurrency, "Bitolo response currency differs from the requested currency; values omitted.");
        // Do not compare to the historical account 174 / amount supplied as an example.
        // Negative balances are not rejected without an agreed provider overdraft policy.
    }

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

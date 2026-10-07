using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

public class ProviderBatchCheckTests
{
    private static JObject Session() => JObject.Parse("""
        {"error":"ok","result":{"sessionToken":"fixture-secret","merchantId":"fixture-merchant","merchantSiteId":"fixture-site","status":"SUCCESS","errCode":0,"version":"1.0"}}
        """);

    [Theory, Trait("Suite", "Offline")]
    [InlineData("status", "ERROR")]
    [InlineData("errCode", "1")]
    [InlineData("sessionToken", "")]
    [InlineData("merchantId", "")]
    [InlineData("version", "2.0")]
    public void NuveiSessionRejectsUnusableAuthentication(string field, string value)
    {
        var data = Session(); data["result"]![field] = value;
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.NuveiSession(data));
    }

    [Fact, Trait("Suite", "Offline")]
    public void NuveiAcceptsNumericAndStringZeroWithoutPrintingTokens()
    {
        var data = Session(); ProviderBatchChecks.NuveiSession(data);
        data["result"]!["errCode"] = "0"; ProviderBatchChecks.NuveiSession(data);
    }

    [Theory, Trait("Suite", "Offline")]
    [InlineData("isSuccess")]
    [InlineData("IsSuccess")]
    [InlineData("success")]
    public void ExplicitFailureFlagsCannotPassWithHttp200(string field)
    {
        var data = Session(); data[field] = false;
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.SuccessFlags(data));
        data.Remove(field); data["result"]![field] = false;
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.SuccessFlags(data));
    }

    [Fact, Trait("Suite", "Offline")]
    public void OpenPaydRejectsExpiredOrMissingTokenMetadata()
    {
        var data = JObject.Parse("{\"result\":{\"access_token\":\"fixture-secret\",\"expires_in\":0}}");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.OpenPaydToken(data));
        data["result"]!["expires_in"] = 30; ProviderBatchChecks.OpenPaydToken(data);
        ((JObject)data["result"]!).Remove("access_token");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.OpenPaydToken(data));
    }

    [Fact, Trait("Suite", "Offline")]
    public void BankAccountsRejectDuplicatesButAllowNullableCountrySpecificFields()
    {
        var data = JObject.Parse("{\"result\":{\"data\":[{\"id\":\"a\",\"currency\":\"BRL\",\"iban\":null},{\"id\":\"b\",\"currency\":\"USD\",\"bank\":null}]}}");
        ProviderBatchChecks.BankAccounts(data);
        data["result"]!["data"]![1]!["id"] = "a";
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.BankAccounts(data));
    }

    [Fact, Trait("Suite", "Offline")]
    public void BankAccountsRejectWrongMetadataTypeAndEmptyFixtures()
    {
        var data = JObject.Parse("{\"result\":{\"data\":[{\"id\":\"a\",\"bank\":[]}]}}");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.BankAccounts(data));
        data["result"]!["data"] = new JArray();
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ProviderBatchChecks.BankAccounts(data));
    }

    [Fact, Trait("Suite", "Offline")]
    public void ReadCatalogIsRestrictedToSwaggerReads()
    {
        foreach (var entry in VerifiedReadProfile.Catalog)
        {
            var operation = ApiSpecification.Snapshot["paths"]![entry.Value.Route]!["post"]!;
            Assert.NotNull(operation);
            Assert.StartsWith("get", entry.Value.Route.Split('/').Last());
            Assert.Equal(entry.Value.Body, (operation["parameters"] ?? new JArray()).Any(p => p.Value<string>("in") == "body"));
        }
        var injected = JObject.Parse("{\"version\":1,\"cases\":{\"createWithdrawal\":{}}}");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadProfile.ValidateCatalog(injected));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadProfile.Route("OpenPaydAccount", JObject.Parse("{\"body\":{},\"path\":\"https://example.com\"}")));
    }

    [Fact, Trait("Suite", "Offline")]
    public void CurrencyCannotInjectAPathOrUseAnUnconfiguredDefault()
    {
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadProfile.Route("BitoloBalance", JObject.Parse("{\"currency\":\"ARS/withdraw\"}")));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadProfile.Route("BitoloBalance", new JObject()));
        Assert.EndsWith("/ARS/getBalance", VerifiedReadProfile.Route("BitoloBalance", JObject.Parse("{\"currency\":\"ARS\"}")));
    }

    [Theory, Trait("Suite", "Offline")]
    [InlineData("{\"select\":\"$.result.id\",\"op\":\"equals\",\"value\":\"known-id\"}", "{\"id\":\"wrong-id\"}")]
    [InlineData("{\"select\":\"$.result.currency\",\"op\":\"equals\",\"value\":\"ARS\"}", "{\"currency\":\"USD\"}")]
    [InlineData("{\"select\":\"$.result.amount\",\"op\":\"number\",\"min\":0}", "{\"amount\":-1}")]
    [InlineData("{\"select\":\"$.result.items\",\"op\":\"count\",\"min\":1}", "{\"items\":[]}")]
    [InlineData("{\"select\":\"$.result.items[*].id\",\"op\":\"unique\"}", "{\"items\":[{\"id\":\"a\"},{\"id\":\"a\"}]}")]
    [InlineData("{\"select\":\"$.result.id\",\"op\":\"nonEmpty\"}", "{\"id\":null}")]
    [InlineData("{\"select\":\"$.result.id\",\"op\":\"nonEmpty\"}", "{}")]
    [InlineData("{\"select\":\"$.result.items[*].id\",\"op\":\"unique\"}", "{\"items\":[]}")]
    public void WrongDataCannotPassConfiguredAssertions(string rule, string result)
    {
        var response = new JObject { ["result"] = JObject.Parse(result) };
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadChecks.Validate(response, new JArray(JObject.Parse(rule))));
    }

    [Fact, Trait("Suite", "Offline")]
    public void NumericStringsPreservePrecisionAndNegativeBalancesNeedExplicitPolicy()
    {
        var data = JObject.Parse("{\"result\":{\"balance\":\"-0.000000000000000001\",\"items\":[{\"id\":\"a\"},{\"id\":\"b\"}]}}");
        var checks = JArray.Parse("[{\"select\":\"$.result.balance\",\"op\":\"number\"},{\"select\":\"$.result.items[*].id\",\"op\":\"unique\"},{\"select\":\"$.result.items\",\"op\":\"count\",\"min\":2,\"max\":2}]");
        VerifiedReadChecks.Validate(data, checks);
        checks[0]!["min"] = 0;
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadChecks.Validate(data, checks));
    }

    [Fact, Trait("Suite", "Offline")]
    public void InvalidOrVacuousProfilesFailBeforeMakingRequests()
    {
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadProfile.Checks(JObject.Parse("{\"checks\":[{\"select\":\"$.result\",\"op\":\"type\",\"value\":\"object\"}]}")));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadChecks.ValidateRule(JObject.Parse("{\"select\":\"$.access_token\",\"op\":\"nonEmpty\"}")));
        Assert.Throws<InvalidOperationException>(() => VerifiedReadChecks.ValidateRule(JObject.Parse("{\"select\":\"$.result[\",\"op\":\"nonEmpty\"}")));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => VerifiedReadChecks.ValidateRule(JObject.Parse("{\"select\":\"$.result.amount\",\"op\":\"number\",\"min\":10,\"max\":1}")));
    }

    private sealed class Capture : ITestOutputHelper
    {
        internal readonly List<string> Lines = [];
        public void WriteLine(string message) => Lines.Add(message);
        public void WriteLine(string format, params object[] args) => Lines.Add(string.Format(format, args));
    }

    [Fact, Trait("Suite", "Offline")]
    public void ProviderBalanceOutputShowsAmountsWithoutOwnerOrTokenData()
    {
        var output = new Capture();
        TestReport.Describe(output, "result", JObject.Parse("""
            {"content":[{"id":"fixture-id","currency":"ARS","availableBalance":"12.0000001","owner_name":"private-owner","iban":"private-iban","access_token":"private-token"}]}
            """));
        var report = string.Join("\n", output.Lines);
        Assert.Contains("1 items", report); Assert.Contains("ARS", report); Assert.Contains("12.0000001", report);
        Assert.DoesNotContain("private-owner", report); Assert.DoesNotContain("private-iban", report); Assert.DoesNotContain("private-token", report);
    }
}

using Xunit.Abstractions;
using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

// Contract fields come from the existing repository tests; original tests are untouched.
[Collection("Stage reads")]
public class OpenPaydTokenContracts(ApiHarness api, ITestOutputHelper output)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "OpenPayd")]
    public async Task ReturnsAccessToken()
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/OpenPaydProvider/getToken", await TestReport.GetTokenAsync(api, output)));
        ProviderReadContracts.RequiredString(Assert.IsType<JObject>(data["result"]), "access_token");
    }
}
[Collection("Stage reads")]
public class NuveiTokenContracts(ApiHarness api, ITestOutputHelper output)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Nuvei")]
    public async Task ReturnsSuccessfulSession()
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/NuveiProvider/getSessionToken", await TestReport.GetTokenAsync(api, output)));
        var result = Assert.IsType<JObject>(data["result"]);
        foreach (var field in new[] { "sessionToken", "merchantId", "merchantSiteId" }) ProviderReadContracts.RequiredString(result, field);
        Assert.True(result.Value<string>("status") == "SUCCESS", "Provider session status was not SUCCESS.");
        Assert.True(result["errCode"]?.ToString() == "0", "Provider session has a nonzero error code.");
    }
}
[Collection("Stage reads")]
public class FacilitaPayTokenContracts(ApiHarness api, ITestOutputHelper output)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay")]
    public async Task ReturnsUserAndToken()
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/FacilitaPayProvider/getToken", await TestReport.GetTokenAsync(api, output)));
        var result = Assert.IsType<JObject>(data["result"]);
        foreach (var field in new[] { "username", "name", "jwt" }) ProviderReadContracts.RequiredString(result, field);
    }
}
[Collection("Stage reads")]
public class KoyweTokenContracts(ApiHarness api, ITestOutputHelper output)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task ReturnsToken()
    {
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "POST", "/api/KoyweProvider/getToken", await TestReport.GetTokenAsync(api, output)));
        ProviderReadContracts.RequiredString(Assert.IsType<JObject>(data["result"]), "token");
    }
}

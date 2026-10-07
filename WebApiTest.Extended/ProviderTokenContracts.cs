using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

// Contract fields come from the existing repository tests; original tests are untouched.
[Collection("Stage reads")]
public class OpenPaydTokenContracts(ApiHarness api)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "OpenPayd")]
    public async Task ReturnsAccessToken()
    {
        var data = ApiHarness.SuccessfulJson(await api.SendAsync("POST", "/api/OpenPaydProvider/getToken", await api.GetTokenAsync()));
        ProviderReadContracts.RequiredString(Assert.IsType<JObject>(data["result"]), "access_token");
    }
}
[Collection("Stage reads")]
public class NuveiTokenContracts(ApiHarness api)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Nuvei")]
    public async Task ReturnsSuccessfulSession()
    {
        var data = ApiHarness.SuccessfulJson(await api.SendAsync("POST", "/api/NuveiProvider/getSessionToken", await api.GetTokenAsync()));
        var result = Assert.IsType<JObject>(data["result"]);
        foreach (var field in new[] { "sessionToken", "merchantId", "merchantSiteId" }) ProviderReadContracts.RequiredString(result, field);
        Assert.True(result.Value<string>("status") == "SUCCESS", "Provider session status was not SUCCESS.");
        Assert.True(result["errCode"]?.ToString() == "0", "Provider session has a nonzero error code.");
    }
}
[Collection("Stage reads")]
public class FacilitaPayTokenContracts(ApiHarness api)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "FacilitaPay")]
    public async Task ReturnsUserAndToken()
    {
        var data = ApiHarness.SuccessfulJson(await api.SendAsync("POST", "/api/FacilitaPayProvider/getToken", await api.GetTokenAsync()));
        var result = Assert.IsType<JObject>(data["result"]);
        foreach (var field in new[] { "username", "name", "jwt" }) ProviderReadContracts.RequiredString(result, field);
    }
}
[Collection("Stage reads")]
public class KoyweTokenContracts(ApiHarness api)
{
    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Koywe")]
    public async Task ReturnsToken()
    {
        var data = ApiHarness.SuccessfulJson(await api.SendAsync("POST", "/api/KoyweProvider/getToken", await api.GetTokenAsync()));
        ProviderReadContracts.RequiredString(Assert.IsType<JObject>(data["result"]), "token");
    }
}

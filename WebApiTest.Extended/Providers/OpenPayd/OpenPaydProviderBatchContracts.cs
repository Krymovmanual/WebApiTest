using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class OpenPaydProviderBatchContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JObject> Read(string route) => ApiHarness.SuccessfulJson(
        await TestReport.SendAsync(api, output, "POST", route, await TestReport.GetTokenAsync(api, output)));

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "OpenPayd")]
    public async Task OpenPaydTokenReturnsUsableAuthenticationMetadata()
    {
        var data = await Read("/api/OpenPaydProvider/getToken");
        ProviderBatchChecks.OpenPaydToken(data);
        output.WriteLine("OpenPayd: access_token present. Optional token_type/expiry validated when returned. Token omitted; issuing a token does not prove account/history access.");
    }

}

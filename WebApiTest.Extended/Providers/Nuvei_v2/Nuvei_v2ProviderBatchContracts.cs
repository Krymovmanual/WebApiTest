using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class Nuvei_v2ProviderBatchContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JObject> Read(string route) => ApiHarness.SuccessfulJson(
        await TestReport.SendAsync(api, output, "POST", route, await TestReport.GetTokenAsync(api, output)));

    [LiveFact, Trait("Suite", "Live"), Trait("Provider", "Nuvei_v2")]
    public async Task NuveiV2SessionReturnsSuccessfulProviderSession()
    {
        var data = await Read("/api/Nuvei_v2_Provider/getSessionToken");
        ProviderBatchChecks.NuveiSession(data);
        output.WriteLine("Nuvei_v2: sessionToken present; merchant identifiers present; status SUCCESS; errCode 0. Values of secrets/merchant identifiers omitted.");
    }

}

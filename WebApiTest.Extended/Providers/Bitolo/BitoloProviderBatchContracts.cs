using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class BitoloProviderBatchContracts(ApiHarness api, ITestOutputHelper output)
{
    private async Task<JObject> Read(string route) => ApiHarness.SuccessfulJson(
        await TestReport.SendAsync(api, output, "POST", route, await TestReport.GetTokenAsync(api, output)));

    [BitoloReadFact, Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ReadContract")]
    public async Task BitoloBalanceReturnsSuccessfulStructuredResult()
    {
        var currency = Environment.GetEnvironmentVariable("WEBAPI_BITOLO_CURRENCY")!;
        var data = await Read("/api/Bitolo/" + Uri.EscapeDataString(currency) + "/getBalance");
        ProviderBatchChecks.BitoloBalance(data, currency);
        output.WriteLine("Bitolo: positive integer account_id, numeric balance and currency matching the requested currency verified. Current balance is displayed, not pinned to a historical amount.");
    }

}

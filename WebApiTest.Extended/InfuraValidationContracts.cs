using Xunit.Abstractions;
using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public sealed class InfuraTheoryAttribute : TheoryAttribute
{
    public InfuraTheoryAttribute()
    {
        if (!Settings.Live || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBAPI_BLOCKCHAIN")))
            Skip = "Set WEBAPI_RUN_LIVE=1 and WEBAPI_BLOCKCHAIN to a supported DEV chain identifier.";
    }
}

[Collection("Stage reads")]
public class InfuraValidationContracts(ApiHarness api, ITestOutputHelper output)
{
    [InfuraTheory, Trait("Suite", "Live"), Trait("Provider", "Infura")]
    [InlineData("/api/Infura/{blockchain}/getBalance")]
    [InlineData("/api/Infura/{blockchain}/getTransactionByHash")]
    [InlineData("/api/Infura/{blockchain}/getTransactionCount")]
    public async Task RejectsMissingRequiredRequestFields(string route)
    {
        var chain = Environment.GetEnvironmentVariable("WEBAPI_BLOCKCHAIN")!;
        var path = route.Replace("{blockchain}", Uri.EscapeDataString(chain));
        var response = await TestReport.SendAsync(api, output, "POST", path, await TestReport.GetTokenAsync(api, output), "{}", expectedStatuses: [400]);
        Assert.Equal(400, response.Status);
        var body = JObject.Parse(response.Body);
        var schema = ApiSpecification.Snapshot["paths"]![route]!["post"]!["responses"]!["400"]!["schema"]!;
        ResponseSchema.Validate(body, schema);
        Assert.True(body["status"]?.Value<int>() == 400, "Expected validation ProblemDetails status 400.");
    }
}

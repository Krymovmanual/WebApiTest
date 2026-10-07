using Xunit.Abstractions;
using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class ZeroHashReadContracts(ApiHarness api, ITestOutputHelper output)
{
    [LiveTheory, Trait("Suite", "Live"), Trait("Provider", "ZeroHash")]
    [InlineData("/api/ZeroHashProvider/assets")]
    [InlineData("/api/ZeroHashProvider/accounts")]
    [InlineData("/api/ZeroHashProvider/transfers")]
    [InlineData("/api/ZeroHashProvider/movements")]
    [InlineData("/api/ZeroHashProvider/deposits")]
    [InlineData("/api/ZeroHashProvider/deposits/digital_asset_addresses")]
    [InlineData("/api/ZeroHashProvider/withdrawals/requests")]
    [InlineData("/api/ZeroHashProvider/withdrawals/digital_asset_addresses")]
    public async Task ListResponseMatchesDocumentedSchema(string path)
    {
        // Lack of Swagger security metadata does not establish anonymous access. Use the configured API token.
        var data = ApiHarness.SuccessfulJson(await TestReport.SendAsync(api, output, "GET", path, await TestReport.GetTokenAsync(api, output)));
        var schema = ApiSpecification.Snapshot["paths"]![path]!["get"]!["responses"]!["200"]!["schema"]!;
        ResponseSchema.Validate(data, schema);
        if (data["result"] is JObject result && result["message"] is JToken message)
            Assert.IsType<JArray>(message);
    }
}

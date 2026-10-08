using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Public"), Trait("Suite", "Live")]
public sealed class PublicMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("GET /api/DeployDate")]
    public Task DeployDate() => CallAsync("GET /api/DeployDate");

    [ApiMethodFact("GET /api/Version")]
    public Task Version() => CallAsync("GET /api/Version");

}

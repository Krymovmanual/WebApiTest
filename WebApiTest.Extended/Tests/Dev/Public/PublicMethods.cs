using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Public")]
public sealed class PublicMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("GET /api/DeployDate", "DEV")]
    public Task DeployDate() => CallAsync("GET /api/DeployDate");

    [ApiMethodFact("GET /api/Version", "DEV")]
    public Task Version() => CallAsync("GET /api/Version");

}

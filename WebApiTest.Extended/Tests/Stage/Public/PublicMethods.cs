using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "Public")]
public sealed class PublicMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("GET /api/DeployDate", "STAGE")]
    public Task DeployDate() => CallAsync("GET /api/DeployDate");

    [ApiMethodFact("GET /api/Version", "STAGE")]
    public Task Version() => CallAsync("GET /api/Version");

}

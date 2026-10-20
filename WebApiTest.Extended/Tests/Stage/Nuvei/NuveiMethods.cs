using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "Nuvei")]
public sealed class NuveiMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/NuveiProvider/getSessionToken", "STAGE")]
    public Task getSessionToken() => CallAsync("POST /api/NuveiProvider/getSessionToken");

    [ApiMethodFact("POST /api/NuveiProvider/getRequests", "STAGE")]
    public Task getRequests() => CallAsync("POST /api/NuveiProvider/getRequests");

}

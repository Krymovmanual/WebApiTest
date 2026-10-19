using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Nuvei")]
public sealed class NuveiMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/NuveiProvider/getSessionToken", "DEV")]
    public Task getSessionToken() => CallAsync("POST /api/NuveiProvider/getSessionToken");

    [ApiMethodFact("POST /api/NuveiProvider/getRequests", "DEV")]
    public Task getRequests() => CallAsync("POST /api/NuveiProvider/getRequests");

}

using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Nuvei"), Trait("Suite", "Live")]
public sealed class NuveiMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/NuveiProvider/getSessionToken")]
    public Task getSessionToken() => CallAsync("POST /api/NuveiProvider/getSessionToken");

    [ApiMethodFact("POST /api/NuveiProvider/getPayoutStatus")]
    public Task getPayoutStatus() => CallAsync("POST /api/NuveiProvider/getPayoutStatus");

    [ApiMethodFact("POST /api/NuveiProvider/getRequests")]
    public Task getRequests() => CallAsync("POST /api/NuveiProvider/getRequests");

}

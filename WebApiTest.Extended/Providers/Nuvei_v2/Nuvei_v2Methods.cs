using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Nuvei_v2")]
public sealed class Nuvei_v2Methods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getSessionToken")]
    public Task getSessionToken() => CallAsync("POST /api/Nuvei_v2_Provider/getSessionToken");

    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getPayoutStatus")]
    public Task getPayoutStatus() => CallAsync("POST /api/Nuvei_v2_Provider/getPayoutStatus");

    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getRequests")]
    public Task getRequests() => CallAsync("POST /api/Nuvei_v2_Provider/getRequests");

}

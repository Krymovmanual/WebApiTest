using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "Nuvei_v2")]
public sealed class Nuvei_v2Methods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getSessionToken", "STAGE")]
    public Task getSessionToken() => CallAsync("POST /api/Nuvei_v2_Provider/getSessionToken");

    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getPayoutStatus", "STAGE")]
    public Task getPayoutStatus() => CallAsync("POST /api/Nuvei_v2_Provider/getPayoutStatus");

    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getRequests", "STAGE")]
    public Task getRequests() => CallAsync("POST /api/Nuvei_v2_Provider/getRequests");

}

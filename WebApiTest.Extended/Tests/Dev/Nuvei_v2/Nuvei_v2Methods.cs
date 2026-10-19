using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Nuvei_v2")]
public sealed class Nuvei_v2Methods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getSessionToken", "DEV")]
    public Task getSessionToken() => CallAsync("POST /api/Nuvei_v2_Provider/getSessionToken");

    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getPayoutStatus", "DEV")]
    public Task getPayoutStatus() => CallAsync("POST /api/Nuvei_v2_Provider/getPayoutStatus");

    [ApiMethodFact("POST /api/Nuvei_v2_Provider/getRequests", "DEV")]
    public Task getRequests() => CallAsync("POST /api/Nuvei_v2_Provider/getRequests");

}

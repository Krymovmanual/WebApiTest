using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class Nuvei_v2VerifiedProviderReadContracts(ApiHarness api, ITestOutputHelper output) : VerifiedProviderReadBase(api, output)
{
    [VerifiedReadFact("NuveiV2PayoutStatus"), Trait("Suite", "Live"), Trait("Provider", "Nuvei_v2"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task NuveiV2PayoutStatusMatchesVerifiedReadAssertions() => Read("NuveiV2PayoutStatus");

    [VerifiedReadFact("NuveiV2Requests"), Trait("Suite", "Live"), Trait("Provider", "Nuvei_v2"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task NuveiV2RequestsMatchesVerifiedReadAssertions() => Read("NuveiV2Requests");


}

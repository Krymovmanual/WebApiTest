using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "QuantfuryPaymentsSign"), Trait("Suite", "Live")]
public sealed class QuantfuryPaymentsSignMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/QuantfuryPaymentsSign/getCertificateData")]
    public Task getCertificateData() => CallAsync("POST /api/QuantfuryPaymentsSign/getCertificateData");

}

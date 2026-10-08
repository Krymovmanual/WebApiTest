using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "QuantfuryPayments"), Trait("Suite", "Live")]
public sealed class QuantfuryPaymentsMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/QuantfuryPaymentsProvider/getKyc")]
    public Task getKyc() => CallAsync("POST /api/QuantfuryPaymentsProvider/getKyc");

    [ApiMethodFact("POST /api/QuantfuryPaymentsProvider/getCurrencyFee")]
    public Task getCurrencyFee() => CallAsync("POST /api/QuantfuryPaymentsProvider/getCurrencyFee");

}

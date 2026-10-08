using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "CoinPayments")]
public sealed class CoinPaymentsMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/CoinPaymentsProvider/getBalances")]
    public Task getBalances() => CallAsync("POST /api/CoinPaymentsProvider/getBalances");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getExchangeRates")]
    public Task getExchangeRates() => CallAsync("POST /api/CoinPaymentsProvider/getExchangeRates");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getWithdrawalInformation")]
    public Task getWithdrawalInformation() => CallAsync("POST /api/CoinPaymentsProvider/getWithdrawalInformation");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getWithdrawalHistory")]
    public Task getWithdrawalHistory() => CallAsync("POST /api/CoinPaymentsProvider/getWithdrawalHistory");

}

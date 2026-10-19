using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "CoinPayments")]
public sealed class CoinPaymentsMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/CoinPaymentsProvider/getBalances", "DEV")]
    public Task getBalances() => CallAsync("POST /api/CoinPaymentsProvider/getBalances");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getExchangeRates", "DEV")]
    public Task getExchangeRates() => CallAsync("POST /api/CoinPaymentsProvider/getExchangeRates");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getWithdrawalInformation", "DEV")]
    public Task getWithdrawalInformation() => CallAsync("POST /api/CoinPaymentsProvider/getWithdrawalInformation");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getWithdrawalHistory", "DEV")]
    public Task getWithdrawalHistory() => CallAsync("POST /api/CoinPaymentsProvider/getWithdrawalHistory");

}

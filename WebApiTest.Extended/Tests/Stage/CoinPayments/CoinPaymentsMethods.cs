using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "CoinPayments")]
public sealed class CoinPaymentsMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/CoinPaymentsProvider/getBalances", "STAGE")]
    public Task getBalances() => CallAsync("POST /api/CoinPaymentsProvider/getBalances");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getExchangeRates", "STAGE")]
    public Task getExchangeRates() => CallAsync("POST /api/CoinPaymentsProvider/getExchangeRates");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getWithdrawalInformation", "STAGE")]
    public Task getWithdrawalInformation() => CallAsync("POST /api/CoinPaymentsProvider/getWithdrawalInformation");

    [ApiMethodFact("POST /api/CoinPaymentsProvider/getWithdrawalHistory", "STAGE")]
    public Task getWithdrawalHistory() => CallAsync("POST /api/CoinPaymentsProvider/getWithdrawalHistory");
}

using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Koywe")]
public sealed class KoyweMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/KoyweProvider/getToken", "DEV")]
    public Task getToken() => CallAsync("POST /api/KoyweProvider/getToken");

    [ApiMethodFact("POST /api/KoyweProvider/getAccounyInfo", "DEV")]
    public Task getAccounyInfo() => CallAsync("POST /api/KoyweProvider/getAccounyInfo");

    [ApiMethodFact("POST /api/KoyweProvider/getAllCurrencyTokenPairs", "DEV")]
    public Task getAllCurrencyTokenPairs() => CallAsync("POST /api/KoyweProvider/getAllCurrencyTokenPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getCurrencyTokenPairs", "DEV")]
    public Task getCurrencyTokenPairs() => CallAsync("POST /api/KoyweProvider/getCurrencyTokenPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getAllTokenCurrenciesPairs", "DEV")]
    public Task getAllTokenCurrenciesPairs() => CallAsync("POST /api/KoyweProvider/getAllTokenCurrenciesPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getTokenCurrenciesPairs", "DEV")]
    public Task getTokenCurrenciesPairs() => CallAsync("POST /api/KoyweProvider/getTokenCurrenciesPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getPaymentMethods", "DEV")]
    public Task getPaymentMethods() => CallAsync("POST /api/KoyweProvider/getPaymentMethods");

    [ApiMethodFact("POST /api/KoyweProvider/getPayoutMethods", "DEV")]
    public Task getPayoutMethods() => CallAsync("POST /api/KoyweProvider/getPayoutMethods");

    [ApiMethodFact("POST /api/KoyweProvider/getHistory", "DEV")]
    public Task getHistory() => CallAsync("POST /api/KoyweProvider/getHistory");

    [ApiMethodFact("POST /api/KoyweProvider/getOrderInfo", "DEV")]
    public Task getOrderInfo() => CallAsync("POST /api/KoyweProvider/getOrderInfo");

    [ApiMethodFact("POST /api/KoyweProvider/getOrderInfoByExternalId", "DEV")]
    public Task getOrderInfoByExternalId() => CallAsync("POST /api/KoyweProvider/getOrderInfoByExternalId");

    [ApiMethodFact("POST /api/KoyweProvider/getBalance", "DEV")]
    public Task getBalance() => CallAsync("POST /api/KoyweProvider/getBalance");

    [ApiMethodFact("GET /api/KoyweProvider/quotes/{quoteId}", "DEV")]
    public Task quotes_quoteId() => CallAsync("GET /api/KoyweProvider/quotes/{quoteId}");

}

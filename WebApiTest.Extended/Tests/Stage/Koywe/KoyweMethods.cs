using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "Koywe")]
public sealed class KoyweMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/KoyweProvider/getToken", "STAGE")]
    public Task getToken() => CallAsync("POST /api/KoyweProvider/getToken");

    [ApiMethodFact("POST /api/KoyweProvider/getAllCurrencyTokenPairs", "STAGE")]
    public Task getAllCurrencyTokenPairs() => CallAsync("POST /api/KoyweProvider/getAllCurrencyTokenPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getAllTokenCurrenciesPairs", "STAGE")]
    public Task getAllTokenCurrenciesPairs() => CallAsync("POST /api/KoyweProvider/getAllTokenCurrenciesPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getAccounyInfo", "STAGE")]
    public Task getAccounyInfo() => CallAsync("POST /api/KoyweProvider/getAccounyInfo");

    [ApiMethodFact("POST /api/KoyweProvider/getCurrencyTokenPairs", "STAGE")]
    public Task getCurrencyTokenPairs() => CallAsync("POST /api/KoyweProvider/getCurrencyTokenPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getTokenCurrenciesPairs", "STAGE")]
    public Task getTokenCurrenciesPairs() => CallAsync("POST /api/KoyweProvider/getTokenCurrenciesPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getPaymentMethods", "STAGE")]
    public Task getPaymentMethods() => CallAsync("POST /api/KoyweProvider/getPaymentMethods");

    [ApiMethodFact("POST /api/KoyweProvider/getPayoutMethods", "STAGE")]
    public Task getPayoutMethods() => CallAsync("POST /api/KoyweProvider/getPayoutMethods");

    [ApiMethodFact("POST /api/KoyweProvider/getHistory", "STAGE")]
    public Task getHistory() => CallAsync("POST /api/KoyweProvider/getHistory");

    [ApiMethodFact("POST /api/KoyweProvider/getOrderInfo", "STAGE")]
    public Task getOrderInfo() => CallAsync("POST /api/KoyweProvider/getOrderInfo");

    [ApiMethodFact("POST /api/KoyweProvider/getOrderInfoByExternalId", "STAGE")]
    public Task getOrderInfoByExternalId() => CallAsync("POST /api/KoyweProvider/getOrderInfoByExternalId");

    [ApiMethodFact("POST /api/KoyweProvider/getBalance", "STAGE")]
    public Task getBalance() => CallAsync("POST /api/KoyweProvider/getBalance");

    [ApiMethodFact("GET /api/KoyweProvider/quotes/{quoteId}", "STAGE")]
    public Task quotes_quoteId() => CallAsync("GET /api/KoyweProvider/quotes/{quoteId}");
}

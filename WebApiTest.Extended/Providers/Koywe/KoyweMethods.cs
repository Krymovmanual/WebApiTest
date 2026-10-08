using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Koywe")]
public sealed class KoyweMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/KoyweProvider/getToken")]
    public Task getToken() => CallAsync("POST /api/KoyweProvider/getToken");

    [ApiMethodFact("POST /api/KoyweProvider/getAccounyInfo")]
    public Task getAccounyInfo() => CallAsync("POST /api/KoyweProvider/getAccounyInfo");

    [ApiMethodFact("POST /api/KoyweProvider/getAllCurrencyTokenPairs")]
    public Task getAllCurrencyTokenPairs() => CallAsync("POST /api/KoyweProvider/getAllCurrencyTokenPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getCurrencyTokenPairs")]
    public Task getCurrencyTokenPairs() => CallAsync("POST /api/KoyweProvider/getCurrencyTokenPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getAllTokenCurrenciesPairs")]
    public Task getAllTokenCurrenciesPairs() => CallAsync("POST /api/KoyweProvider/getAllTokenCurrenciesPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getTokenCurrenciesPairs")]
    public Task getTokenCurrenciesPairs() => CallAsync("POST /api/KoyweProvider/getTokenCurrenciesPairs");

    [ApiMethodFact("POST /api/KoyweProvider/getPaymentMethods")]
    public Task getPaymentMethods() => CallAsync("POST /api/KoyweProvider/getPaymentMethods");

    [ApiMethodFact("POST /api/KoyweProvider/getPayoutMethods")]
    public Task getPayoutMethods() => CallAsync("POST /api/KoyweProvider/getPayoutMethods");

    [ApiMethodFact("POST /api/KoyweProvider/getHistory")]
    public Task getHistory() => CallAsync("POST /api/KoyweProvider/getHistory");

    [ApiMethodFact("POST /api/KoyweProvider/getOrderInfo")]
    public Task getOrderInfo() => CallAsync("POST /api/KoyweProvider/getOrderInfo");

    [ApiMethodFact("POST /api/KoyweProvider/getOrderInfoByExternalId")]
    public Task getOrderInfoByExternalId() => CallAsync("POST /api/KoyweProvider/getOrderInfoByExternalId");

    [ApiMethodFact("POST /api/KoyweProvider/getBalance")]
    public Task getBalance() => CallAsync("POST /api/KoyweProvider/getBalance");

    [ApiMethodFact("GET /api/KoyweProvider/quotes/{quoteId}")]
    public Task quotes_quoteId() => CallAsync("GET /api/KoyweProvider/quotes/{quoteId}");

}

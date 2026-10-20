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
}

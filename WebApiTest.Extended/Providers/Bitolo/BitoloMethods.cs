using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Bitolo"), Trait("Suite", "Live")]
public sealed class BitoloMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/Bitolo/{currency}/getBalance")]
    public Task getBalance() => CallAsync("POST /api/Bitolo/{currency}/getBalance");

    [ApiMethodFact("POST /api/Bitolo/{currency}/getTransactionStatus")]
    public Task getTransactionStatus() => CallAsync("POST /api/Bitolo/{currency}/getTransactionStatus");

    [ApiMethodFact("POST /api/Bitolo/{currency}/getTransactionList")]
    public Task getTransactionList() => CallAsync("POST /api/Bitolo/{currency}/getTransactionList");

}

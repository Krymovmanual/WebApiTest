using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "Bitolo")]
public sealed class BitoloMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/Bitolo/{currency}/getBalance", "STAGE")]
    public Task getBalance() => CallAsync("POST /api/Bitolo/{currency}/getBalance");

    [ApiMethodFact("POST /api/Bitolo/{currency}/getTransactionStatus", "STAGE")]
    public Task getTransactionStatus() => CallAsync("POST /api/Bitolo/{currency}/getTransactionStatus");

    [ApiMethodFact("POST /api/Bitolo/{currency}/getTransactionList", "STAGE")]
    public Task getTransactionList() => CallAsync("POST /api/Bitolo/{currency}/getTransactionList");
}

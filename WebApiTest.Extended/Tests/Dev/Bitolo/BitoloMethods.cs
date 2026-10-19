using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Bitolo")]
public sealed class BitoloMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/Bitolo/{currency}/getBalance", "DEV")]
    public Task getBalance() => CallAsync("POST /api/Bitolo/{currency}/getBalance");

    [ApiMethodFact("POST /api/Bitolo/{currency}/getTransactionStatus", "DEV")]
    public Task getTransactionStatus() => CallAsync("POST /api/Bitolo/{currency}/getTransactionStatus");

    [ApiMethodFact("POST /api/Bitolo/{currency}/getTransactionList", "DEV")]
    public Task getTransactionList() => CallAsync("POST /api/Bitolo/{currency}/getTransactionList");

}

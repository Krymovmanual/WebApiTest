using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "OpenPayd"), Trait("Suite", "Live")]
public sealed class OpenPaydMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/OpenPaydProvider/getToken")]
    public Task getToken() => CallAsync("POST /api/OpenPaydProvider/getToken");

    [ApiMethodFact("POST /api/OpenPaydProvider/getHistory")]
    public Task getHistory() => CallAsync("POST /api/OpenPaydProvider/getHistory");

    [ApiMethodFact("POST /api/OpenPaydProvider/getTransactionInfo")]
    public Task getTransactionInfo() => CallAsync("POST /api/OpenPaydProvider/getTransactionInfo");

    [ApiMethodFact("POST /api/OpenPaydProvider/getAccount")]
    public Task getAccount() => CallAsync("POST /api/OpenPaydProvider/getAccount");

    [ApiMethodFact("POST /api/OpenPaydProvider/getAccounts")]
    public Task getAccounts() => CallAsync("POST /api/OpenPaydProvider/getAccounts");

}

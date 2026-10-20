using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "OpenPayd")]
public sealed class OpenPaydMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/OpenPaydProvider/getToken", "STAGE")]
    public Task getToken() => CallAsync("POST /api/OpenPaydProvider/getToken");

    [ApiMethodFact("POST /api/OpenPaydProvider/getTransactionInfo", "STAGE")]
    public Task getTransactionInfo() => CallAsync("POST /api/OpenPaydProvider/getTransactionInfo");

    [ApiMethodFact("POST /api/OpenPaydProvider/getAccount", "STAGE")]
    public Task getAccount() => CallAsync("POST /api/OpenPaydProvider/getAccount");

    [ApiMethodFact("POST /api/OpenPaydProvider/getAccounts", "STAGE")]
    public Task getAccounts() => CallAsync("POST /api/OpenPaydProvider/getAccounts");

}

using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "OpenPayd")]
public sealed class OpenPaydMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/OpenPaydProvider/getToken", "DEV")]
    public Task getToken() => CallAsync("POST /api/OpenPaydProvider/getToken");

    [ApiMethodFact("POST /api/OpenPaydProvider/getTransactionInfo", "DEV")]
    public Task getTransactionInfo() => CallAsync("POST /api/OpenPaydProvider/getTransactionInfo");

    [ApiMethodFact("POST /api/OpenPaydProvider/getAccount", "DEV")]
    public Task getAccount() => CallAsync("POST /api/OpenPaydProvider/getAccount");

    [ApiMethodFact("POST /api/OpenPaydProvider/getAccounts", "DEV")]
    public Task getAccounts() => CallAsync("POST /api/OpenPaydProvider/getAccounts");

}

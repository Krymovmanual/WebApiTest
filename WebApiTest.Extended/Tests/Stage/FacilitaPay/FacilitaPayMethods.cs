using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "FacilitaPay")]
public sealed class FacilitaPayMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/FacilitaPayProvider/getToken", "STAGE")]
    public Task getToken() => CallAsync("POST /api/FacilitaPayProvider/getToken");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getExchangeRates", "STAGE")]
    public Task getExchangeRates() => CallAsync("POST /api/FacilitaPayProvider/getExchangeRates");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccounts", "STAGE")]
    public Task getBankAccounts() => CallAsync("POST /api/FacilitaPayProvider/getBankAccounts");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccount", "STAGE")]
    public Task getBankAccount() => CallAsync("POST /api/FacilitaPayProvider/getBankAccount");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccountBalance", "STAGE")]
    public Task getBankAccountBalance() => CallAsync("POST /api/FacilitaPayProvider/getBankAccountBalance");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getTransactions", "STAGE")]
    public Task getTransactions() => CallAsync("POST /api/FacilitaPayProvider/getTransactions");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getTransaction", "STAGE")]
    public Task getTransaction() => CallAsync("POST /api/FacilitaPayProvider/getTransaction");

}

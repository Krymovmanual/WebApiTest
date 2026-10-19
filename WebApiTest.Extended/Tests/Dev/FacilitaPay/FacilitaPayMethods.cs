using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "FacilitaPay")]
public sealed class FacilitaPayMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/FacilitaPayProvider/getToken", "DEV")]
    public Task getToken() => CallAsync("POST /api/FacilitaPayProvider/getToken");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getExchangeRates", "DEV")]
    public Task getExchangeRates() => CallAsync("POST /api/FacilitaPayProvider/getExchangeRates");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccounts", "DEV")]
    public Task getBankAccounts() => CallAsync("POST /api/FacilitaPayProvider/getBankAccounts");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccount", "DEV")]
    public Task getBankAccount() => CallAsync("POST /api/FacilitaPayProvider/getBankAccount");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccountStatement", "DEV")]
    public Task getBankAccountStatement() => CallAsync("POST /api/FacilitaPayProvider/getBankAccountStatement");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccountBalance", "DEV")]
    public Task getBankAccountBalance() => CallAsync("POST /api/FacilitaPayProvider/getBankAccountBalance");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getTransactions", "DEV")]
    public Task getTransactions() => CallAsync("POST /api/FacilitaPayProvider/getTransactions");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getTransaction", "DEV")]
    public Task getTransaction() => CallAsync("POST /api/FacilitaPayProvider/getTransaction");

}

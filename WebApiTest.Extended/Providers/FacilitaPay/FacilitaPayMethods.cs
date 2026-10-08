using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "FacilitaPay")]
public sealed class FacilitaPayMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/FacilitaPayProvider/getToken")]
    public Task getToken() => CallAsync("POST /api/FacilitaPayProvider/getToken");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getExchangeRates")]
    public Task getExchangeRates() => CallAsync("POST /api/FacilitaPayProvider/getExchangeRates");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccounts")]
    public Task getBankAccounts() => CallAsync("POST /api/FacilitaPayProvider/getBankAccounts");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccount")]
    public Task getBankAccount() => CallAsync("POST /api/FacilitaPayProvider/getBankAccount");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccountStatement")]
    public Task getBankAccountStatement() => CallAsync("POST /api/FacilitaPayProvider/getBankAccountStatement");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getBankAccountBalance")]
    public Task getBankAccountBalance() => CallAsync("POST /api/FacilitaPayProvider/getBankAccountBalance");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getTransactions")]
    public Task getTransactions() => CallAsync("POST /api/FacilitaPayProvider/getTransactions");

    [ApiMethodFact("POST /api/FacilitaPayProvider/getTransaction")]
    public Task getTransaction() => CallAsync("POST /api/FacilitaPayProvider/getTransaction");

}

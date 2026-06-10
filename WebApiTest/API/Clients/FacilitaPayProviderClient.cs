using Newtonsoft.Json.Linq;

namespace WebApiTest.API.Clients;

public sealed class FacilitaPayProviderClient : ApiClientBase
{
    public Task<JObject> GetTokenAsync()
    {
        return PostEmptyJsonAsync("/api/FacilitaPayProvider/getToken");
    }

    public Task<JObject> GetExchangeRatesAsync()
    {
        return PostEmptyJsonAsync("/api/FacilitaPayProvider/getExchangeRates");
    }

    public Task<JObject> GetBankAccountsAsync()
    {
        return PostEmptyJsonAsync("/api/FacilitaPayProvider/getBankAccounts");
    }
}

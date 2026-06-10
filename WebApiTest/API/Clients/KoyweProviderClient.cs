using Newtonsoft.Json.Linq;

namespace WebApiTest.API.Clients;

public sealed class KoyweProviderClient : ApiClientBase
{
    public Task<JObject> GetTokenAsync()
    {
        return PostEmptyJsonAsync("/api/KoyweProvider/getToken");
    }

    public Task<JObject> GetAllCurrencyTokenPairsAsync()
    {
        return PostEmptyJsonAsync("/api/KoyweProvider/getAllCurrencyTokenPairs");
    }
}

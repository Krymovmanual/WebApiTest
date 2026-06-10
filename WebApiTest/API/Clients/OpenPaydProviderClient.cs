using Newtonsoft.Json.Linq;

namespace WebApiTest.API.Clients;

public sealed class OpenPaydProviderClient : ApiClientBase
{
    public Task<JObject> GetTokenAsync()
    {
        return PostEmptyJsonAsync("/api/OpenPaydProvider/getToken");
    }
}

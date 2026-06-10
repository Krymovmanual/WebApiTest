using Newtonsoft.Json.Linq;

namespace WebApiTest.API.Clients;

public sealed class NuveiProviderClient : ApiClientBase
{
    public Task<JObject> GetSessionTokenAsync()
    {
        return PostEmptyJsonAsync("/api/NuveiProvider/getSessionToken");
    }
}

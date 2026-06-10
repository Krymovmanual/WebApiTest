using Newtonsoft.Json.Linq;

namespace WebApiTest.API.Clients;

public sealed class FireblocksProviderClient : ApiClientBase
{
    public Task<JObject> GetExchangeAccountsAsync()
    {
        return PostEmptyJsonAsync("/api/FireblocksProvider/getExchangeAccounts");
    }

    public Task<JObject> GetAssetsAsync()
    {
        return PostEmptyJsonAsync("/api/FireblocksProvider/getAssets");
    }

    public Task<JObject> GetFiatAccountsAsync()
    {
        return PostEmptyJsonAsync("/api/FireblocksProvider/getFiatAccounts");
    }

    public Task<JObject> GetInternalWalletsAsync()
    {
        return PostEmptyJsonAsync("/api/FireblocksProvider/getInternalWallets");
    }

    public Task<JObject> GetExternalWalletsAsync()
    {
        return PostEmptyJsonAsync("/api/FireblocksProvider/getExternalWallets");
    }
}

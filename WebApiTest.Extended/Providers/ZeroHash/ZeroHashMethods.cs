using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "ZeroHash")]
public sealed class ZeroHashMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("GET /api/ZeroHashProvider/assets")]
    public Task assets() => CallAsync("GET /api/ZeroHashProvider/assets");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses")]
    public Task withdrawals_digital_asset_addresses() => CallAsync("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses");

    [ApiMethodFact("GET /api/ZeroHashProvider/transfers")]
    public Task transfers() => CallAsync("GET /api/ZeroHashProvider/transfers");

    [ApiMethodFact("GET /api/ZeroHashProvider/movements")]
    public Task movements() => CallAsync("GET /api/ZeroHashProvider/movements");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts")]
    public Task accounts() => CallAsync("GET /api/ZeroHashProvider/accounts");

    [ApiMethodFact("GET /api/ZeroHashProvider/deposits")]
    public Task deposits() => CallAsync("GET /api/ZeroHashProvider/deposits");

}

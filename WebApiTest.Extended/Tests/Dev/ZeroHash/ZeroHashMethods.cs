using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "ZeroHash")]
public sealed class ZeroHashMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("GET /api/ZeroHashProvider/assets", "DEV")]
    public Task assets() => CallAsync("GET /api/ZeroHashProvider/assets");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses", "DEV")]
    public Task withdrawals_digital_asset_addresses() => CallAsync("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses");

    [ApiMethodFact("GET /api/ZeroHashProvider/transfers", "DEV")]
    public Task transfers() => CallAsync("GET /api/ZeroHashProvider/transfers");

    [ApiMethodFact("GET /api/ZeroHashProvider/movements", "DEV")]
    public Task movements() => CallAsync("GET /api/ZeroHashProvider/movements");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts", "DEV")]
    public Task accounts() => CallAsync("GET /api/ZeroHashProvider/accounts");

    [ApiMethodFact("GET /api/ZeroHashProvider/deposits", "DEV")]
    public Task deposits() => CallAsync("GET /api/ZeroHashProvider/deposits");

}

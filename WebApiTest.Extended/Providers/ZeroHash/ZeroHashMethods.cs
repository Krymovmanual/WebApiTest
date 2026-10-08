using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "ZeroHash")]
public sealed class ZeroHashMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("GET /api/ZeroHashProvider/assets")]
    public Task assets() => CallAsync("GET /api/ZeroHashProvider/assets");


    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/requests/{id}")]
    public Task withdrawals_requests_id() => CallAsync("GET /api/ZeroHashProvider/withdrawals/requests/{id}");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/locked_network_fee")]
    public Task withdrawals_locked_network_fee() => CallAsync("GET /api/ZeroHashProvider/withdrawals/locked_network_fee");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/estimate_network_fee")]
    public Task withdrawals_estimate_network_fee() => CallAsync("GET /api/ZeroHashProvider/withdrawals/estimate_network_fee");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses")]
    public Task withdrawals_digital_asset_addresses() => CallAsync("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses/{id}")]
    public Task withdrawals_digital_asset_addresses_id() => CallAsync("GET /api/ZeroHashProvider/withdrawals/digital_asset_addresses/{id}");

    [ApiMethodFact("GET /api/ZeroHashProvider/withdrawals/validate_address")]
    public Task withdrawals_validate_address() => CallAsync("GET /api/ZeroHashProvider/withdrawals/validate_address");

    [ApiMethodFact("GET /api/ZeroHashProvider/transfers")]
    public Task transfers() => CallAsync("GET /api/ZeroHashProvider/transfers");

    [ApiMethodFact("GET /api/ZeroHashProvider/transfers/{id}")]
    public Task transfers_id() => CallAsync("GET /api/ZeroHashProvider/transfers/{id}");

    [ApiMethodFact("GET /api/ZeroHashProvider/movements")]
    public Task movements() => CallAsync("GET /api/ZeroHashProvider/movements");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts")]
    public Task accounts() => CallAsync("GET /api/ZeroHashProvider/accounts");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts/{accountId}")]
    public Task accounts_accountId() => CallAsync("GET /api/ZeroHashProvider/accounts/{accountId}");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts/{accountId}/movements")]
    public Task accounts_accountId_movements() => CallAsync("GET /api/ZeroHashProvider/accounts/{accountId}/movements");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts/{accountId}/run_history")]
    public Task accounts_accountId_run_history() => CallAsync("GET /api/ZeroHashProvider/accounts/{accountId}/run_history");

    [ApiMethodFact("GET /api/ZeroHashProvider/accounts/{zrn}/details")]
    public Task accounts_zrn_details() => CallAsync("GET /api/ZeroHashProvider/accounts/{zrn}/details");


    [ApiMethodFact("GET /api/ZeroHashProvider/deposits")]
    public Task deposits() => CallAsync("GET /api/ZeroHashProvider/deposits");

}

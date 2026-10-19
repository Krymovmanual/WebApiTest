using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Fireblocks")]
public sealed class FireblocksMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/FireblocksProvider/getAssetsData", "DEV")]
    public Task getAssetsData() => CallAsync("POST /api/FireblocksProvider/getAssetsData");

    [ApiMethodFact("POST /api/FireblocksProvider/getAssets", "DEV")]
    public Task getAssets() => CallAsync("POST /api/FireblocksProvider/getAssets");

    [ApiMethodFact("POST /api/FireblocksProvider/getVaultAccountsPaged", "DEV")]
    public Task getVaultAccountsPaged() => CallAsync("POST /api/FireblocksProvider/getVaultAccountsPaged");

    [ApiMethodFact("POST /api/FireblocksProvider/getVaultAccount", "DEV")]
    public Task getVaultAccount() => CallAsync("POST /api/FireblocksProvider/getVaultAccount");

    [ApiMethodFact("POST /api/FireblocksProvider/getFiatAccounts", "DEV")]
    public Task getFiatAccounts() => CallAsync("POST /api/FireblocksProvider/getFiatAccounts");

    [ApiMethodFact("POST /api/FireblocksProvider/getNetworkFee", "DEV")]
    public Task getNetworkFee() => CallAsync("POST /api/FireblocksProvider/getNetworkFee");

    [ApiMethodFact("POST /api/FireblocksProvider/getTransactions", "DEV")]
    public Task getTransactions() => CallAsync("POST /api/FireblocksProvider/getTransactions");

    [ApiMethodFact("POST /api/FireblocksProvider/getTransaction", "DEV")]
    public Task getTransaction() => CallAsync("POST /api/FireblocksProvider/getTransaction");

    [ApiMethodFact("POST /api/FireblocksProvider/getMaxSpendableAmount", "DEV")]
    public Task getMaxSpendableAmount() => CallAsync("POST /api/FireblocksProvider/getMaxSpendableAmount");

    [ApiMethodFact("POST /api/FireblocksProvider/getUnspentInputs", "DEV")]
    public Task getUnspentInputs() => CallAsync("POST /api/FireblocksProvider/getUnspentInputs");

    [ApiMethodFact("POST /api/FireblocksProvider/getBalance", "DEV")]
    public Task getBalance() => CallAsync("POST /api/FireblocksProvider/getBalance");

    [ApiMethodFact("POST /api/FireblocksProvider/getInternalWallets", "DEV")]
    public Task getInternalWallets() => CallAsync("POST /api/FireblocksProvider/getInternalWallets");

    [ApiMethodFact("POST /api/FireblocksProvider/getExternalWallets", "DEV")]
    public Task getExternalWallets() => CallAsync("POST /api/FireblocksProvider/getExternalWallets");

    [ApiMethodFact("POST /api/FireblocksProvider/getAccountAddresses", "DEV")]
    public Task getAccountAddresses() => CallAsync("POST /api/FireblocksProvider/getAccountAddresses");

    [ApiMethodFact("POST /api/FireblocksProvider/getAccountAddressesPaginated", "DEV")]
    public Task getAccountAddressesPaginated() => CallAsync("POST /api/FireblocksProvider/getAccountAddressesPaginated");

    [ApiMethodFact("POST /api/FireblocksProvider/getExchangeAccounts", "DEV")]
    public Task getExchangeAccounts() => CallAsync("POST /api/FireblocksProvider/getExchangeAccounts");

    [ApiMethodFact("POST /api/FireblocksProvider/getExchangeAccountById", "DEV")]
    public Task getExchangeAccountById() => CallAsync("POST /api/FireblocksProvider/getExchangeAccountById");

    [ApiMethodFact("POST /api/FireblocksProvider/getFeeForAsset", "DEV")]
    public Task getFeeForAsset() => CallAsync("POST /api/FireblocksProvider/getFeeForAsset");

    [ApiMethodFact("POST /api/FireblocksProvider/createTransaction", "DEV")]
    public Task createTransaction() => CallAsync("POST /api/FireblocksProvider/createTransaction");
    [ApiMethodFact("POST /api/FireblocksProvider/setAutoFuel", "DEV")]
    public Task setAutoFuel() => CallAsync("POST /api/FireblocksProvider/setAutoFuel");

    [ApiMethodFact("POST /api/FireblocksProvider/refreshBalance", "DEV")]
    public Task refreshBalance() => CallAsync("POST /api/FireblocksProvider/refreshBalance");
}

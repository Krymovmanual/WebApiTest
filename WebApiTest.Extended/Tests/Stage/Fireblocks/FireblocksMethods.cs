using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Stage;

[Collection("API methods"), Trait("Environment", "STAGE"), Trait("Provider", "Fireblocks")]
public sealed class FireblocksMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "STAGE")
{
    [ApiMethodFact("POST /api/FireblocksProvider/getAssetsData", "STAGE")]
    public Task getAssetsData() => CallAsync("POST /api/FireblocksProvider/getAssetsData");

    [ApiMethodFact("POST /api/FireblocksProvider/getAssets", "STAGE")]
    public Task getAssets() => CallAsync("POST /api/FireblocksProvider/getAssets");

    [ApiMethodFact("POST /api/FireblocksProvider/getVaultAccountsPaged", "STAGE")]
    public Task getVaultAccountsPaged() => CallAsync("POST /api/FireblocksProvider/getVaultAccountsPaged");

    [ApiMethodFact("POST /api/FireblocksProvider/getVaultAccount", "STAGE")]
    public Task getVaultAccount() => CallAsync("POST /api/FireblocksProvider/getVaultAccount");

    [ApiMethodFact("POST /api/FireblocksProvider/getFiatAccounts", "STAGE")]
    public Task getFiatAccounts() => CallAsync("POST /api/FireblocksProvider/getFiatAccounts");

    [ApiMethodFact("POST /api/FireblocksProvider/getBalance", "STAGE")]
    public Task getBalance() => CallAsync("POST /api/FireblocksProvider/getBalance");

    [ApiMethodFact("POST /api/FireblocksProvider/getInternalWallets", "STAGE")]
    public Task getInternalWallets() => CallAsync("POST /api/FireblocksProvider/getInternalWallets");

    [ApiMethodFact("POST /api/FireblocksProvider/getExternalWallets", "STAGE")]
    public Task getExternalWallets() => CallAsync("POST /api/FireblocksProvider/getExternalWallets");

    [ApiMethodFact("POST /api/FireblocksProvider/getAccountAddresses", "STAGE")]
    public Task getAccountAddresses() => CallAsync("POST /api/FireblocksProvider/getAccountAddresses");

    [ApiMethodFact("POST /api/FireblocksProvider/getExchangeAccounts", "STAGE")]
    public Task getExchangeAccounts() => CallAsync("POST /api/FireblocksProvider/getExchangeAccounts");

    [ApiMethodFact("POST /api/FireblocksProvider/getAccountAddressesPaginated", "STAGE")]
    public Task getAccountAddressesPaginated() => CallAsync("POST /api/FireblocksProvider/getAccountAddressesPaginated");

    [ApiMethodFact("POST /api/FireblocksProvider/getExchangeAccountById", "STAGE")]
    public Task getExchangeAccountById() => CallAsync("POST /api/FireblocksProvider/getExchangeAccountById");

    [ApiMethodFact("POST /api/FireblocksProvider/getNetworkFee", "STAGE")]
    public Task getNetworkFee() => CallAsync("POST /api/FireblocksProvider/getNetworkFee");

    [ApiMethodFact("POST /api/FireblocksProvider/getMaxSpendableAmount", "STAGE")]
    public Task getMaxSpendableAmount() => CallAsync("POST /api/FireblocksProvider/getMaxSpendableAmount");

    [ApiMethodFact("POST /api/FireblocksProvider/getUnspentInputs", "STAGE")]
    public Task getUnspentInputs() => CallAsync("POST /api/FireblocksProvider/getUnspentInputs");

    [ApiMethodFact("POST /api/FireblocksProvider/getFeeForAsset", "STAGE")]
    public Task getFeeForAsset() => CallAsync("POST /api/FireblocksProvider/getFeeForAsset");

    [ApiMethodFact("POST /api/FireblocksProvider/getTransactions", "STAGE")]
    public Task getTransactions() => CallAsync("POST /api/FireblocksProvider/getTransactions");

    [ApiMethodFact("POST /api/FireblocksProvider/getTransaction", "STAGE")]
    public Task getTransaction() => CallAsync("POST /api/FireblocksProvider/getTransaction");
}

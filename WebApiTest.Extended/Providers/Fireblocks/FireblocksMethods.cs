using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Fireblocks"), Trait("Suite", "Live")]
public sealed class FireblocksMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/FireblocksProvider/getAssetsData")]
    public Task getAssetsData() => CallAsync("POST /api/FireblocksProvider/getAssetsData");

    [ApiMethodFact("POST /api/FireblocksProvider/getAssets")]
    public Task getAssets() => CallAsync("POST /api/FireblocksProvider/getAssets");

    [ApiMethodFact("POST /api/FireblocksProvider/getVaultAccountsPaged")]
    public Task getVaultAccountsPaged() => CallAsync("POST /api/FireblocksProvider/getVaultAccountsPaged");

    [ApiMethodFact("POST /api/FireblocksProvider/getVaultAccount")]
    public Task getVaultAccount() => CallAsync("POST /api/FireblocksProvider/getVaultAccount");

    [ApiMethodFact("POST /api/FireblocksProvider/getFiatAccounts")]
    public Task getFiatAccounts() => CallAsync("POST /api/FireblocksProvider/getFiatAccounts");

    [ApiMethodFact("POST /api/FireblocksProvider/getFiatAccount")]
    public Task getFiatAccount() => CallAsync("POST /api/FireblocksProvider/getFiatAccount");

    [ApiMethodFact("POST /api/FireblocksProvider/getNetworkFee")]
    public Task getNetworkFee() => CallAsync("POST /api/FireblocksProvider/getNetworkFee");

    [ApiMethodFact("POST /api/FireblocksProvider/getTransactions")]
    public Task getTransactions() => CallAsync("POST /api/FireblocksProvider/getTransactions");

    [ApiMethodFact("POST /api/FireblocksProvider/getTransaction")]
    public Task getTransaction() => CallAsync("POST /api/FireblocksProvider/getTransaction");

    [ApiMethodFact("POST /api/FireblocksProvider/getMaxSpendableAmount")]
    public Task getMaxSpendableAmount() => CallAsync("POST /api/FireblocksProvider/getMaxSpendableAmount");

    [ApiMethodFact("POST /api/FireblocksProvider/getUnspentInputs")]
    public Task getUnspentInputs() => CallAsync("POST /api/FireblocksProvider/getUnspentInputs");

    [ApiMethodFact("POST /api/FireblocksProvider/getBalance")]
    public Task getBalance() => CallAsync("POST /api/FireblocksProvider/getBalance");

    [ApiMethodFact("POST /api/FireblocksProvider/getInternalWallets")]
    public Task getInternalWallets() => CallAsync("POST /api/FireblocksProvider/getInternalWallets");

    [ApiMethodFact("POST /api/FireblocksProvider/getInternalWallet")]
    public Task getInternalWallet() => CallAsync("POST /api/FireblocksProvider/getInternalWallet");

    [ApiMethodFact("POST /api/FireblocksProvider/getInternalWalletPaginated")]
    public Task getInternalWalletPaginated() => CallAsync("POST /api/FireblocksProvider/getInternalWalletPaginated");

    [ApiMethodFact("POST /api/FireblocksProvider/getInternalWalletAsset")]
    public Task getInternalWalletAsset() => CallAsync("POST /api/FireblocksProvider/getInternalWalletAsset");

    [ApiMethodFact("POST /api/FireblocksProvider/getExternalWallets")]
    public Task getExternalWallets() => CallAsync("POST /api/FireblocksProvider/getExternalWallets");

    [ApiMethodFact("POST /api/FireblocksProvider/getExternalWallet")]
    public Task getExternalWallet() => CallAsync("POST /api/FireblocksProvider/getExternalWallet");

    [ApiMethodFact("POST /api/FireblocksProvider/getExternalWalletAsset")]
    public Task getExternalWalletAsset() => CallAsync("POST /api/FireblocksProvider/getExternalWalletAsset");

    [ApiMethodFact("POST /api/FireblocksProvider/getAccountAddresses")]
    public Task getAccountAddresses() => CallAsync("POST /api/FireblocksProvider/getAccountAddresses");

    [ApiMethodFact("POST /api/FireblocksProvider/getAccountAddressesPaginated")]
    public Task getAccountAddressesPaginated() => CallAsync("POST /api/FireblocksProvider/getAccountAddressesPaginated");

    [ApiMethodFact("POST /api/FireblocksProvider/getExchangeAccounts")]
    public Task getExchangeAccounts() => CallAsync("POST /api/FireblocksProvider/getExchangeAccounts");

    [ApiMethodFact("POST /api/FireblocksProvider/getExchangeAccountById")]
    public Task getExchangeAccountById() => CallAsync("POST /api/FireblocksProvider/getExchangeAccountById");

    [ApiMethodFact("POST /api/FireblocksProvider/getFeeForAsset")]
    public Task getFeeForAsset() => CallAsync("POST /api/FireblocksProvider/getFeeForAsset");

}

using Xunit.Abstractions;
namespace WebApiTest.Extended;

public abstract class ProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output)
{
    protected async Task Reject(string method, string path, string mode)
    {
        output.WriteLine("Authorization: {0} bearer; expected HTTP 401 or 403.", mode);
        var response = await TestReport.SendAsync(api, output, method, path, mode == "missing" ? null : "not-a-valid-token", expectedStatuses: [401, 403]);
        output.WriteLine("Negative authorization test: an expected rejection does not verify provider functionality.");
        Assert.True(response.Status is 401 or 403,
            $"{method} {path}: {mode} bearer returned HTTP {response.Status}; expected 401/403. Body omitted.");
    }
}

[Collection("Stage reads")]
public class BitoloAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/Bitolo/ARS/getBalance", "missing" },
        new object[] { "POST", "/api/Bitolo/ARS/getBalance", "malformed" },
        new object[] { "POST", "/api/Bitolo/ARS/getTransactionStatus", "missing" },
        new object[] { "POST", "/api/Bitolo/ARS/getTransactionStatus", "malformed" },
        new object[] { "POST", "/api/Bitolo/ARS/getTransactionList", "missing" },
        new object[] { "POST", "/api/Bitolo/ARS/getTransactionList", "malformed" },
        new object[] { "POST", "/api/Bitolo/ARS/resendCallback", "missing" },
        new object[] { "POST", "/api/Bitolo/ARS/resendCallback", "malformed" },
        new object[] { "POST", "/api/Bitolo/ARS/withdraw", "missing" },
        new object[] { "POST", "/api/Bitolo/ARS/withdraw", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "Bitolo")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class CoinPaymentsProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/CoinPaymentsProvider/getBalances", "missing" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getBalances", "malformed" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getExchangeRates", "missing" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getExchangeRates", "malformed" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getWithdrawalInformation", "missing" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getWithdrawalInformation", "malformed" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getWithdrawalHistory", "missing" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getWithdrawalHistory", "malformed" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getCreateWithdrawal", "missing" },
        new object[] { "POST", "/api/CoinPaymentsProvider/getCreateWithdrawal", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "CoinPaymentsProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class ConsolidationAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/Consolidation/adresses", "missing" },
        new object[] { "POST", "/api/Consolidation/adresses", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "Consolidation")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class DistributedKeysAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/DistributedKeys/checkEnvAuth", "missing" },
        new object[] { "POST", "/api/DistributedKeys/checkEnvAuth", "malformed" },
        new object[] { "POST", "/api/DistributedKeys/loadDistributedKeysAuth", "missing" },
        new object[] { "POST", "/api/DistributedKeys/loadDistributedKeysAuth", "malformed" },
        new object[] { "POST", "/api/DistributedKeys/unloadDistributedKeysAuth", "missing" },
        new object[] { "POST", "/api/DistributedKeys/unloadDistributedKeysAuth", "malformed" },
        new object[] { "POST", "/api/DistributedKeys/clearKeyVault", "missing" },
        new object[] { "POST", "/api/DistributedKeys/clearKeyVault", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "DistributedKeys")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class FacilitaPayProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/FacilitaPayProvider/getToken", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getToken", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getExchangeRates", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getExchangeRates", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccounts", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccounts", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccount", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccount", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccountStatement", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccountStatement", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccountBalance", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getBankAccountBalance", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getTransactions", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getTransactions", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/getTransaction", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/getTransaction", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/createInternalWithdrawal", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/createInternalWithdrawal", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/createWithdrawal", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/createWithdrawal", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/enableWebhooks", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/enableWebhooks", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/disableWebhooks", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/disableWebhooks", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/refund", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/refund", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/refundReceivedTransaction", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/refundReceivedTransaction", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/notifications", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/notifications", "malformed" },
        new object[] { "POST", "/api/FacilitaPayProvider/resendNotification", "missing" },
        new object[] { "POST", "/api/FacilitaPayProvider/resendNotification", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "FacilitaPayProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class FireblocksGasStationProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/FireblocksGasStationProvider/createTransaction", "missing" },
        new object[] { "POST", "/api/FireblocksGasStationProvider/createTransaction", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "FireblocksGasStationProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class FireblocksProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/FireblocksProvider/getAssetsData", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getAssetsData", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getAssets", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getAssets", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getVaultAccountsPaged", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getVaultAccountsPaged", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getVaultAccount", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getVaultAccount", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getFiatAccounts", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getFiatAccounts", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getFiatAccount", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getFiatAccount", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getNetworkFee", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getNetworkFee", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getTransactions", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getTransactions", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getTransaction", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getTransaction", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createTransaction", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createTransaction", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/cancelTransaction", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/cancelTransaction", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/estimateFeeForTransaction", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/estimateFeeForTransaction", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createValutAccount", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createValutAccount", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/updateVaultAccount", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/updateVaultAccount", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/setAutoFuel", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/setAutoFuel", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getMaxSpendableAmount", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getMaxSpendableAmount", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getUnspentInputs", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getUnspentInputs", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getBalance", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getBalance", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/refreshBalance", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/refreshBalance", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createVaultAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createVaultAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getInternalWallets", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getInternalWallets", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getInternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getInternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getInternalWalletAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getInternalWalletAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createInternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createInternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForInternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForInternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForVaultAccount", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForVaultAccount", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createInternalWalletAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createInternalWalletAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/deleteInternalWalletAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/deleteInternalWalletAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/deleteInternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/deleteInternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getExternalWallets", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getExternalWallets", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getExternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getExternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getExternalWalletAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getExternalWalletAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createExternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createExternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForExternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForExternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/createExternalWalletAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/createExternalWalletAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/deleteExternalWalletAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/deleteExternalWalletAsset", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/deleteExternalWallet", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/deleteExternalWallet", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getAccountAddresses", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getAccountAddresses", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getAccountAddressesPaginated", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getAccountAddressesPaginated", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/generateNewAddress", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/generateNewAddress", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForAddress", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/setCustomerRefIdForAddress", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/setDescriptionForAddress", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/setDescriptionForAddress", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getExchangeAccounts", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getExchangeAccounts", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getExchangeAccountById", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getExchangeAccountById", "malformed" },
        new object[] { "POST", "/api/FireblocksProvider/getFeeForAsset", "missing" },
        new object[] { "POST", "/api/FireblocksProvider/getFeeForAsset", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "FireblocksProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class InfuraAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/Infura/ethereum/getAccounts", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getAccounts", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/generateAddress", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/generateAddress", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getBalance", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getBalance", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/getTransactions", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/getTransactions", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getEthTransactions", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getEthTransactions", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getBlockByNumber/0", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getBlockByNumber/0", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getTransactionRecipient", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getTransactionRecipient", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getTransactionByHash", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getTransactionByHash", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getGasPrice", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getGasPrice", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/estimateFees", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/estimateFees", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/getBalance", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/getBalance", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/getTransactionByHash", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/getTransactionByHash", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/sendTransaction", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/sendTransaction", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/boostTransaction", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/boostTransaction", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/cancelTransaction", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/token/0x0000000000000000000000000000000000000000/cancelTransaction", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/sendTransaction", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/sendTransaction", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/boostTransaction", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/boostTransaction", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/cancelTransaction", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/cancelTransaction", "malformed" },
        new object[] { "POST", "/api/Infura/ethereum/getTransactionCount", "missing" },
        new object[] { "POST", "/api/Infura/ethereum/getTransactionCount", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "Infura")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}


[Collection("Stage reads")]
public class KoyweProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/KoyweProvider/getToken", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getToken", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getAccounyInfo", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getAccounyInfo", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getAllCurrencyTokenPairs", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getAllCurrencyTokenPairs", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getCurrencyTokenPairs", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getCurrencyTokenPairs", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getAllTokenCurrenciesPairs", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getAllTokenCurrenciesPairs", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getTokenCurrenciesPairs", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getTokenCurrenciesPairs", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getPaymentMethods", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getPaymentMethods", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getPayoutMethods", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getPayoutMethods", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getHistory", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getHistory", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getOrderInfo", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getOrderInfo", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getOrderInfoByExternalId", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getOrderInfoByExternalId", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/postOrder", "missing" },
        new object[] { "POST", "/api/KoyweProvider/postOrder", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/getBalance", "missing" },
        new object[] { "POST", "/api/KoyweProvider/getBalance", "malformed" },
        new object[] { "GET", "/api/KoyweProvider/quotes/webapi-test-nonexistent", "missing" },
        new object[] { "GET", "/api/KoyweProvider/quotes/webapi-test-nonexistent", "malformed" },
        new object[] { "POST", "/api/KoyweProvider/quotes", "missing" },
        new object[] { "POST", "/api/KoyweProvider/quotes", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "KoyweProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class MailAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/Mail/sendMail", "missing" },
        new object[] { "POST", "/api/Mail/sendMail", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "Mail")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}


[Collection("Stage reads")]
public class Nuvei_v2_ProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/Nuvei_v2_Provider/getSessionToken", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/getSessionToken", "malformed" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/payout", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/payout", "malformed" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/getPayoutStatus", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/getPayoutStatus", "malformed" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/getRequests", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/getRequests", "malformed" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/approveRequest", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/approveRequest", "malformed" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/cancelRequest", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/cancelRequest", "malformed" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/submitRequest", "missing" },
        new object[] { "POST", "/api/Nuvei_v2_Provider/submitRequest", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "Nuvei_v2_Provider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class NuveiProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/NuveiProvider/getSessionToken", "missing" },
        new object[] { "POST", "/api/NuveiProvider/getSessionToken", "malformed" },
        new object[] { "POST", "/api/NuveiProvider/payout", "missing" },
        new object[] { "POST", "/api/NuveiProvider/payout", "malformed" },
        new object[] { "POST", "/api/NuveiProvider/getPayoutStatus", "missing" },
        new object[] { "POST", "/api/NuveiProvider/getPayoutStatus", "malformed" },
        new object[] { "POST", "/api/NuveiProvider/getRequests", "missing" },
        new object[] { "POST", "/api/NuveiProvider/getRequests", "malformed" },
        new object[] { "POST", "/api/NuveiProvider/approveRequest", "missing" },
        new object[] { "POST", "/api/NuveiProvider/approveRequest", "malformed" },
        new object[] { "POST", "/api/NuveiProvider/cancelRequest", "missing" },
        new object[] { "POST", "/api/NuveiProvider/cancelRequest", "malformed" },
        new object[] { "POST", "/api/NuveiProvider/submitRequest", "missing" },
        new object[] { "POST", "/api/NuveiProvider/submitRequest", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "NuveiProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class OpenPaydProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/OpenPaydProvider/getToken", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/getToken", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/getHistory", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/getHistory", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/getTransactionInfo", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/getTransactionInfo", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/createSweepTransaction", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/createSweepTransaction", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/updateTransaction", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/updateTransaction", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/updatePayoutStatus", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/updatePayoutStatus", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/getAccount", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/getAccount", "malformed" },
        new object[] { "POST", "/api/OpenPaydProvider/getAccounts", "missing" },
        new object[] { "POST", "/api/OpenPaydProvider/getAccounts", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "OpenPaydProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}


[Collection("Stage reads")]
public class QDTAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/QDT/statistic", "missing" },
        new object[] { "POST", "/api/QDT/statistic", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "QDT")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class QuantfuryPaymentsProviderAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/deploydate", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/deploydate", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/getKyc", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/getKyc", "malformed" },
        new object[] { "PUT", "/api/QuantfuryPaymentsProvider/kyc", "missing" },
        new object[] { "PUT", "/api/QuantfuryPaymentsProvider/kyc", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/process", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/process", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/processStockWithdrawal", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/processStockWithdrawal", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/withdrawal", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/withdrawal", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositCustomerAddress", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositCustomerAddress", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositFacilitaPay", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositFacilitaPay", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositNuvei", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositNuvei", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/eventWebhookNuvei", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/eventWebhookNuvei", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositKoywe", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositKoywe", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/updateAccountKoywe", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/updateAccountKoywe", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositOpenPayd", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositOpenPayd", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositBitolo", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositBitolo", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositPaysafe", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositPaysafe", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositZeroHash", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/depositZeroHash", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/putRefundDeposit", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsProvider/putRefundDeposit", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "QuantfuryPaymentsProvider")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class QuantfuryPaymentsSignAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/QuantfuryPaymentsSign/insertSignature", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/insertSignature", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/updatePublicKeyFromFile/webapi-test-nonexistent", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/updatePublicKeyFromFile/webapi-test-nonexistent", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/updatePublicKey", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/updatePublicKey", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/signBody", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/signBody", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/verifyBody", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/verifyBody", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/signBodyString", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/signBodyString", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/verifyBodyString", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/verifyBodyString", "malformed" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/insertSignatureForConsolidationPayments", "missing" },
        new object[] { "POST", "/api/QuantfuryPaymentsSign/insertSignatureForConsolidationPayments", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "QuantfuryPaymentsSign")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}

[Collection("Stage reads")]
public class validate_addressAuthorizationContracts(ApiHarness api, ITestOutputHelper output) : ProviderAuthorizationContracts(api, output)
{
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "POST", "/api/validate-address", "missing" },
        new object[] { "POST", "/api/validate-address", "malformed" },
    };
    [LiveTheory, MemberData(nameof(Cases)), Trait("Suite", "Live"), Trait("Coverage", "Authorization"), Trait("Provider", "validate-address")]
    public Task RejectsMissingOrMalformedBearer(string method, string path, string mode) => Reject(method, path, mode);
}


using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Collection("API methods"), Trait("Provider", "Infura"), Trait("Suite", "Live")]
public sealed class InfuraMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output)
{
    [ApiMethodFact("POST /api/Infura/{blockchain}/getAccounts")]
    public Task getAccounts() => CallAsync("POST /api/Infura/{blockchain}/getAccounts");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getBalance")]
    public Task getBalance() => CallAsync("POST /api/Infura/{blockchain}/getBalance");

    [ApiMethodFact("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactions")]
    public Task token_contractAddress_getTransactions() => CallAsync("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactions");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getEthTransactions")]
    public Task getEthTransactions() => CallAsync("POST /api/Infura/{blockchain}/getEthTransactions");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getBlockByNumber/{blockNumber}")]
    public Task getBlockByNumber_blockNumber() => CallAsync("POST /api/Infura/{blockchain}/getBlockByNumber/{blockNumber}");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getTransactionRecipient")]
    public Task getTransactionRecipient() => CallAsync("POST /api/Infura/{blockchain}/getTransactionRecipient");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getTransactionByHash")]
    public Task getTransactionByHash() => CallAsync("POST /api/Infura/{blockchain}/getTransactionByHash");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getGasPrice")]
    public Task getGasPrice() => CallAsync("POST /api/Infura/{blockchain}/getGasPrice");

    [ApiMethodFact("POST /api/Infura/{blockchain}/token/{contractAddress}/getBalance")]
    public Task token_contractAddress_getBalance() => CallAsync("POST /api/Infura/{blockchain}/token/{contractAddress}/getBalance");

    [ApiMethodFact("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactionByHash")]
    public Task token_contractAddress_getTransactionByHash() => CallAsync("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactionByHash");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getTransactionCount")]
    public Task getTransactionCount() => CallAsync("POST /api/Infura/{blockchain}/getTransactionCount");

}

using Xunit.Abstractions;

namespace WebApiTest.Extended.Tests.Dev;

[Collection("API methods"), Trait("Environment", "DEV"), Trait("Provider", "Infura")]
public sealed class InfuraMethods(ApiHarness api, ITestOutputHelper output) : ApiMethodTestsBase(api, output, "DEV")
{
    [ApiMethodFact("POST /api/Infura/{blockchain}/getAccounts", "DEV")]
    public Task getAccounts() => CallAsync("POST /api/Infura/{blockchain}/getAccounts");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getBalance", "DEV")]
    public Task getBalance() => CallAsync("POST /api/Infura/{blockchain}/getBalance");

    [ApiMethodFact("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactions", "DEV")]
    public Task token_contractAddress_getTransactions() => CallAsync("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactions");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getEthTransactions", "DEV")]
    public Task getEthTransactions() => CallAsync("POST /api/Infura/{blockchain}/getEthTransactions");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getBlockByNumber/{blockNumber}", "DEV")]
    public Task getBlockByNumber_blockNumber() => CallAsync("POST /api/Infura/{blockchain}/getBlockByNumber/{blockNumber}");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getTransactionRecipient", "DEV")]
    public Task getTransactionRecipient() => CallAsync("POST /api/Infura/{blockchain}/getTransactionRecipient");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getTransactionByHash", "DEV")]
    public Task getTransactionByHash() => CallAsync("POST /api/Infura/{blockchain}/getTransactionByHash");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getGasPrice", "DEV")]
    public Task getGasPrice() => CallAsync("POST /api/Infura/{blockchain}/getGasPrice");

    [ApiMethodFact("POST /api/Infura/{blockchain}/token/{contractAddress}/getBalance", "DEV")]
    public Task token_contractAddress_getBalance() => CallAsync("POST /api/Infura/{blockchain}/token/{contractAddress}/getBalance");

    [ApiMethodFact("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactionByHash", "DEV")]
    public Task token_contractAddress_getTransactionByHash() => CallAsync("POST /api/Infura/{blockchain}/token/{contractAddress}/getTransactionByHash");

    [ApiMethodFact("POST /api/Infura/{blockchain}/getTransactionCount", "DEV")]
    public Task getTransactionCount() => CallAsync("POST /api/Infura/{blockchain}/getTransactionCount");

    [ApiMethodFact("POST /api/Infura/{blockchain}/estimateFees", "DEV")]
    public Task estimateFees() => CallAsync("POST /api/Infura/{blockchain}/estimateFees");

}

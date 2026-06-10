using System;
using System.Threading.Tasks;
using WebApiTest.API.Clients;
using Newtonsoft.Json.Linq;
using Xunit;

namespace WebApiTest
{
    public class FireblocksProviderTests
    {
        private readonly FireblocksProviderClient client = new();

        [Fact]
        public async Task GetExchangeAccounts_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetExchangeAccountsAsync();
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);
            Assert.NotEmpty(resultArray!);

            var assertions = new List<Action>();

            foreach (var item in resultArray)
            {
                var assetsArray = item["assets"] as JArray;

                assertions.AddRange(new Action[]
                {
                    () => Assert.NotNull(item["id"]),
                    () => Assert.NotNull(item["type"]),
                    () => Assert.NotNull(item["name"]),
                    () => Assert.NotNull(item["assets"]),
                    () => Assert.Matches(@"^[a-f0-9\-]{36}$", item["id"]!.ToString()),
                    () => Assert.Contains(item["type"]!.ToString(), new[] { "DERIBIT_TESTNET", "BITMEX_TESTNET" }),
                    () => Assert.NotNull(assetsArray),
                    () => Assert.NotEmpty(assetsArray!)
                });

                if (assetsArray is null)
                {
                    continue;
                }

                foreach (var asset in assetsArray)
                {
                    assertions.AddRange(new Action[]
                    {
                        () => Assert.NotNull(asset["id"]),
                        () => Assert.NotNull(asset["total"]),
                        () => Assert.NotNull(asset["balance"]),
                        () => Assert.NotNull(asset["lockedAmount"]),
                        () => Assert.NotNull(asset["available"]),
                        () => Assert.True(Convert.ToDecimal(asset["total"]) >= 0),
                        () => Assert.True(Convert.ToDecimal(asset["balance"]) >= 0),
                        () => Assert.True(Convert.ToDecimal(asset["lockedAmount"]) >= 0),
                        () => Assert.True(Convert.ToDecimal(asset["available"]) >= 0)
                    });
                }
            }

            Assert.Multiple(assertions.ToArray());
        }

        [Fact]
        public async Task GetAssets_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetAssetsAsync();
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);
            Assert.NotEmpty(resultArray!);

            var assertions = new List<Action>();

            foreach (var item in resultArray)
            {
                assertions.AddRange(new Action[]
                {
                    () => Assert.NotNull(item["id"]),
                    () => Assert.NotNull(item["name"]),
                    () => Assert.NotNull(item["type"]),
                    () => Assert.NotNull(item["contractAddress"]),
                    () => Assert.NotNull(item["nativeAsset"]),
                    () => Assert.NotNull(item["decimals"]),
                    () => Assert.True(Convert.ToInt32(item["decimals"]) >= 0)
                });

                string contractAddress = item["contractAddress"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(contractAddress))
                {
                    assertions.Add(() => Assert.Matches(@"^0x[a-fA-F0-9]{40}$", contractAddress));
                }
            }

            Assert.Multiple(assertions.ToArray());
        }

        [Fact]
        public async Task GetFiatAccounts_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetFiatAccountsAsync();
            Assert.NotNull(jsonResponse);
        }

        [Fact]
        public async Task GetInternalWallets_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetInternalWalletsAsync();
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);
            Assert.NotEmpty(resultArray!);

            var assertions = new List<Action>();

            foreach (var item in resultArray)
            {
                var assetsArray = item["assets"] as JArray;

                assertions.AddRange(new Action[]
                {
                    () => Assert.NotNull(item["id"]),
                    () => Assert.NotNull(item["name"]),
                    () => Assert.NotNull(item["assets"]),
                    () => Assert.NotNull(assetsArray)
                });

                if (assetsArray is null)
                {
                    continue;
                }

                foreach (var asset in assetsArray)
                {
                    assertions.AddRange(new Action[]
                    {
                        () => Assert.NotNull(asset["id"]),
                        () => Assert.NotNull(asset["status"]),
                        () => Assert.Contains(asset["status"]!.ToString(), new[] { "APPROVED", "PENDING" })
                    });
                }
            }

            Assert.Multiple(assertions.ToArray());
        }

        [Fact]
        public async Task GetExternalWallets_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetExternalWalletsAsync();
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);

            var assertions = new List<Action>();

            foreach (var item in resultArray)
            {
                assertions.AddRange(new Action[]
                {
                    () => Assert.NotNull(item["id"]),
                    () => Assert.Matches(@"^[a-f0-9\-]{36}$", item["id"]!.ToString()),
                    () => Assert.NotNull(item["name"]),
                    () => Assert.NotNull(item["assets"]),
                    () => Assert.IsType<JArray>(item["assets"])
                });
            }

            Assert.Multiple(assertions.ToArray());
        }
    }
}

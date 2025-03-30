using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ApiTests
{
    public class FireblocksProviderTests
    {
        private readonly HttpClient _client;
        private const string BearerToken = "";

        public FireblocksProviderTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://dev-qfwebapi.bkgdsvc.com")
            };

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {BearerToken}");
        }

        private async Task<JObject> PostRequestAsync(string endpoint)
        {
            var requestBody = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(endpoint, requestBody);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            return jsonResponse;
        }

        [Fact]
        public async Task GetExchangeAccounts_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FireblocksProvider/getExchangeAccounts");
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);
            Assert.NotEmpty(resultArray);

            foreach (var item in resultArray)
            {
                Assert.NotNull(item["id"]);
                Assert.NotNull(item["type"]);
                Assert.NotNull(item["name"]);
                Assert.NotNull(item["assets"]);

                Assert.Matches(@"^[a-f0-9\-]{36}$", item["id"].ToString());
                Assert.Contains(item["type"].ToString(), new[] { "DERIBIT_TESTNET", "BITMEX_TESTNET" });

                var assetsArray = item["assets"] as JArray;
                Assert.NotNull(assetsArray);
                Assert.NotEmpty(assetsArray);

                foreach (var asset in assetsArray)
                {
                    Assert.NotNull(asset["id"]);
                    Assert.NotNull(asset["total"]);
                    Assert.NotNull(asset["balance"]);
                    Assert.NotNull(asset["lockedAmount"]);
                    Assert.NotNull(asset["available"]);

                    decimal total = Convert.ToDecimal(asset["total"]);
                    decimal balance = Convert.ToDecimal(asset["balance"]);
                    decimal lockedAmount = Convert.ToDecimal(asset["lockedAmount"]);
                    decimal available = Convert.ToDecimal(asset["available"]);

                    Assert.True(total >= 0);
                    Assert.True(balance >= 0);
                    Assert.True(lockedAmount >= 0);
                    Assert.True(available >= 0);
                }
            }
        }

        [Fact]
        public async Task GetAssets_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FireblocksProvider/getAssets");
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);
            Assert.NotEmpty(resultArray);

            foreach (var item in resultArray)
            {
                Assert.NotNull(item["id"]);
                Assert.NotNull(item["name"]);
                Assert.NotNull(item["type"]);
                Assert.NotNull(item["contractAddress"]);
                Assert.NotNull(item["nativeAsset"]);
                Assert.NotNull(item["decimals"]);

                string contractAddress = item["contractAddress"].ToString();
                if (!string.IsNullOrEmpty(contractAddress))
                {
                    Assert.Matches(@"^0x[a-fA-F0-9]{40}$", contractAddress);
                }

                int decimals = Convert.ToInt32(item["decimals"]);
                Assert.True(decimals >= 0);
            }
        }

        [Fact]
        public async Task GetFiatAccounts_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FireblocksProvider/getFiatAccounts");
            Assert.NotNull(jsonResponse);
        }
        

        [Fact]
        public async Task GetInternalWallets_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FireblocksProvider/getInternalWallets");
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);
            Assert.NotEmpty(resultArray);

            foreach (var item in resultArray)
            {
                Assert.NotNull(item["id"]);
                Assert.NotNull(item["name"]);
                Assert.NotNull(item["assets"]);

                var assetsArray = item["assets"] as JArray;
                Assert.NotNull(assetsArray);

                foreach (var asset in assetsArray)
                {
                    Assert.NotNull(asset["id"]);
                    Assert.NotNull(asset["status"]);
                    Assert.Contains(asset["status"].ToString(), new[] { "APPROVED", "PENDING" });
                }
            }
        }
        [Fact]
        public async Task GetExternalWallets_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FireblocksProvider/getExternalWallets");
            var resultArray = jsonResponse["result"] as JArray;

            Assert.NotNull(resultArray);

            foreach (var item in resultArray)
            {
                Assert.NotNull(item["id"]);
                Assert.Matches(@"^[a-f0-9\-]{36}$", item["id"].ToString());
                Assert.NotNull(item["name"]);
                Assert.NotNull(item["assets"]);
                Assert.IsType<JArray>(item["assets"]);
            }
        }
    }
}
    

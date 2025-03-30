using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ApiTests
{
    public class KoyweApiTests
    {
        private readonly HttpClient _client;
        private const string BearerToken = "";

        public KoyweApiTests()
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
        public async Task ApiRequests_ShouldReturnValidResponses()
        {
            var jsonResponseToken = await PostRequestAsync("/api/KoyweProvider/getToken");

            Assert.NotNull(jsonResponseToken);
            Assert.Equal("ok", jsonResponseToken["error"]?.ToString());

            var resultToken = jsonResponseToken["result"];
            Assert.NotNull(resultToken);
            Assert.NotEmpty(resultToken["token"]?.ToString());

            Assert.Matches(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$", resultToken["token"].ToString());

            var jsonResponseCurrency = await PostRequestAsync("/api/KoyweProvider/getAllCurrencyTokenPairs");

            Assert.NotNull(jsonResponseCurrency);
            Assert.Equal("ok", jsonResponseCurrency["error"]?.ToString());

            var resultCurrency = jsonResponseCurrency["result"];
            Assert.NotNull(resultCurrency);
            Assert.True(resultCurrency.HasValues);  

            foreach (var item in resultCurrency)
            {
                Assert.NotNull(item["_id"]?.ToString());
                Assert.NotNull(item["name"]?.ToString());
                Assert.NotNull(item["symbol"]?.ToString());

                var limits = item["limits"] as JObject;
                Assert.NotNull(limits);
                Assert.True(decimal.TryParse(limits["max"]?.ToString(), out _));
                Assert.True(decimal.TryParse(limits["min"]?.ToString(), out _));

                var tokens = item["tokens"] as JArray;
                Assert.NotNull(tokens);
                Assert.True(tokens.Count > 0);

                foreach (var token in tokens)
                {
                    Assert.NotNull(token["_id"]?.ToString());
                    Assert.NotNull(token["name"]?.ToString());
                    Assert.NotNull(token["symbol"]?.ToString());
                    Assert.NotNull(token["logo"]?.ToString());
                }
            }
        }
    }
}

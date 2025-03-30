using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ApiTests
{
    public class NuveiApiTests
    {
        private readonly HttpClient _client;
        private const string BearerToken = "";  

        public NuveiApiTests()
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
        public async Task GetSessionToken_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/NuveiProvider/getSessionToken");

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            var result = jsonResponse["result"];
            Assert.NotNull(result);
            Assert.NotEmpty(result["sessionToken"]?.ToString());
            Assert.Equal("SUCCESS", result["status"]?.ToString());
            Assert.Equal("0", result["errCode"]?.ToString());
            Assert.NotEmpty(result["merchantId"]?.ToString());
            Assert.NotEmpty(result["merchantSiteId"]?.ToString());
            Assert.Equal("1.0", result["version"]?.ToString());
        }
    }
}

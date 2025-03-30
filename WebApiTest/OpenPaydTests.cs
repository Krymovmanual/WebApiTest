using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ApiTests
{
    public class OpenPaydApiTests
    {
        private readonly HttpClient _client;
        private const string BearerToken = "";

        public OpenPaydApiTests()
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
            var jsonResponseToken = await PostRequestAsync("/api/OpenPaydProvider/getToken");

            Assert.NotNull(jsonResponseToken);
            Assert.Equal("ok", jsonResponseToken["error"]?.ToString());

            var resultToken = jsonResponseToken["result"];
            Assert.NotNull(resultToken);
            Assert.NotEmpty(resultToken["access_token"]?.ToString());

            Assert.Matches(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$", resultToken["access_token"].ToString());
        }
    }
}

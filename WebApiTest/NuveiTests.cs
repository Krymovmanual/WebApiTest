using System.Threading.Tasks;
using WebApiTest.API.Clients;
using Xunit;

namespace WebApiTest
{
    public class NuveiApiTests
    {
        private readonly NuveiProviderClient client = new();

        [Fact]
        public async Task GetSessionToken_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetSessionTokenAsync();

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            var result = jsonResponse["result"];
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result!["sessionToken"]?.ToString()));
            Assert.Equal("SUCCESS", result["status"]?.ToString());
            Assert.Equal("0", result["errCode"]?.ToString());
            Assert.False(string.IsNullOrEmpty(result["merchantId"]?.ToString()));
            Assert.False(string.IsNullOrEmpty(result["merchantSiteId"]?.ToString()));
            Assert.Equal("1.0", result["version"]?.ToString());
        }
    }
}

using System.Threading.Tasks;
using WebApiTest.API.Clients;
using Xunit;

namespace WebApiTest
{
    public class OpenPaydApiTests
    {
        private readonly OpenPaydProviderClient client = new();

        [Fact]
        public async Task ApiRequests_ShouldReturnValidResponses()
        {
            var jsonResponseToken = await client.GetTokenAsync();

            Assert.NotNull(jsonResponseToken);
            Assert.Equal("ok", jsonResponseToken["error"]?.ToString());

            var resultToken = jsonResponseToken["result"];
            Assert.NotNull(resultToken);
            Assert.False(string.IsNullOrEmpty(resultToken!["access_token"]?.ToString()));

            Assert.Matches(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$", resultToken["access_token"]!.ToString());
        }
    }
}

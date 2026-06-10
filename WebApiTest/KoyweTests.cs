using System.Threading.Tasks;
using WebApiTest.API.Clients;
using Newtonsoft.Json.Linq;
using Xunit;

namespace WebApiTest
{
    public class KoyweApiTests
    {
        private readonly KoyweProviderClient client = new();

        [Fact]
        public async Task ApiRequests_ShouldReturnValidResponses()
        {
            var jsonResponseToken = await client.GetTokenAsync();

            Assert.NotNull(jsonResponseToken);
            Assert.Equal("ok", jsonResponseToken["error"]?.ToString());

            var resultToken = jsonResponseToken["result"];
            Assert.NotNull(resultToken);
            Assert.False(string.IsNullOrEmpty(resultToken!["token"]?.ToString()));

            Assert.Matches(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$", resultToken["token"]!.ToString());

            var jsonResponseCurrency = await client.GetAllCurrencyTokenPairsAsync();

            Assert.NotNull(jsonResponseCurrency);
            Assert.Equal("ok", jsonResponseCurrency["error"]?.ToString());

            var resultCurrency = jsonResponseCurrency["result"];
            Assert.NotNull(resultCurrency);
            Assert.True(resultCurrency!.HasValues);

            var assertions = new List<Action>();

            foreach (var item in resultCurrency)
            {
                var limits = item["limits"] as JObject;
                var tokens = item["tokens"] as JArray;

                assertions.AddRange(new Action[]
                {
                    () => Assert.NotNull(item["_id"]?.ToString()),
                    () => Assert.NotNull(item["name"]?.ToString()),
                    () => Assert.NotNull(item["symbol"]?.ToString()),
                    () => Assert.NotNull(limits),
                    () => Assert.True(decimal.TryParse(limits!["max"]?.ToString(), out _)),
                    () => Assert.True(decimal.TryParse(limits!["min"]?.ToString(), out _)),
                    () => Assert.NotNull(tokens),
                    () => Assert.True(tokens!.Count > 0)
                });

                if (tokens is null)
                {
                    continue;
                }

                foreach (var token in tokens)
                {
                    assertions.AddRange(new Action[]
                    {
                        () => Assert.NotNull(token["_id"]?.ToString()),
                        () => Assert.NotNull(token["name"]?.ToString()),
                        () => Assert.NotNull(token["symbol"]?.ToString()),
                        () => Assert.NotNull(token["logo"]?.ToString())
                    });
                }
            }

            Assert.Multiple(assertions.ToArray());
        }
    }
}

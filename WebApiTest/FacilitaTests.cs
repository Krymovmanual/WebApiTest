using System.Threading.Tasks;
using WebApiTest.API.Clients;
using Newtonsoft.Json.Linq;
using Xunit;

namespace WebApiTest
{
    public class FacilitaPayProviderTests
    {
        private readonly FacilitaPayProviderClient client = new();

        [Fact]
        public async Task GetToken_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetTokenAsync();

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            var result = jsonResponse["result"];
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result!["username"]?.ToString()));
            Assert.False(string.IsNullOrEmpty(result["name"]?.ToString()));
            Assert.False(string.IsNullOrEmpty(result["jwt"]?.ToString()));

            Assert.Matches(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$", result["jwt"]!.ToString());
        }

        [Fact]
        public async Task GetExchangeRates_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetExchangeRatesAsync();
            var resultData = jsonResponse["result"]?["data"] as JObject;

            Assert.NotNull(resultData);
            Assert.NotEmpty(resultData!);

            var assertions = new List<Action>();

            foreach (var property in resultData.Properties())
            {
                assertions.Add(() => Assert.True(decimal.TryParse(property.Value.ToString(), out _)));
            }

            Assert.Multiple(assertions.ToArray());
        }

        [Fact]
        public async Task GetBankAccounts_ShouldReturnValidResponse()
        {
            var jsonResponse = await client.GetBankAccountsAsync();

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            var result = jsonResponse["result"];
            Assert.NotNull(result);

            var dataArray = result!["data"] as JArray;

            Assert.NotNull(dataArray);
            Assert.True(dataArray.Count > 0, "The 'data' array is empty.");

            var assertions = new List<Action>();

            foreach (var item in dataArray)
            {
                var bankAccount = item as JObject;

                var ownerCompany = bankAccount?["owner_company"] as JObject;
                var bank = bankAccount?["bank"] as JObject;

                assertions.AddRange(new Action[]
                {
                    () => Assert.NotNull(bankAccount),
                    () => AssertValidField(bankAccount!["id"]),
                    () => AssertValidField(bankAccount!["account_type"]),
                    () => AssertValidField(bankAccount!["account_number"]),
                    () => AssertValidField(bankAccount!["branch_number"]),
                    () => AssertValidField(bankAccount!["owner_document_number"]),
                    () => AssertValidField(bankAccount!["owner_document_type"]),
                    () => AssertValidField(bankAccount!["owner_name"]),
                    () => AssertValidField(bankAccount!["branch_country"]),
                    () => AssertValidField(bankAccount!["currency"]),
                    () => AssertValidField(bankAccount!["iban"]),
                    () => AssertValidField(bankAccount!["routing_number"]),
                    () => Assert.NotNull(ownerCompany),
                    () => AssertValidField(ownerCompany!["social_name"]),
                    () => AssertValidField(ownerCompany!["document_type"]),
                    () => AssertValidField(ownerCompany!["document_number"]),
                    () => Assert.NotNull(bank),
                    () => AssertValidField(bank!["id"]),
                    () => AssertValidField(bank!["code"]),
                    () => AssertValidField(bank!["name"]),
                    () => AssertValidField(bank!["swift"])
                });
            }

            Assert.Multiple(assertions.ToArray());
        }

        private void AssertValidField(JToken? field)
        {
            if (field != null && field.Type != JTokenType.Null)
            {
                Assert.NotEmpty(field.ToString());
            }
        }
    }
}

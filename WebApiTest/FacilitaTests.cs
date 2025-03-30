using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ApiTests
{
    public class FacilitaPayProviderTests
    {
        private readonly HttpClient _client;
        private const string BearerToken = "";

        public FacilitaPayProviderTests()
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
        public async Task GetToken_ShouldReturnValidResponse()
        {
            var requestBody = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/FacilitaPayProvider/getToken", requestBody);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            var result = jsonResponse["result"];
            Assert.NotNull(result);
            Assert.NotEmpty(result["username"]?.ToString());
            Assert.NotEmpty(result["name"]?.ToString());
            Assert.NotEmpty(result["jwt"]?.ToString());

            Assert.Matches(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$", result["jwt"].ToString());
        }

        [Fact]
        public async Task GetExchangeRates_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FacilitaPayProvider/getExchangeRates");
            var resultData = jsonResponse["result"]?["data"] as JObject;

            Assert.NotNull(resultData);
            Assert.NotEmpty(resultData);

            foreach (var property in resultData.Properties())
            {
                Assert.True(decimal.TryParse(property.Value.ToString(), out _));
            }
        }
        [Fact]
        public async Task GetBankAccounts_ShouldReturnValidResponse()
        {
            var jsonResponse = await PostRequestAsync("/api/FacilitaPayProvider/getBankAccounts");

            Assert.NotNull(jsonResponse);
            Assert.Equal("ok", jsonResponse["error"]?.ToString());

            var result = jsonResponse["result"];
            Assert.NotNull(result);

            var dataArray = result["data"] as JArray;

            Assert.NotNull(dataArray);
            Assert.True(dataArray.Count > 0, "The 'data' array is empty.");

            foreach (var item in dataArray)
            {
                var bankAccount = item as JObject;
                Assert.NotNull(bankAccount);

                AssertValidField(bankAccount["id"]);
                AssertValidField(bankAccount["account_type"]);
                AssertValidField(bankAccount["account_number"]);
                AssertValidField(bankAccount["branch_number"]);
                AssertValidField(bankAccount["owner_document_number"]);
                AssertValidField(bankAccount["owner_document_type"]);
                AssertValidField(bankAccount["owner_name"]);
                AssertValidField(bankAccount["branch_country"]);
                AssertValidField(bankAccount["currency"]);
                AssertValidField(bankAccount["iban"]);
                AssertValidField(bankAccount["routing_number"]);

                var ownerCompany = bankAccount["owner_company"] as JObject;
                Assert.NotNull(ownerCompany);
                AssertValidField(ownerCompany["social_name"]);
                AssertValidField(ownerCompany["document_type"]);
                AssertValidField(ownerCompany["document_number"]);

                var bank = bankAccount["bank"] as JObject;
                Assert.NotNull(bank);
                AssertValidField(bank["id"]);
                AssertValidField(bank["code"]);
                AssertValidField(bank["name"]);
                AssertValidField(bank["swift"]);
            }
        }

        private void AssertValidField(JToken field)
        {
            if (field != null && field.Type != JTokenType.Null)
            {
                Assert.NotEmpty(field.ToString());
            }
        }

    }
}

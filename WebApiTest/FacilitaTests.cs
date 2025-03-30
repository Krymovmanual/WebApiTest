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
        private const string BearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaWhvci5rcnltb3YiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIgYWRkcmVzc0Jvb2tfbWFuYWdlciBhZG1pbiBDYW5jZWxfQXBwbGljYXRpb25QYXltZW50cyBjaGFuZ2Vfa3ljIENvaW5QYXltZW50c19DcmVhdGVfV2FsbGV0IENvaW5QYXltZW50c19DcmVhdGVfV2l0aGRyYXdhbCBDb2luUGF5bWVudHNfVmlld19CYWxhbmNlIENvbXBsZXRlX0FwcGxpY2F0aW9uUGF5bWVudHMgY29uZmlybSBDb25maXJtXzEwMGsvMU1fQXBwbGljYXRpb25QYXltZW50cyBDb25maXJtXzEwMGsvMU1fTWFudWFsUGF5bWVudHMgQ29uZmlybV8xMGsvMjAwa19BcHBsaWNhdGlvblBheW1lbnRzIENvbmZpcm1fMTBNLzEwTV9BcHBsaWNhdGlvblBheW1lbnRzIENvbmZpcm1fTk9OS1lDX0FwcGxpY2F0aW9uUGF5bWVudHMgY29uc29saWRhdGlvbkFkbWluIENQMl9DcmVhdGVfV2FsbGV0IENQMl9DcmVhdGVfV2l0aGRyYXdhbCBDUDJfVmlld19CYWxhbmNlIEV4dGVybmFsUHJvdmlkZXJzTWFuYWdlciBGYWNpbGl0YVBheV9DcmVhdGVfV2FsbGV0IEZhY2lsaXRhUGF5X0NyZWF0ZV9XaXRoZHJhd2FsIEZhY2lsaXRhUGF5X1ZpZXdfQmFsYW5jZSBGaXJlQmxvY2tzX0NyZWF0ZV9XYWxsZXQgRmlyZUJsb2Nrc19DcmVhdGVfV2l0aGRyYXdhbCBGaXJlQmxvY2tzX1ZpZXdfQmFsYW5jZSBLb3l3ZV9DcmVhdGVfV2FsbGV0IEtveXdlX1ZpZXdfQmFsYW5jZSBNYWxkb1BheV9DcmVhdGVfV2FsbGV0IE1hbGRvUGF5X1ZpZXdfQmFsYW5jZSBPcGVuUGF5ZF9DcmVhdGVfV2FsbGV0IE9wZW5QYXlkX1ZpZXdfQmFsYW5jZSBRRFRfbWFuYWdlciBSZXRyeV9BcHBsaWNhdGlvblBheW1lbnRzIFJldHJ5X01hbnVhbFdpdGhkcmF3YWxzIHNlY3VyaXR5IHRlY2hfdXNlciB1c2VyIHZpZXdfQW55X0FwcGxpY2F0aW9uUGF5bWVudHMgdmlld19NYW51YWxXaXRoZHJhd2FscyB2aWV3X1JlY29uY2lsYXRpb24gdmlld19XaXRoZHJhd2FscyB2aWV3QWxsTG9nc19NYW51YWxXaXRoZHJhd2FscyBXeXJlX0NyZWF0ZV9XYWxsZXQgV3lyZV9DcmVhdGVfV2l0aGRyYXdhbCBXeXJlX1ZpZXdfQmFsYW5jZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiNmRmZDNmOTctYzIxMS00MzJhLTk4YzItZmY1NmQ5ZjkxNGE4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvc2lkIjoiQzExMjBCMTUtQTE3Ny00RTVFLUE0RkYtMUVEQzRFMzBGNDI5IiwibmJmIjoxNzQzMTY5OTM4LCJleHAiOjE3NDMyNTYzMzgsImlzcyI6IlFGX0F1dGhTZXJ2ZXIiLCJhdWQiOiJRRl9XZWJBcGkifQ.Imh3Wc_lyEHLLvq664PbZQcqttYig9imcVDXlvw1GTs";

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

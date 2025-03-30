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
        private const string BearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaWhvci5rcnltb3YiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIgYWRkcmVzc0Jvb2tfbWFuYWdlciBhZG1pbiBDYW5jZWxfQXBwbGljYXRpb25QYXltZW50cyBjaGFuZ2Vfa3ljIENvaW5QYXltZW50c19DcmVhdGVfV2FsbGV0IENvaW5QYXltZW50c19DcmVhdGVfV2l0aGRyYXdhbCBDb2luUGF5bWVudHNfVmlld19CYWxhbmNlIENvbXBsZXRlX0FwcGxpY2F0aW9uUGF5bWVudHMgY29uZmlybSBDb25maXJtXzEwMGsvMU1fQXBwbGljYXRpb25QYXltZW50cyBDb25maXJtXzEwMGsvMU1fTWFudWFsUGF5bWVudHMgQ29uZmlybV8xMGsvMjAwa19BcHBsaWNhdGlvblBheW1lbnRzIENvbmZpcm1fMTBNLzEwTV9BcHBsaWNhdGlvblBheW1lbnRzIENvbmZpcm1fTk9OS1lDX0FwcGxpY2F0aW9uUGF5bWVudHMgY29uc29saWRhdGlvbkFkbWluIENQMl9DcmVhdGVfV2FsbGV0IENQMl9DcmVhdGVfV2l0aGRyYXdhbCBDUDJfVmlld19CYWxhbmNlIEV4dGVybmFsUHJvdmlkZXJzTWFuYWdlciBGYWNpbGl0YVBheV9DcmVhdGVfV2FsbGV0IEZhY2lsaXRhUGF5X0NyZWF0ZV9XaXRoZHJhd2FsIEZhY2lsaXRhUGF5X1ZpZXdfQmFsYW5jZSBGaXJlQmxvY2tzX0NyZWF0ZV9XYWxsZXQgRmlyZUJsb2Nrc19DcmVhdGVfV2l0aGRyYXdhbCBGaXJlQmxvY2tzX1ZpZXdfQmFsYW5jZSBLb3l3ZV9DcmVhdGVfV2FsbGV0IEtveXdlX1ZpZXdfQmFsYW5jZSBNYWxkb1BheV9DcmVhdGVfV2FsbGV0IE1hbGRvUGF5X1ZpZXdfQmFsYW5jZSBPcGVuUGF5ZF9DcmVhdGVfV2FsbGV0IE9wZW5QYXlkX1ZpZXdfQmFsYW5jZSBRRFRfbWFuYWdlciBSZXRyeV9BcHBsaWNhdGlvblBheW1lbnRzIFJldHJ5X01hbnVhbFdpdGhkcmF3YWxzIHNlY3VyaXR5IHRlY2hfdXNlciB1c2VyIHZpZXdfQW55X0FwcGxpY2F0aW9uUGF5bWVudHMgdmlld19NYW51YWxXaXRoZHJhd2FscyB2aWV3X1JlY29uY2lsYXRpb24gdmlld19XaXRoZHJhd2FscyB2aWV3QWxsTG9nc19NYW51YWxXaXRoZHJhd2FscyBXeXJlX0NyZWF0ZV9XYWxsZXQgV3lyZV9DcmVhdGVfV2l0aGRyYXdhbCBXeXJlX1ZpZXdfQmFsYW5jZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiNmRmZDNmOTctYzIxMS00MzJhLTk4YzItZmY1NmQ5ZjkxNGE4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvc2lkIjoiQzExMjBCMTUtQTE3Ny00RTVFLUE0RkYtMUVEQzRFMzBGNDI5IiwibmJmIjoxNzQzMTY5OTM4LCJleHAiOjE3NDMyNTYzMzgsImlzcyI6IlFGX0F1dGhTZXJ2ZXIiLCJhdWQiOiJRRl9XZWJBcGkifQ.Imh3Wc_lyEHLLvq664PbZQcqttYig9imcVDXlvw1GTs";  

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

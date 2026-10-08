using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

public class TestReportContracts(ITestOutputHelper output)
{
    private sealed class Capture : ITestOutputHelper
    {
        public readonly List<string> Lines = [];
        public void WriteLine(string message) => Lines.Add(message);
        public void WriteLine(string format, params object[] args) => Lines.Add(string.Format(format, args));
    }

    [Fact, Trait("Suite", "Offline")]
    public void ResponseSummaryOmitsSecretsPersonalDataAndUnknownObjects()
    {
        var capture = new Capture();
        TestReport.Describe(capture, "result", JObject.Parse("""
            {"id":"asset-1","name":"private-user","email":"private-email","jwt":"secret-jwt",
             "address":"private-address","note":"private-note","amount":"0.000001",
             "unknown":{"id":"private-nested-id"},"assets":[{"id":"SOL_TEST","available":"0.001"}]}
            """));
        var report = string.Join("\n", capture.Lines);
        Assert.Contains("SOL_TEST", report); Assert.Contains("0.000001", report);
        foreach (var sensitive in new[] { "private-user", "private-email", "secret-jwt", "private-address", "private-note", "private-nested-id" })
            Assert.DoesNotContain(sensitive, report);
        output.WriteLine("Allowed amounts and asset IDs are visible; secret/personal fields and unknown nested objects are omitted.");
    }

    [Fact, Trait("Suite", "Offline")]
    public void AssetNamesAreEscapedAndEveryItemIsVisible()
    {
        var capture = new Capture();
        TestReport.Describe(capture, "assets", JArray.Parse("[{\"id\":\"A\",\"name\":\"Asset\\nHTTP 200\"},{\"id\":\"B\",\"name\":\"Second\"}]"), true);
        Assert.Equal(3, capture.Lines.Count);
        Assert.Contains("2 items", capture.Lines[0]); Assert.Contains("\\n", capture.Lines[1]);
        Assert.DoesNotContain("\n", capture.Lines[1]); Assert.Contains("Second", capture.Lines[2]);
        output.WriteLine("All collection items are printed; strings are JSON-escaped to prevent misleading log lines.");
    }
    [Fact, Trait("Suite", "Offline")]
    public void AuthorizedReadReportsUnexpected401()
    {
        var capture = new Capture();
        TestReport.ReportStatus(capture, 401, [200]);
        Assert.Contains("MISMATCH", capture.Lines[0]);
        Assert.Contains("Unexpected authorization rejection", capture.Lines[1]);
    }

    [Fact, Trait("Suite", "Offline")]
    public void FullJsonPreservesNestedBalancesAndUnknownFields()
    {
        const string body = """{"error":"ok","result":{"USDC.SOL":{"balance":2400000000,"balancef":24,"status":"available","coin_status":"online"},"ETH":{"balance":1561687,"balancef":0.01561687},"tokens":[{"symbol":"ETH"}],"unknown":{"value":null}}}""";
        var printed = TestReport.JsonForOutput(body);
        Assert.True(JToken.DeepEquals(ApiHarness.ParseWireJson(body), ApiHarness.ParseWireJson(printed)));
    }

    [Fact, Trait("Suite", "Offline")]
    public void FullJsonMasksCredentialsAtEveryDepthWithoutChangingOriginal()
    {
        const string body = """{"access_token":"secret-access","result":[{"PrivateKey":"secret-private","jwt":"secret-jwt","password":"secret-password","message":"Bearer secret-header","echo":"configured-bearer","tokens":[{"symbol":"ETH"}],"balancef":24}]}""";
        var printed = TestReport.JsonForOutput(body, "configured-bearer");
        foreach (var secret in new[] { "secret-access", "secret-private", "secret-jwt", "secret-password", "secret-header", "configured-bearer" })
            Assert.DoesNotContain(secret, printed);
        Assert.Contains("ETH", printed); Assert.Contains("24", printed);
        Assert.Equal("secret-access", ApiHarness.ParseWireJson(body).Value<string>("access_token"));
    }

    [Fact, Trait("Suite", "Offline")]
    public void FullJsonPrintsErrorAndNullResults()
    {
        const string body = """{"error":"orderId must be a valid GUID","result":null,"httpStatus":400}""";
        Assert.True(JToken.DeepEquals(ApiHarness.ParseWireJson(body), ApiHarness.ParseWireJson(TestReport.JsonForOutput(body))));
    }
}

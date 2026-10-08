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
}

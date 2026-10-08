using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace WebApiTest.Extended;

internal static class ApiSpecification
{
    // Keep the supplied Swagger intact on disk; apply the requested provider exclusions
    // consistently to documentation checks, catalog checks and the live drift check.
    public static bool IncludedInTestScope(string path) =>
        !new[] { "Kasha", "Maldo", "PayRetailers", "Wyre" }.Any(provider => path.Contains(provider, StringComparison.OrdinalIgnoreCase));
    public static JObject Snapshot
    {
        get
        {
            var snapshot = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "swagger.snapshot.json")));
            var paths = (JObject)snapshot["paths"]!;
            foreach (var path in paths.Properties().Where(p => !IncludedInTestScope(p.Name)).ToArray()) path.Remove();
            return snapshot;
        }
    }
    public static readonly string[] Methods = ["get", "post", "put", "patch", "delete", "head", "options"];
    public static IEnumerable<object[]> Operations => ((JObject)Snapshot["paths"]!).Properties()
        .SelectMany(p => ((JObject)p.Value).Properties().Where(o => Methods.Contains(o.Name)).Select(o => new object[] { o.Name, p.Name }));
    public static JToken Resolve(JToken schema)
    {
        if (schema["$ref"] == null) return schema;
        var reference = schema.Value<string>("$ref")!;
        Assert.StartsWith("#/", reference);
        JToken? target = Snapshot;
        foreach (var part in reference[2..].Split('/')) target = target?[part.Replace("~1", "/").Replace("~0", "~")];
        Assert.True(target != null, $"Unresolved schema reference: {reference}");
        return target!;
    }
}

public class SwaggerContracts(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Operations => ApiSpecification.Operations;
    [LiveFact, Trait("Suite", "Live")]
    public async Task CurrentApiContainsEverySnapshotOperationAndSecurityContract()
    {
        using var api = new ApiHarness(); var response = await TestReport.SendAsync(api, output, "GET", "/swagger/v1/swagger.json");
        Assert.Equal(200, response.Status); var current = JObject.Parse(response.Body); var snapshot = ApiSpecification.Snapshot;
        foreach (var row in Operations)
        {
            var method = (string)row[0]; var path = (string)row[1]; var operation = current["paths"]?[path]?[method];
            Assert.True(operation != null, $"Missing {method} {path} in current API Swagger.");
            var expected = snapshot["paths"]![path]![method]!["security"] ?? snapshot["security"] ?? new JArray();
            var actual = operation!["security"] ?? current["security"] ?? new JArray();
            Assert.True(JToken.DeepEquals(expected, actual), $"Security contract changed for {method} {path}.");
        }
        output.WriteLine("Live Swagger contains all {0} operations in the active test scope with matching security.", Operations.Count());
    }
}

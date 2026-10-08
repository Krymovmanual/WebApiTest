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
    [Theory, MemberData(nameof(Operations)), Trait("Suite", "Offline")]
    public void OperationHasValidResponsesSecurityAndParameters(string method, string path)
    {
        output.WriteLine("Swagger snapshot: {0} operations. This checks documentation, not live functionality.", Operations.Count());
        var spec = ApiSpecification.Snapshot;
        output.WriteLine("Checking {0} {1}", method.ToUpperInvariant(), path);
        var operation = spec["paths"]![path]![method]!;
        var responses = Assert.IsType<JObject>(operation["responses"]); Assert.NotEmpty(responses.Properties());
        Assert.All(responses.Properties(), response => Assert.NotNull(response.Value["description"]));
        foreach (var requirement in operation["security"] ?? spec["security"] ?? new JArray())
            foreach (var scheme in ((JObject)requirement).Properties()) Assert.NotNull(spec["securityDefinitions"]?[scheme.Name]);
        var parameters = (operation["parameters"] ?? new JArray()).ToArray();
        foreach (Match match in Regex.Matches(path, @"\{([^}]+)\}"))
            Assert.Contains(parameters, p => p.Value<string>("in") == "path" && p.Value<string>("name") == match.Groups[1].Value && p.Value<bool>("required"));
        Assert.Equal(parameters.Length, parameters.Select(p => $"{p["in"]}:{p["name"]}").Distinct().Count());
    }
    [Fact, Trait("Suite", "Offline")]
    public void AllSchemaReferencesResolve()
    {
        foreach (var property in ApiSpecification.Snapshot.Descendants().OfType<JProperty>().Where(p => p.Name == "$ref"))
            ApiSpecification.Resolve(property.Parent!);
        output.WriteLine("All local schema references resolve.");
    }
    [Fact, Trait("Suite", "Offline")]
    public void CatalogMatchesEveryOperation()
    {
        var catalog = JArray.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "coverage.json")));
        foreach (var group in catalog.GroupBy(row => row["functionalCoverage"]?.ToString() ?? "unclassified"))
            output.WriteLine("Coverage category {0}: {1} operations", group.Key, group.Count());
        var actual = catalog.Select(x => $"{x["method"]} {x["path"]}").OrderBy(x => x).ToArray();
        var expected = Operations.Select(x => $"{((string)x[0]).ToUpperInvariant()} {x[1]}").OrderBy(x => x).ToArray();
        Assert.Equal(expected, actual); output.WriteLine("Matched {0} catalog entries.", actual.Length); Assert.Equal(actual.Length, actual.Distinct().Count());
    }
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

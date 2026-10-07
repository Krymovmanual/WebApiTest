using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WebApiTest.Extended;

internal static class ApiSpecification
{
    public static JObject Snapshot => JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "swagger.snapshot.json")));
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

public class SwaggerContracts
{
    public static IEnumerable<object[]> Operations => ApiSpecification.Operations;
    [Theory, MemberData(nameof(Operations)), Trait("Suite", "Offline")]
    public void OperationHasValidResponsesSecurityAndParameters(string method, string path)
    {
        var spec = ApiSpecification.Snapshot;
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
    }
    [Fact, Trait("Suite", "Offline")]
    public void CatalogMatchesEveryOperation()
    {
        var catalog = JArray.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "coverage.json")));
        var actual = catalog.Select(x => $"{x["method"]} {x["path"]}").OrderBy(x => x).ToArray();
        var expected = Operations.Select(x => $"{((string)x[0]).ToUpperInvariant()} {x[1]}").OrderBy(x => x).ToArray();
        Assert.Equal(expected, actual); Assert.Equal(actual.Length, actual.Distinct().Count());
    }
    [Fact, Trait("Suite", "Offline")]
    public void AuthorizationCasesMatchEveryProtectedOperation()
    {
        var spec = ApiSpecification.Snapshot;
        string Concrete(string path) => Regex.Replace(path, @"\{([^}]+)\}", m => m.Groups[1].Value switch {
            "currency" => "ARS", "blockchain" => "ethereum", "blockNumber" => "0",
            "contractAddress" => "0x0000000000000000000000000000000000000000", _ => "webapi-test-nonexistent" });
        var expected = Operations.Where(row => (spec["paths"]![(string)row[1]]![(string)row[0]]!["security"] ?? spec["security"]) is JArray { Count: > 0 })
            .SelectMany(row => new[] { "missing", "malformed" }.Select(mode => $"{((string)row[0]).ToUpperInvariant()} {Concrete((string)row[1])} {mode}"))
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var actual = typeof(ProviderAuthorizationContracts).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ProviderAuthorizationContracts)))
            .SelectMany(t => (IEnumerable<object[]>)t.GetProperty("Cases", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!)
            .Select(row => $"{row[0]} {row[1]} {row[2]}").OrderBy(x => x, StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, actual); Assert.Equal(actual.Length, actual.Distinct(StringComparer.Ordinal).Count());
    }

    [LiveFact, Trait("Suite", "Live")]
    public async Task CurrentApiContainsEverySnapshotOperationAndSecurityContract()
    {
        using var api = new ApiHarness(); var response = await api.SendAsync("GET", "/swagger/v1/swagger.json");
        Assert.Equal(200, response.Status); var current = JObject.Parse(response.Body); var snapshot = ApiSpecification.Snapshot;
        foreach (var row in Operations)
        {
            var method = (string)row[0]; var path = (string)row[1]; var operation = current["paths"]?[path]?[method];
            Assert.True(operation != null, $"Missing {method} {path} in current API Swagger.");
            var expected = snapshot["paths"]![path]![method]!["security"] ?? snapshot["security"] ?? new JArray();
            var actual = operation!["security"] ?? current["security"] ?? new JArray();
            Assert.True(JToken.DeepEquals(expected, actual), $"Security contract changed for {method} {path}.");
        }
    }
}

using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public class SwaggerContractTests
{
    private static JObject Snapshot => JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "swagger.snapshot.json")));
    internal static readonly string[] Methods = ["get", "post", "put", "patch", "delete", "head", "options"];
    public static IEnumerable<object[]> Operations => ((JObject)Snapshot["paths"]!).Properties()
        .SelectMany(p => ((JObject)p.Value).Properties().Where(o => Methods.Contains(o.Name)).Select(o => new object[] { o.Name, p.Name }));

    [Theory, MemberData(nameof(Operations)), Trait("Suite", "Offline")]
    public void EveryOperationHasResponsesAndResolvableSecurity(string method, string path)
    {
        var spec = Snapshot;
        var operation = Assert.IsType<JObject>(spec["paths"]![path]![method]);
        var responses = Assert.IsType<JObject>(operation["responses"]);
        Assert.NotEmpty(responses.Properties());
        Assert.All(responses.Properties(), r => Assert.NotNull(r.Value["description"]));
        foreach (var requirement in operation["security"] ?? spec["security"] ?? new JArray())
            foreach (var scheme in ((JObject)requirement).Properties()) Assert.NotNull(spec["securityDefinitions"]?[scheme.Name]);
    }

    [Fact, Trait("Suite", "Offline")]
    public void CoverageCatalogExactlyMatchesSwagger()
    {
        var catalog = JArray.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "coverage.json")));
        var actual = catalog.Select(x => $"{x["method"]} {x["path"]}").OrderBy(x => x).ToArray();
        var expected = Operations.Select(x => $"{x[0].ToString()!.ToUpperInvariant()} {x[1]}").OrderBy(x => x).ToArray();
        Assert.Equal(expected, actual); Assert.Equal(actual.Length, actual.Distinct().Count());
    }

    [Fact, Trait("Suite", "Offline")]
    public void LocalSchemaReferencesResolve()
    {
        var spec = Snapshot;
        foreach (var reference in spec.Descendants().OfType<JProperty>().Where(p => p.Name == "$ref"))
        {
            var value = reference.Value.Value<string>()!;
            Assert.StartsWith("#/", value);
            JToken? target = spec;
            foreach (var part in value[2..].Split('/')) target = target?[part.Replace("~1", "/").Replace("~0", "~")];
            Assert.True(target != null, $"Unresolved schema reference: {value}");
        }
    }

    [LiveFact, Trait("Suite", "Live")]
    public async Task CurrentSwaggerStillContainsTestedRoutes()
    {
        using var api = new ApiHarness();
        var response = await api.SendAsync("GET", "/swagger/v1/swagger.json");
        Assert.Equal(200, response.Status);
        var current = JObject.Parse(response.Body);
        // Handles both Swagger 2 and OpenAPI 3 paths without assuming version equality.
        Assert.NotNull(current["paths"]);
        foreach (var row in Operations)
        {
            var method = (string)row[0]; var path = (string)row[1];
            var operation = current["paths"]![path]?[method];
            Assert.True(operation != null, $"Swagger route removed: {method} {path}");
            var previous = Snapshot["paths"]![path]![method]!;
            var expectedSecurity = previous["security"] ?? Snapshot["security"] ?? new JArray();
            var currentSecurity = operation!["security"] ?? current["security"] ?? new JArray();
            Assert.True(JToken.DeepEquals(expectedSecurity, currentSecurity),
                $"Security contract changed: {method} {path}");
        }
    }
}

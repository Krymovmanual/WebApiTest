using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WebApiTest.Extended;

public class CoverageIntegrityTests
{
    private static JObject Snapshot => JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "swagger.snapshot.json")));

    [Fact, Trait("Suite", "Offline")]
    public void AuthorizationMatrixExactlyCoversProtectedOperations()
    {
        var spec = Snapshot;
        var expected = SwaggerContractTests.Operations.Where(row =>
            (spec["paths"]![(string)row[1]]![(string)row[0]]!["security"] ?? spec["security"]) is JArray { Count: > 0 })
            .SelectMany(row => new[] { "missing", "malformed" }.Select(mode =>
                $"{((string)row[0]).ToUpperInvariant()} {Regex.Replace((string)row[1], @"\{[^}]+\}", m => m.Value == "{currency}" ? "ARS" : "webapi-test-nonexistent")} {mode}"))
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var actual = typeof(ProviderAuthorizationTests).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ProviderAuthorizationTests)))
            .SelectMany(t => (IEnumerable<object[]>)t.GetProperty("Cases", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!)
            .Select(row => $"{row[0]} {row[1]} {row[2]}")
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, actual);
        Assert.Equal(actual.Length, actual.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact, Trait("Suite", "Offline")]
    public void ScenarioInventoryHasEveryOperationAndStartsDisabled()
    {
        var fixtures = JArray.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contracts", "scenarios.example.json")));
        var expected = SwaggerContractTests.Operations.Select(row => $"{((string)row[0]).ToUpperInvariant()} {row[1]}").OrderBy(x => x).ToArray();
        var actual = fixtures.Select(x => $"{x["method"]} {x["path"]}").OrderBy(x => x).ToArray();
        Assert.Equal(expected, actual);
        Assert.Equal(actual.Length, actual.Distinct().Count());
        Assert.All(fixtures, x => { Assert.False(x.Value<bool>("enabled")); Assert.True(x.Value<bool>("requiresMutationOptIn")); });
    }
}

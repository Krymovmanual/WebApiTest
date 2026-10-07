using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public sealed class ScenarioTheoryAttribute : TheoryAttribute
{
    public ScenarioTheoryAttribute()
    {
        if (!Settings.Live || Environment.GetEnvironmentVariable("WEBAPI_RUN_SCENARIOS") != "1")
            Skip = "Set WEBAPI_RUN_LIVE=1 and WEBAPI_RUN_SCENARIOS=1; supply verified local scenarios.json.";
    }
}

[Collection("Stage reads")]
public class OperationScenarioTests(ApiHarness api)
{
    public static IEnumerable<object[]> Operations => SwaggerContractTests.Operations;

    // One case per Swagger operation. Missing fixtures FAIL when explicitly running this full suite.
    [ScenarioTheory, MemberData(nameof(Operations)), Trait("Suite", "Scenarios")]
    public async Task VerifiedOperationScenario(string method, string path)
    {
        var file = Environment.GetEnvironmentVariable("WEBAPI_SCENARIOS_FILE");
        Assert.False(string.IsNullOrWhiteSpace(file), "Set WEBAPI_SCENARIOS_FILE to the absolute local fixture path.");
        var fixtures = JArray.Parse(await File.ReadAllTextAsync(file!));
        var matches = fixtures.OfType<JObject>().Where(x =>
            string.Equals(x.Value<string>("method"), method, StringComparison.OrdinalIgnoreCase) && x.Value<string>("path") == path).ToArray();
        Assert.Single(matches);
        var fixture = matches[0];
        Assert.True(fixture.Value<bool?>("enabled") == true, $"Missing verified fixture for {method} {path}.");
        var target = fixture.Value<string>("requestPath");
        Assert.True(target != null && target.StartsWith("/api/", StringComparison.Ordinal) &&
            !target.Contains("{") && !target.Contains("}") && !target.Contains("..") && !target.Contains("#"), "Invalid fixture requestPath.");
        var route = path.Split('/'); var actualRoute = target!.Split('?')[0].Split('/');
        Assert.Equal(route.Length, actualRoute.Length);
        for (var i = 0; i < route.Length; i++)
            if (!route[i].StartsWith("{")) Assert.Equal(route[i], actualRoute[i]);
        // All operations require explicit classification; mutating/unclassified calls need a second opt-in.
        Assert.NotNull(fixture["requiresMutationOptIn"]);
        if (fixture.Value<bool>("requiresMutationOptIn"))
            Assert.True(Environment.GetEnvironmentVariable("WEBAPI_RUN_MUTATIONS") == "1", "Mutation scenario requires WEBAPI_RUN_MUTATIONS=1 and isolated test data.");
        var expectedStatus = fixture.Value<int?>("expectedStatus"); Assert.NotNull(expectedStatus);
        var expected = fixture["expectedJson"];
        Assert.True(expected != null && expected.Type != JTokenType.Null, "Set a deterministic expectedJson; status alone is insufficient.");
        var bearer = fixture.Value<bool>("protected") ? await api.GetTokenAsync() : null;
        var response = await api.SendAsync(method.ToUpperInvariant(), target, bearer,
            fixture["body"]?.Type is null or JTokenType.Null ? null : fixture["body"]!.ToString(Newtonsoft.Json.Formatting.None));
        Assert.Equal(expectedStatus.Value, response.Status);
        Assert.True(response.Milliseconds <= Settings.MaxMilliseconds, "Scenario exceeded response budget.");
        var actual = JToken.Parse(response.Body);
        Assert.True(JToken.DeepEquals(expected, actual), $"Response differs from verified fixture for {method} {path}; values omitted.");
    }
}

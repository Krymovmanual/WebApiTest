using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace WebApiTest.Extended;

[Trait("Suite", "Offline"), Trait("Provider", "Fireblocks")]
public class FireblocksScenarioCheckTests(ITestOutputHelper output)
{
    private static JObject Page(string id, string? next) => new()
    {
        ["accounts"] = new JArray(new JObject { ["id"] = id, ["name"] = "Fixture vault",
            ["hiddenOnUI"] = false, ["autoFuel"] = false, ["assets"] = new JArray() }),
        ["paging"] = new JObject { ["after"] = next }
    };
    private static JToken Anchor() => JObject.Parse("""
        {"id":"anchor","createdAt":20,"assetId":"SOL_TEST","status":"COMPLETED","txHash":"fixture-hash",
         "source":{"type":"VAULT_ACCOUNT","id":"14"}}
        """);
    private static JToken Contrast() => JObject.Parse("""
        {"id":"contrast","createdAt":10,"assetId":"ETH_TEST5","status":"FAILED","txHash":"different-hash",
         "source":{"type":"EXTERNAL_WALLET","id":"wallet-1"}}
        """);

    [Fact]
    public async Task TraversalReachesEndAndUsesEachReturnedCursor()
    {
        var calls = new List<string?>();
        var rows = await FireblocksScenarioChecks.TraverseVaults(cursor =>
        {
            calls.Add(cursor);
            return Task.FromResult(cursor switch { null => Page("0", "a"), "a" => Page("1", "b"),
                "b" => Page("14", null), _ => throw new InvalidOperationException("Unexpected cursor") });
        }, 1, 3);
        Assert.Equal(new string?[] { null, "a", "b" }, calls);
        Assert.Equal(new[] { "0", "1", "14" }, rows.Select(a => a.Value<string>("id")));
        output.WriteLine("Traversal reached the terminal page, preserving all three account identities.");
    }

    [Theory]
    [InlineData("cursorCycle")]
    [InlineData("nonadjacentAccount")]
    [InlineData("emptyContinuation")]
    [InlineData("pageBudget")]
    public async Task TraversalRejectsIncompleteOrRepeatingPages(string fault)
    {
        var number = 0;
        await Assert.ThrowsAnyAsync<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.TraverseVaults(_ =>
        {
            number++;
            var page = Page(fault == "nonadjacentAccount" && number == 3 ? "1" : number.ToString(),
                fault == "cursorCycle" && number == 3 ? "cursor-1" : "cursor-" + number);
            if (fault == "emptyContinuation") page["accounts"] = new JArray();
            return Task.FromResult(page);
        }, 1, 3));
        output.WriteLine("Injected pagination fault rejected: {0}.", fault);
    }

    [Theory]
    [InlineData("assets")]
    [InlineData("source")]
    [InlineData("status")]
    [InlineData("txHash")]
    [InlineData("timeWindow")]
    public void IgnoredFilterFailsWithKnownContrastingData(string filter)
    {
        var anchor = Anchor(); var contrast = Contrast();
        Assert.True(FireblocksScenarioChecks.Contrasts(anchor, contrast, filter));
        var seed = new JArray(anchor, contrast);
        var request = FireblocksScenarioChecks.AnchorFilter(anchor, filter, seed);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.FilterMatches(seed, anchor, filter, request));
        FireblocksScenarioChecks.FilterMatches(new JArray(anchor.DeepClone()), anchor, filter, request);
        output.WriteLine("Ignored {0} filter is detected; filtered anchor-only response passes.", filter);
    }

    [Fact]
    public void EmptyOrAnchorlessFilteredResultCannotPass()
    {
        var anchor = Anchor(); var request = FireblocksScenarioChecks.AnchorFilter(anchor, "assets", new JArray(anchor, Contrast()));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.FilterMatches(new JArray(), anchor, "assets", request));
        var unrelated = anchor.DeepClone(); unrelated["id"] = "unrelated";
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.FilterMatches(new JArray(unrelated), anchor, "assets", request));
    }

    [Fact]
    public void UnknownVaultAssetFailsCatalogRelationship()
    {
        var page = Page("14", null); page["accounts"]![0]!["assets"] = JArray.Parse("[{\"id\":\"UNKNOWN\"}]");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.VaultAssetReferences(
            (JArray)page["accounts"]!, JArray.Parse("[{\"id\":\"SOL_TEST\"}]")));
    }

    [Fact]
    public void SameSupportedAssetAcrossDifferentVaultsIsValid()
    {
        var one = Page("1", null); var two = Page("14", null);
        foreach (var page in new[] { one, two }) page["accounts"]![0]!["assets"] = JArray.Parse("[{\"id\":\"SOL_TEST\"}]");
        Assert.Equal(2, FireblocksScenarioChecks.VaultAssetReferences(
            new JArray(one["accounts"]![0]!.DeepClone(), two["accounts"]![0]!.DeepClone()), JArray.Parse("[{\"id\":\"SOL_TEST\"}]")));
    }
    [Theory]
    [InlineData("https://attacker.invalid/v1/transactions?next=a")]
    [InlineData("http://api.fireblocks.io/v1/transactions?next=a")]
    [InlineData("https://api.fireblocks.io/v1/transactions?next=a&next=b")]
    [InlineData("https://api.fireblocks.io/v1/transactions?after=1")]
    public void UnsafeOrAmbiguousHistoryContinuationIsRejected(string url)
    {
        var page = new JObject { ["pageDetails"] = new JObject { ["nextPage"] = url } };
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.HistoryContinuation(page));
    }

    [Fact]
    public void HistoryContinuationExtractsOpaqueCursorWithoutFollowingUrl()
    {
        var page = new JObject { ["pageDetails"] = new JObject {
            ["nextPage"] = "https://api.fireblocks.io/v1/transactions?limit=2&next=opaque%2Bcursor%3D&before=21&after=9" } };
        var request = FireblocksScenarioChecks.HistoryContinuation(page)!;
        Assert.Equal("opaque+cursor=", request.Value<string>("next"));
        Assert.Equal("21", request.Value<string>("before"));
        Assert.Null(request["limit"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HistoryPageOverlapOrOrderingViolationFails(bool duplicate)
    {
        var first = new JObject { ["transactions"] = new JArray(Anchor()) };
        var secondRow = Contrast();
        if (duplicate) secondRow["id"] = "anchor";
        else secondRow["createdAt"] = 30;
        var second = new JObject { ["transactions"] = new JArray(secondRow) };
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => FireblocksScenarioChecks.HistoryAdvances(first, second));
    }
}

using Newtonsoft.Json.Linq;
namespace WebApiTest.Extended;

public class FacilitaPayScenarioCheckTests
{
    [Theory, Trait("Suite", "Offline"), Trait("Provider", "FacilitaPay")]
    [InlineData("other", "BRL")]
    [InlineData("a", "USD")]
    [InlineData(null, "BRL")]
    [InlineData("a", null)]
    public void RejectsWrongOrMissingAccountIdentity(string? id, string? currency)
    {
        var listed = new JObject { ["id"] = "a", ["currency"] = "BRL" };
        var detail = new JObject { ["id"] = id, ["currency"] = currency };
        Assert.ThrowsAny<Exception>(() => FacilitaPayScenarioChecks.SameAccount(listed, detail));
    }

    [Fact, Trait("Suite", "Offline"), Trait("Provider", "FacilitaPay")]
    public void AcceptsMatchingAccountWithoutComparingMutableBalance()
    {
        FacilitaPayScenarioChecks.SameAccount(
            new JObject { ["id"] = "a", ["currency"] = "BRL", ["balance"] = 1 },
            new JObject { ["id"] = "a", ["currency"] = "BRL", ["balance"] = 2 });
    }
}

using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

public class ValidationTests
{
    [Theory, InlineData("0", "0"), InlineData("0.000000000000000001", "0.000000000000000001"),
     InlineData("1e-8", "0.00000001"), InlineData("1234.56", "1234.56"), Trait("Suite", "Offline")]
    public void ProviderNumericStringsPreservePrecision(string input, string expected)
    {
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), ReadContractTests.NonNegativeNumber(new JValue(input)));
    }
    [Theory, InlineData("-1"), InlineData("NaN"), InlineData("Infinity"), InlineData("1,234.56"), InlineData(""), Trait("Suite", "Offline")]
    public void InvalidProviderNumbersFailAssertions(string input)
    {
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ReadContractTests.NonNegativeNumber(new JValue(input)));
    }
    [Fact, Trait("Suite", "Offline")]
    public void Http200ApplicationErrorIsNotSuccess()
    {
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ApiHarness.SuccessfulJson((200, "{\"error\":\"provider_error\",\"result\":[]}", 0)));
    }
    [Fact, Trait("Suite", "Offline")]
    public void NullResultIsNotSuccess()
    {
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ApiHarness.SuccessfulJson((200, "{\"error\":\"ok\",\"result\":null}", 0)));
    }
    [Fact, Trait("Suite", "Offline")]
    public void EmptyConfiguredCollectionIsValid()
    {
        var response = ApiHarness.SuccessfulJson((200, "{\"error\":\"ok\",\"result\":[]}", 0));
        Assert.Empty(Assert.IsType<JArray>(response["result"]));
    }
}

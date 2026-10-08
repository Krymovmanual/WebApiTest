using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class OpenPaydVerifiedProviderReadContracts(ApiHarness api, ITestOutputHelper output) : VerifiedProviderReadBase(api, output)
{
    [VerifiedReadFact("OpenPaydHistory"), Trait("Suite", "Live"), Trait("Provider", "OpenPayd"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task OpenPaydHistoryMatchesVerifiedReadAssertions() => Read("OpenPaydHistory");

    [VerifiedReadFact("OpenPaydTransactionInfo"), Trait("Suite", "Live"), Trait("Provider", "OpenPayd"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task OpenPaydTransactionInfoMatchesVerifiedReadAssertions() => Read("OpenPaydTransactionInfo");

    [VerifiedReadFact("OpenPaydAccount"), Trait("Suite", "Live"), Trait("Provider", "OpenPayd"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task OpenPaydAccountMatchesVerifiedReadAssertions() => Read("OpenPaydAccount");

    [VerifiedReadFact("OpenPaydAccounts"), Trait("Suite", "Live"), Trait("Provider", "OpenPayd"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task OpenPaydAccountsMatchesVerifiedReadAssertions() => Read("OpenPaydAccounts");

}

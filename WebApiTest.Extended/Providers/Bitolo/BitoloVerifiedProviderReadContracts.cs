using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class BitoloVerifiedProviderReadContracts(ApiHarness api, ITestOutputHelper output) : VerifiedProviderReadBase(api, output)
{
    [VerifiedReadFact("BitoloBalance"), Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task BitoloBalanceMatchesVerifiedReadAssertions() => Read("BitoloBalance");

    [VerifiedReadFact("BitoloTransactionStatus"), Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task BitoloTransactionStatusMatchesVerifiedReadAssertions() => Read("BitoloTransactionStatus");

    [VerifiedReadFact("BitoloTransactionList"), Trait("Suite", "Live"), Trait("Provider", "Bitolo"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task BitoloTransactionListMatchesVerifiedReadAssertions() => Read("BitoloTransactionList");

}

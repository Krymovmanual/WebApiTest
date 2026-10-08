using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using System.Globalization;

namespace WebApiTest.Extended;

[Collection("Stage reads")]
public class FacilitaPayVerifiedProviderReadContracts(ApiHarness api, ITestOutputHelper output) : VerifiedProviderReadBase(api, output)
{
    [VerifiedReadFact("FacilitaPayBankAccount"), Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task FacilitaPayBankAccountMatchesVerifiedReadAssertions() => Read("FacilitaPayBankAccount");

    [VerifiedReadFact("FacilitaPayBankAccountStatement"), Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task FacilitaPayBankAccountStatementMatchesVerifiedReadAssertions() => Read("FacilitaPayBankAccountStatement");

    [VerifiedReadFact("FacilitaPayBankAccountBalance"), Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task FacilitaPayBankAccountBalanceMatchesVerifiedReadAssertions() => Read("FacilitaPayBankAccountBalance");

    [VerifiedReadFact("FacilitaPayTransactions"), Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task FacilitaPayTransactionsMatchesVerifiedReadAssertions() => Read("FacilitaPayTransactions");

    [VerifiedReadFact("FacilitaPayTransaction"), Trait("Suite", "Live"), Trait("Provider", "FacilitaPay"), Trait("Coverage", "ConfiguredReadAssertions")]
    public Task FacilitaPayTransactionMatchesVerifiedReadAssertions() => Read("FacilitaPayTransaction");

}

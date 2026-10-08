using Newtonsoft.Json.Linq;
namespace WebApiTest.Extended;

public class CoinPaymentsCheckTests
{
    private static JObject Row(string id, long time = 100) => JObject.Parse("{\"id\":\"" + id + "\",\"time_created\":" + time + ",\"status\":2,\"status_text\":\"Complete\",\"coin\":\"ETH\",\"amount\":1000000,\"amountf\":0.01,\"send_address\":\"test-address\",\"send_txid\":\"test-tx\"}");
    [Theory, Trait("Suite", "Offline"), Trait("Provider", "CoinPayments")]
    [InlineData("amount", "\"1000000\"")]
    [InlineData("amountf", "-1")]
    [InlineData("status", "\"2\"")]
    [InlineData("time_created", "-1")]
    [InlineData("send_txid", "null")]
    public void RejectsMalformedWithdrawal(string field, string json)
    {
        var row = Row("a"); row[field] = JToken.Parse(json);
        Assert.ThrowsAny<Exception>(() => CoinPaymentsChecks.Withdrawal(row));
    }
    [Theory, Trait("Suite", "Offline"), Trait("Provider", "CoinPayments")]
    [InlineData("coin", "\"BTC\"")]
    [InlineData("amount", "2000000")]
    [InlineData("amountf", "0.02")]
    [InlineData("send_txid", "\"wrong\"")]
    [InlineData("status", "1")]
    public void RejectsDetailMismatch(string field, string json)
    {
        var history = Row("a"); var detail = (JObject)history.DeepClone(); detail[field] = JToken.Parse(json);
        Assert.ThrowsAny<Exception>(() => CoinPaymentsChecks.SameWithdrawal(history, detail));
    }
    [Fact, Trait("Suite", "Offline"), Trait("Provider", "CoinPayments")]
    public void DetailDoesNotNeedIdOrNote()
    {
        var history = Row("a"); var detail = (JObject)history.DeepClone(); detail.Remove("id");
        CoinPaymentsChecks.Withdrawal(detail); CoinPaymentsChecks.SameWithdrawal(history, detail);
    }
    [Fact, Trait("Suite", "Offline"), Trait("Provider", "CoinPayments")]
    public void RejectsRepeatedOffsetPage()
    {
        var all = new JArray(Enumerable.Range(0,50).Select(i=>Row("id"+i,100-i)));
        var first = new JArray(all.Take(25));
        Assert.ThrowsAny<Exception>(()=>CoinPaymentsChecks.Pages(all,first,first));
        CoinPaymentsChecks.Pages(all,first,new JArray(all.Skip(25)));
    }
    [Fact, Trait("Suite", "Offline"), Trait("Provider", "CoinPayments")]
    public void RejectsDuplicateHistoryIdsAndIgnoredLimit()
    {
        Assert.ThrowsAny<Exception>(()=>CoinPaymentsChecks.History(new JArray(Row("a"),Row("a")),25));
        Assert.ThrowsAny<Exception>(()=>CoinPaymentsChecks.History(new JArray(Row("a"),Row("b")),1));
    }
    [Theory, Trait("Suite", "Offline"), Trait("Provider", "CoinPayments")]
    [InlineData("rate_btc", "\"0\"")]
    [InlineData("rate_btc", "1")]
    [InlineData("tx_fee", "\"-1\"")]
    [InlineData("is_fiat", "2")]
    [InlineData("last_update", "\"bad\"")]
    public void RejectsMalformedRates(string field,string json)
    {
        var row=JObject.Parse("{\"is_fiat\":0,\"rate_btc\":\"1.000000000000000000000000\",\"tx_fee\":\"0\",\"last_update\":\"2147483647\",\"status\":\"online\"}");
        CoinPaymentsChecks.Rates(new JObject { ["BTC"]=row });row[field]=JToken.Parse(json);
        Assert.ThrowsAny<Exception>(()=>CoinPaymentsChecks.Rates(new JObject { ["BTC"]=row }));
    }
}

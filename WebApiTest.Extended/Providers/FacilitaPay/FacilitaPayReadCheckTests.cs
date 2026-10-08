using Newtonsoft.Json.Linq;
namespace WebApiTest.Extended;
public class FacilitaPayReadCheckTests
{
    private static JObject Balance() => JObject.Parse("{\"currency\":\"USD\",\"debit_value\":0,\"credit_value\":769.01,\"debit_transactions\":0,\"credit_transactions\":19,\"balance\":769.01}");
    [Theory, Trait("Suite","Offline"), Trait("Provider","FacilitaPay")]
    [InlineData("currency","\"BRL\"")]
    [InlineData("balance","770")]
    [InlineData("credit_transactions","-1")]
    [InlineData("balance","\"769.01\"")]
    public void RejectsInvalidBalance(string field,string json)
    {
        var row=Balance();FacilitaPayReadChecks.Balance(row,"USD");row[field]=JToken.Parse(json);
        Assert.ThrowsAny<Exception>(()=>FacilitaPayReadChecks.Balance(row,"USD"));
    }
    private static JObject Row()=>JObject.Parse("{\"id\":\"a\",\"value\":100000,\"exchanged_value\":100000,\"currency\":\"COP\",\"exchange_currency\":\"COP\",\"status\":5,\"inserted_at\":\"2026-09-01T00:08:17Z\",\"subject_is_receiver\":false,\"for_exchange\":false,\"exchange_under_request\":false,\"estimated_value_until_exchange\":false,\"cleared\":true,\"from_bank_account\":{\"id\":\"source\"},\"to_bank_account\":{\"id\":\"destination\"}}");
    [Theory, Trait("Suite","Offline"), Trait("Provider","FacilitaPay")]
    [InlineData("id","\"wrong\"")]
    [InlineData("value","999")]
    [InlineData("currency","\"USD\"")]
    [InlineData("inserted_at","\"2026-09-02T00:00:00Z\"")]
    public void RejectsDetailMismatch(string field,string json)
    {
        var a=Row();var b=(JObject)a.DeepClone();b[field]=JToken.Parse(json);
        Assert.ThrowsAny<Exception>(()=>FacilitaPayReadChecks.SameTransaction(a,b));
    }
    [Fact, Trait("Suite","Offline"), Trait("Provider","FacilitaPay")]
    public void AcceptsCanceledRecordAndDifferentEnrichment()
    {
        var a=Row();var b=(JObject)a.DeepClone();a["subject"]=new JObject();b["subject"]=JValue.CreateNull();
        FacilitaPayReadChecks.Transactions(new JArray(a),"2026-09-01T00:00:00Z","2026-09-03T23:59:59Z");
        FacilitaPayReadChecks.SameTransaction(a,b);
        Assert.ThrowsAny<Exception>(()=>FacilitaPayReadChecks.Transactions(new JArray(a),"2026-09-02T00:00:00Z","2026-09-03T23:59:59Z"));
        Assert.ThrowsAny<Exception>(()=>FacilitaPayReadChecks.Transactions(new JArray(a,a.DeepClone()),"2026-09-01T00:00:00Z","2026-09-03T23:59:59Z"));
    }
}

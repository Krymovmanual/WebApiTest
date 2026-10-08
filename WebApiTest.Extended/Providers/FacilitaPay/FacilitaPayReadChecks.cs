using Newtonsoft.Json.Linq;
using System.Globalization;
namespace WebApiTest.Extended;

internal static class FacilitaPayReadChecks
{
    internal static decimal Number(JObject row, string field)
    {
        Assert.True(row[field]?.Type is JTokenType.Integer or JTokenType.Float, "Expected JSON number: " + field);
        Assert.True(decimal.TryParse(row[field]!.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n), "Invalid numeric field: " + field);
        return n;
    }
    internal static void Balance(JObject row, string currency)
    {
        Assert.True(ProviderReadContracts.RequiredString(row,"currency") == currency,"Balance currency differs from account.");
        var debit = Number(row,"debit_value"); var credit = Number(row,"credit_value");
        Assert.True(debit >= 0 && credit >= 0,"Debit/credit totals must be nonnegative.");
        foreach(var field in new[]{"debit_transactions","credit_transactions"})
        { Assert.Equal(JTokenType.Integer,row[field]?.Type);Assert.True(row.Value<long>(field)>=0); }
        Assert.True(credit-debit == Number(row,"balance"),"Balance differs from credit minus debit; confirm provider formula before reporting a defect.");
    }
    internal static DateTimeOffset Time(string text)
    {
        Assert.True(DateTimeOffset.TryParse(text,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var time),"Invalid transaction date.");return time;
    }
    internal static void Transaction(JObject row)
    {
        ProviderReadContracts.RequiredString(row,"id");ProviderReadContracts.RequiredString(row,"currency");
        ProviderReadContracts.RequiredString(row,"exchange_currency");
        Assert.True(Number(row,"value")>=0);Assert.True(Number(row,"exchanged_value")>=0);
        Assert.Equal(JTokenType.Integer,row["status"]?.Type);
        Time(ProviderReadContracts.RequiredString(row,"inserted_at"));
        foreach(var f in new[]{"subject_is_receiver","for_exchange","exchange_under_request","estimated_value_until_exchange","cleared"})
            Assert.Equal(JTokenType.Boolean,row[f]?.Type);
        foreach(var f in new[]{"from_bank_account","to_bank_account"})
            ProviderReadContracts.RequiredString(Assert.IsType<JObject>(row[f]),"id");
    }
    internal static JArray Transactions(JArray rows,string from,string to)
    {
        var start=Time(from);var end=Time(to);Assert.True(start<=end,"Invalid configured date range.");
        Assert.NotEmpty(rows);Assert.True(rows.Count<=100,"Response exceeds requested per_page.");
        var ids=new HashSet<string>();
        foreach(var token in rows)
        {
            var row=Assert.IsType<JObject>(token);Transaction(row);
            Assert.True(ids.Add(row.Value<string>("id")!),"Duplicate transaction ID.");
            var time=Time(row.Value<string>("inserted_at")!);Assert.True(time>=start&&time<=end,"Transaction outside requested date range.");
        }
        return rows;
    }
    internal static void SameTransaction(JObject list,JObject detail)
    {
        foreach(var f in new[]{"id","currency","exchange_currency"})
            Assert.True(ProviderReadContracts.RequiredString(list,f)==ProviderReadContracts.RequiredString(detail,f),"Transaction mismatch: "+f);
        foreach(var f in new[]{"value","exchanged_value"})Assert.True(Number(list,f)==Number(detail,f),"Transaction amount mismatch: "+f);
        Assert.True(Time(list.Value<string>("inserted_at")!)==Time(detail.Value<string>("inserted_at")!),"Creation time mismatch.");
        foreach(var f in new[]{"from_bank_account","to_bank_account"})
            Assert.True(list[f]?.Value<string>("id")==detail[f]?.Value<string>("id"),"Bank account identity mismatch.");
    }
}

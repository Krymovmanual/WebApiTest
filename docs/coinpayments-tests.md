# CoinPayments read scenarios

Five live tests in Providers/CoinPayments cover balances, rates, withdrawal history, offset pagination and completed history/detail consistency. Contracts are based on supplied successful DEV samples. Original tests and other providers are unchanged.

Requests use the supplied credential aliases mCoinPublicKey and mCoinPrivateKey, not raw keys. Override with WEBAPI_CP_PUBLIC_KEY_ALIAS / WEBAPI_CP_PRIVATE_KEY_ALIAS if needed. Existing API bearer/username configuration is still required. Request bodies and sensitive note/address values are not printed.

```powershell
dotnet test .\WebApiTest.Extended\WebApiTest.Extended.csproj --settings .\WebApiTest.Extended\dev.runsettings --filter "Provider=CoinPayments&Suite=Live" --logger "trx;LogFileName=coinpayments.trx" --results-directory .\TestResults
```

Pagination requires at least 26 records and a stable history. It reads a 50-record snapshot before and after offsets 0/25; concurrent insertion causes an explicit retry failure rather than being presented as a pagination defect. Successful snapshots do not prove the absence of transient concurrent changes. Newest-first ordering follows the observed responses and still needs live confirmation.

Detail comparison selects a completed withdrawal from history; it does not pin a historical ID, mutable balance or rate. Fields compared: creation time, status/text, coin, raw/display amount, destination and txid. Detail id is not required. No universal amount scale or full status enum is inferred from the samples. BTC last_update=2147483647 is accepted without a freshness assertion. Empty balance/rate sets and empty first history fail the dataset prerequisites. No payment is created or sent.

# Provider blocks

Extended provider-specific classes live in `WebApiTest.Extended/Providers/<Provider>`.
Shared authorization, Swagger, reporting and configurable read infrastructure remain at the project root. Original `WebApiTest` tests are untouched. Existing batch name filters still match the new classes, whose names end in ProviderBatchContracts or VerifiedProviderReadContracts.

Run a provider in Visual Studio by grouping Test Explorer by Traits, then Provider. Or:

```powershell
dotnet test .\WebApiTest.Extended\WebApiTest.Extended.csproj --settings .\WebApiTest.Extended\dev.runsettings --filter "Provider=FacilitaPay" --logger "trx;LogFileName=facilita.trx" --results-directory .\TestResults
```

The new FacilitaPay scenario uses an actual account ID from getBankAccounts to call getBankAccount, compares ID and currency, and does not pin balances. The request `{ "id": "..." }` is confirmed by the supplied developer model. The detail response path `result.data` is a candidate based on the existing list wrapper, NOT a live-confirmed contract. A shape failure needs a successful detail response before being called an API defect.

## Developer-confirmed request bodies (2026-10-08)

| Operation | Body / fields |
| --- | --- |
| OpenPayd getAccounts | `{}` |
| OpenPayd getTransactionInfo | `{"id":"<actual transaction ID>"}` |
| OpenPayd getHistory | id, amountFrom/amountTo (nullable float), currency, from/to, reference, sentFrom/sentTo, shortId, transactionId, status/excludedStatuses/type (string arrays), updatedDateFrom, **UpdatedDateTo**, page/size (nullable int), sort |
| FacilitaPay getBankAccount / getBankAccountBalance / getTransaction | `{"id":"<actual ID>"}` |
| FacilitaPay getBankAccountStatement | id, from/to (nullable DateTime), inherited pagination fields not supplied |
| FacilitaPay getTransactions | from/to (DateTime), complete (bool), inherited pagination fields not supplied |

Use ISO 8601 dates for OpenPayd; the developer comment's `2019-14-29` is invalid. JsonProperty explicitly spells UpdatedDateTo with an uppercase U. Do not infer supported sort syntax or server validation from C# types. Do not infer pagination defaults from DefaultValue attributes alone.

Koywe, CoinPayments and signature models were also supplied, but are not implemented in this change. No models for Nuvei_v2 or additional Bitolo operations were supplied. Credentials shown as mCoinPrivateKey/mCoinPublicKey and dummy IDs are placeholders. Remaining configured read tests still require verified response assertions; request models alone do not supply these.

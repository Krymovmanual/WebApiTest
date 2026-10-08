# Koywe read checks

Six live cases under `Providers/Koywe`, Provider=Koywe. Existing tests remain unchanged.

- Currency catalog to detail: select a real listed symbol; compare ID, symbol and decimals; validate asset fields and ordered numeric limits.
- Token catalog to lookup: select a listed symbol; require returned symbol and selected ID. No assumption that network variants have unique symbols.
- CLP payment and payout methods: IDs, names, nonnegative numeric fees, string descriptions/details/images. Empty details are allowed.
- History: page 1 / size 50; GUID order IDs, uniqueness, numeric amounts/fees, lifecycle date strings and nullable dates, pagination envelope. Empty periods are allowed. No fixed status enum, sample count, rate or date-filter semantics inferred from one sample.
- Balance: array report, timestamp and nullable warning. Empty report is valid; populated country/currency row schema is not yet evidenced.

History defaults to 2026-09-01 through 2026-09-30; override WEBAPI_KOYWE_FROM / WEBAPI_KOYWE_TO. Balance uses that supplied fixed September period.

Supplied getOrderInfo request used a Mongo-style ID and returned GUID validation error. getOrderInfoByExternalId returned a successful null result for a demo ID. postOrder returned symbolIn validation error. None establishes a successful order-detail or creation contract; no write request added. Raw capture and personal fields are not committed.

Run:

```powershell
dotnet test WebApiTest.Extended --filter "Suite=Live&Provider=Koywe"
```

Requires normal WEBAPI_RUN_LIVE=1, base URL and bearer/stage credentials. This filter also includes existing Koywe token tests. New checks require a DEV rerun; supplied captures are evidence, not execution of the new tests.

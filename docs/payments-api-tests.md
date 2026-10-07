# Payments API tests — Swagger 1.0.10

Authoritative snapshot: supplied Quantfury API Swagger 2.0, 244 operations, 80 definitions. It includes all 193 earlier operations plus 51 new operations (ZeroHash, Infura and additional payment endpoints). The old added tests and unverified scenario placeholders have been replaced. Original WebApiTest files are unchanged.

## Run from Visual Studio

Keep WebApiTest.Extended added to your solution. Test → Configure Run Settings → Select Solution Wide runsettings File → WebApiTest.Extended/dev.runsettings. Build → Rebuild Solution. Test Explorer → WebApiTest.Extended → Run.

Default target is DEV https://qu-dev-qfwebapi.azurewebsites.net/. Credentials: ignored stage_creds.json at solution root, fields userName and password, both for DEV. Remove BEARER_TOKEN from prior local runsettings to acquire a token automatically. Token acquisition failure blocks authenticated read/session tests and is not a provider failure.

The tracked dev.runsettings contains no credentials. Copy it to ignored dev.local.runsettings before adding secrets. BEARER_TOKEN takes priority over credential variables/file. Never commit tokens or passwords.

## Implemented coverage

- Offline: contract integrity for all 244 operations, schema references, catalog and exact authorization matrix. Separate regression tests validate nested response schema checks.
- Live authorization: 370 requests (missing/malformed Bearer) for 185 Swagger-protected operations, grouped by provider. No valid credentials or populated bodies are sent. Placeholder routes use ARS/ethereum/deliberately nonexistent IDs only to exercise authorization. 400/404 is a failure, not proof of authentication enforcement.
- Live provider reads: 10 read endpoints (5 grouped in FireblocksReadContracts, plus vault listing/pagination in FireblocksVaultContracts), 4 provider token/session contracts, Version and DeployDate. Fireblocks checks additionally validate provider metadata, exchange retrieval status, nested fiat balances and wallet field types. See [Fireblocks coverage](fireblocks-coverage.md) for all 49 operations and mapping confidence. Vault listing and transaction history add two functional read endpoints; other Fireblocks gaps remain explicit.
- Live ZeroHash: 8 nonmutating collection GET requests with schema-based validation. Uses configured API credentials even though Swagger omits security. Application errors fail even with HTTP 200. Missing result fails. Optional nullable CLR properties are accepted because this Swagger 2 snapshot lacks reliable nullable metadata; required properties and present nonnull property types are enforced. No inferred numeric/ID restrictions.
- Live Infura: 3 missing-required-body validation cases, enabled only when WEBAPI_BLOCKCHAIN is explicitly configured to a supported DEV chain identifier. No transactions or wallet creation.
- Live current Swagger: verifies presence and security declarations of all 244 operations. This checks contract drift, not functional behavior.

## Remaining gaps

See coverage.csv: request/response schema availability and functionalCoverage for each operation. Authorization-only and offline catalog cases are not successful provider calls. No create/send/refund/deposit/webhook/key-management state changes are executed. Full functional coverage needs wrapper payloads for empty schemas, provider sandbox configuration, entity IDs and dedicated setup/assert/cleanup lifecycle scenarios. ZeroHash security omissions in Swagger are not evidence of actual anonymous access. Never automatically invoke its POST/PATCH/DELETE operations on that assumption.

Schema checks implement only features present in this snapshot: local references, types, properties, arrays/items, required fields, enums and typed additional properties. They are not a full JSON Schema validator or financial correctness check.

CLI offline: dotnet test WebApiTest.Extended/WebApiTest.Extended.csproj --filter "Suite=Offline"
CLI live: dotnet test WebApiTest.Extended/WebApiTest.Extended.csproj --settings WebApiTest.Extended/dev.runsettings --filter "Suite=Live"

FireblocksTransactionContracts checks transaction-history limit and createdAt DESC order, wrapper fields and nullable/unavailable fee representations. No fee arithmetic or cursor pagination is inferred.


### Viewing test responses in Visual Studio

After running Extended tests, select an individual test case in Test Explorer and open its Output / Standard Output link (label varies by VS version). xUnit captures output for passed and failed tests. A grouped theory node may not show output: select the individual data row. The green toolbar Start button starts the application, not these tests.

All live Extended requests now report method, route, HTTP status, elapsed milliseconds and configured response-time budget before assertions. Read tests report collection counts and selected IDs, asset names, balances, transaction amounts/statuses/fees and currency pairs. Exchange rates are listed individually. Empty configured collections print zero items; this does not establish provider availability or exercise pagination. Vault pagination reports whether another cursor is present. Provider session tests report token receipt without printing its value. Authorization cases state missing/malformed bearer and the expected HTTP 401/403.

Responses use an explicit field allowlist instead of raw JSON. Passwords, tokens, headers, bank account details, addresses, notes and unknown nested objects are omitted. Bank-account reads show count only. Offline tests describe fixture/documentation checks, never live provider results; the Swagger catalog summary separates functional coverage categories from authorization-only coverage. Output does not weaken existing assertions or turn application errors into success.

Example output:

```text
Request: POST /api/FireblocksProvider/getVaultAccountsPaged
HTTP 200; elapsed 180 ms; configured budget 5000 ms.
Application error: none.
result.accounts: 2 items
result.accounts[0]: id="0"; name="Default"; hiddenOnUI=false; autoFuel=false
result.accounts[0].assets: 1 items
result.accounts[0].assets[0]: id="BTC_TEST"; total=0; available=0
```

When API authentication fails before a provider request, Output identifies authorization acquisition as the failing step; the existing assertion reports the token endpoint HTTP status without its body. Live validation still requires local credentials and is not run by CI.


### Expected HTTP status and additional read scenarios

Each request now prints expected and actual HTTP status separately from body assertions.
Authorization rejection tests expect 401/403; their success does not verify a working provider operation.
An authenticated read returning 401 prints an unexpected authorization rejection.
Infura missing-body validation expects 400.

New read-envelope tests cover PayRetailers `getBalance`, Bitolo `{currency}/getBalance`,
and Infura `{blockchain}/getAccounts`. Swagger declares no body for these methods.
These tests verify HTTP/application success and a structured result only; they have not yet
been run against DEV and do not claim provider-specific field or financial correctness.
Set `WEBAPI_BITOLO_CURRENCY` to a currency configured in DEV and `WEBAPI_BLOCKCHAIN`
to a supported DEV chain in your runsettings to enable the corresponding tests. No default
currency/chain is guessed. Empty collections are permitted.
Other providers with an undocumented body still require verified request examples.
`Contracts/coverage.json` and `docs/coverage.csv` include the next step for every Swagger operation.

### Save a readable report and TRX together

From Developer PowerShell at the repository root:

```powershell
.\scripts\Run-ExtendedTests.ps1
```

The script runs Extended with `dev.runsettings` and saves both `extended-results.trx`
and `extended-results.html` in `TestResults/<timestamp>/`. Open HTML in a browser and
expand any test to see its output. Test failures still return a failing process exit code;
report generation does not turn them into passes. The report contains captured output and
failure messages, so review it before sharing. No credentials are passed by the script.

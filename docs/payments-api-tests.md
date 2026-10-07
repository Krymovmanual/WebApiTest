# Payments API tests — Swagger 1.0.10

Authoritative snapshot: supplied Quantfury API Swagger 2.0, 244 operations, 80 definitions. It includes all 193 earlier operations plus 51 new operations (ZeroHash, Infura and additional payment endpoints). The old added tests and unverified scenario placeholders have been replaced. Original WebApiTest files are unchanged.

## Run from Visual Studio

Keep WebApiTest.Extended added to your solution. Test → Configure Run Settings → Select Solution Wide runsettings File → WebApiTest.Extended/dev.runsettings. Build → Rebuild Solution. Test Explorer → WebApiTest.Extended → Run.

Default target is DEV https://qu-dev-qfwebapi.azurewebsites.net/. Credentials: ignored stage_creds.json at solution root, fields userName and password, both for DEV. Remove BEARER_TOKEN from prior local runsettings to acquire a token automatically. Token acquisition failure blocks authenticated read/session tests and is not a provider failure.

The tracked dev.runsettings contains no credentials. Copy it to ignored dev.local.runsettings before adding secrets. BEARER_TOKEN takes priority over credential variables/file. Never commit tokens or passwords.

## Implemented coverage

- Offline: contract integrity for all 244 operations, schema references, catalog and exact authorization matrix. Separate regression tests validate nested response schema checks.
- Live authorization: 370 requests (missing/malformed Bearer) for 185 Swagger-protected operations, grouped by provider. No valid credentials or populated bodies are sent. Placeholder routes use ARS/ethereum/deliberately nonexistent IDs only to exercise authorization. 400/404 is a failure, not proof of authentication enforcement.
- Live provider reads: 8 established read contracts (5 now grouped in FireblocksReadContracts), 4 provider token/session contracts, Version and DeployDate. Fireblocks checks additionally validate provider metadata, exchange retrieval status, nested fiat balances and wallet field types. See [Fireblocks coverage](fireblocks-coverage.md) for all 49 operations and mapping confidence. Endpoint coverage counts remain unchanged.
- Live ZeroHash: 8 nonmutating collection GET requests with schema-based validation. Uses configured API credentials even though Swagger omits security. Application errors fail even with HTTP 200. Missing result fails. Optional nullable CLR properties are accepted because this Swagger 2 snapshot lacks reliable nullable metadata; required properties and present nonnull property types are enforced. No inferred numeric/ID restrictions.
- Live Infura: 3 missing-required-body validation cases, enabled only when WEBAPI_BLOCKCHAIN is explicitly configured to a supported DEV chain identifier. No transactions or wallet creation.
- Live current Swagger: verifies presence and security declarations of all 244 operations. This checks contract drift, not functional behavior.

## Remaining gaps

See coverage.csv: request/response schema availability and functionalCoverage for each operation. Authorization-only and offline catalog cases are not successful provider calls. No create/send/refund/deposit/webhook/key-management state changes are executed. Full functional coverage needs wrapper payloads for empty schemas, provider sandbox configuration, entity IDs and dedicated setup/assert/cleanup lifecycle scenarios. ZeroHash security omissions in Swagger are not evidence of actual anonymous access. Never automatically invoke its POST/PATCH/DELETE operations on that assumption.

Schema checks implement only features present in this snapshot: local references, types, properties, arrays/items, required fields, enums and typed additional properties. They are not a full JSON Schema validator or financial correctness check.

CLI offline: dotnet test WebApiTest.Extended/WebApiTest.Extended.csproj --filter "Suite=Offline"
CLI live: dotnet test WebApiTest.Extended/WebApiTest.Extended.csproj --settings WebApiTest.Extended/dev.runsettings --filter "Suite=Live"

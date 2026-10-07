# WebApiTest expansion — 2026-10-07

## Baseline and implementation

Recovered uploaded archive `WebApiTest-main (1).zip` and Swagger 2 snapshot
`WebApiTest-main.json`, both from 2026-06-10. This is an uploaded snapshot, not a
verified current checkout of GitHub main. Original solution, clients, tests,
authentication helpers and pipeline are unchanged byte for byte. Original suite:
11 Facts (one Koywe Fact calls two endpoints).

New independent project: `WebApiTest.Extended/WebApiTest.Extended.csproj`.
It is intentionally outside the existing solution: select its project explicitly.
No changes to old pipeline behavior.

Swagger: 192 paths, 193 HTTP operations. `coverage.csv` classifies every operation:

| Category | Operations | What is implemented |
|---|---:|---|
| LiveReadContract | 8 | Success/status/body/budget, wrapper and provider assertions; missing/malformed Bearer |
| LiveSmoke | 2 | Version and DeployDate |
| AuthenticationInfrastructure | 1 | Token acquisition when no supplied Bearer; not counted as a dedicated token contract test |
| NeedsWrapperContractAndFixture | 79 | Catalog only; needs verified request bodies, wrapper response and deterministic test data |
| StateChangingOrUnclassified | 103 | Catalog only; needs isolated test workspace, fixtures, expected effects and cleanup |

These categories are conservative name-based triage except the explicitly selected
read routes. They are not proof that all 79 pending routes are side-effect-free.

New test cases: 207 offline cases (193 per-operation Swagger checks, catalog
consistency, schema references, numeric precision/error/empty-result regressions).
27 live cases (8 read, 16 authentication, 2 public info, 1 current Swagger).
Offline checks validate the snapshot and harness; they do not prove live endpoint
behavior or count as integration coverage of 193 operations.

## Run

Requires .NET 8 SDK and NuGet access. From repository root:

```powershell
dotnet test ./WebApiTest.Extended/WebApiTest.Extended.csproj --filter "Suite=Offline"
```

Stage integration tests default to skipped. Supply secrets locally, then:

```powershell
$env:WEBAPI_RUN_LIVE = "1"
$env:BEARER_TOKEN = "<set locally>"
dotnet test ./WebApiTest.Extended/WebApiTest.Extended.csproj --filter "Suite=Live" --logger trx
```

Instead of BEARER_TOKEN, use `STAGE_USERNAME` and `STAGE_PASSWORD`, or the existing
`stage_creds.json` next to the solution with `userName` and `password`. New harness
supports token `expires_in`, request-local Authorization, explicit 30-second
transport timeout, no redirects, sequential Stage read tests, and a configurable
5000 ms budget (`WEBAPI_MAX_RESPONSE_MS`). A token is not needed for the offline,
public info, Swagger, and missing/malformed-token cases. To run only auth cases:

```powershell
$env:WEBAPI_RUN_LIVE = "1"
dotnet test ./WebApiTest.Extended/WebApiTest.Extended.csproj --filter "FullyQualifiedName~ProtectedReadRejects"
```

`WEBAPI_BASE_URL` defaults to `https://qu-stage-qfwebapi.azurewebsites.net/`.
Override only to a known test origin; a changed base applies to both login and API
requests. Existing tests continue using their original fixed Stage URL.

No response bodies or secrets are printed by the new status/application-error
assertions. Normal test output can still reveal field names, public endpoint paths
and assertion values. Do not put credentials in committed files.

## Contract gaps and next implementation steps

Many POST parameters have an empty `{}` body schema, and most operations have no
success response schema. Provider documentation cannot determine our wrapper's
parameter names, envelopes, auth policy or error mapping. Do not substitute native
Fireblocks payloads blindly or require every negative case to return HTTP 400.

1. Retrieve current Swagger from the current Stage host; compare with snapshot.
2. Obtain wrapper DTOs/controller source or successful sanitized request/response
   fixtures for the 79 pending routes. Review route semantics before calling.
3. Implement pagination traversal with stable dataset/size/cursor assertions;
   transaction/account lookups with known Stage IDs; filtered histories, balances,
   fees and provider errors using confirmed wrapper payloads.
4. For Fireblocks fee estimation, use actual wrapper source/destination/asset/amount
   mapping. Solana ATA cases need recipients with and without an ATA, optional
   feeDetails handling and confirmed total/rent units; do not enforce rent on every
   network or assume the wrapper already exposes feeDetails.
5. Add mutation tests separately with an isolated provider sandbox, scoped test
   accounts, effects/cleanup and idempotency checks. Includes create/cancel,
   approvals, signatures, webhooks, deposits and distributed-key operations.
6. Run new suite in Stage, investigate failures and add its project to CI once
   environment and expected contracts are confirmed. Old pipeline is unchanged.

## Findings in existing code (unchanged)

- Authentication caches tokens for a fixed hour and does not use expires_in.
- Several tests only assert non-null response; a provider application error can
  still be HTTP 200.
- Exchange tests restrict types to DERIBIT_TESTNET/BITMEX_TESTNET; account/wallet
  lists are sometimes required non-empty; asset contract addresses assume EVM.
- Hardcoded environment and no explicit performance budget in shared client.
- All provider reads in existing clients send POST and `{}`; native Fireblocks GET
  endpoints must not replace these wrapper routes.

## Verification status

Original file SHA-256 preservation and catalog/snapshot consistency checked by
local Python validation. XML/JSON syntax and local schema references checked.
No .NET SDK/compiler is installed in the execution environment: C# build and xUnit
execution are **not verified**. Stage Swagger access returned proxy HTTP 502;
no live API case was run. No Bearer token or Stage credential file was found in the
uploaded archive. No payment or provider mutation was performed.

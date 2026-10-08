# Four-provider read package

Fireblocks changes are paused. Original WebApiTest files are untouched.

`ProviderBatchContracts` adds five live checks: FacilitaPay token, rates and account metadata; OpenPayd token metadata; Nuvei_v2 session success. They use established repository contracts. Optional metadata is checked only when present. An empty FacilitaPay account list is a DEV prerequisite failure, not automatically an API defect. Nuvei_v2 REST session compatibility is pending DEV validation.

Nuvei reference: https://docs.nuvei.com/rcblocks/sending-a-getsessiontoken/

## Reads requiring verified wrapper requests

Swagger declares an untyped body for these methods. Provider documentation does not establish the WebAPI body. The fixed read catalog never permits payouts, withdrawals, callbacks, approvals, updates or webhook calls.

| Local case key | Swagger route | Required request |
| --- | --- | --- |
| `BitoloBalance` | `/api/Bitolo/{currency}/getBalance` | Configured currency; no declared body |
| `BitoloTransactionStatus` | `/api/Bitolo/{currency}/getTransactionStatus` | Verified JSON object |
| `BitoloTransactionList` | `/api/Bitolo/{currency}/getTransactionList` | Verified JSON object |
| `FacilitaPayBankAccount` | `/api/FacilitaPayProvider/getBankAccount` | Verified JSON object |
| `FacilitaPayBankAccountStatement` | `/api/FacilitaPayProvider/getBankAccountStatement` | Verified JSON object |
| `FacilitaPayBankAccountBalance` | `/api/FacilitaPayProvider/getBankAccountBalance` | Verified JSON object |
| `FacilitaPayTransactions` | `/api/FacilitaPayProvider/getTransactions` | Verified JSON object |
| `FacilitaPayTransaction` | `/api/FacilitaPayProvider/getTransaction` | Verified JSON object |
| `OpenPaydHistory` | `/api/OpenPaydProvider/getHistory` | Verified JSON object |
| `OpenPaydTransactionInfo` | `/api/OpenPaydProvider/getTransactionInfo` | Verified JSON object |
| `OpenPaydAccount` | `/api/OpenPaydProvider/getAccount` | Verified JSON object |
| `OpenPaydAccounts` | `/api/OpenPaydProvider/getAccounts` | Verified JSON object |
| `NuveiV2PayoutStatus` | `/api/Nuvei_v2_Provider/getPayoutStatus` | Verified JSON object |
| `NuveiV2Requests` | `/api/Nuvei_v2_Provider/getRequests` | Verified JSON object |

## Configure all available requests in one file

Copy `WebApiTest.Extended/provider-read.example.json` to `provider-read.local.json` at the repository root. It is ignored by git. Add only requests confirmed in our Swagger/UI/backend; never use SQL parameters as an inferred WebAPI body. The empty example intentionally contains no fabricated requests. Missing cases are skipped with a reason. A malformed configured file/case fails; it never turns into a skip/pass.

Each key contains `body` (required for body operations), `checks`, and `currency` for Bitolo. No custom URL or method is accepted. Every active case must have a content/number/count/identity assertion, not just a type check. Selectors use JSONPath inside `$.result`. Comparison values and request bodies are not printed.

The following is the configuration format only, **not a runnable provider body or claimed response schema**:

```json
{
  "version": 1,
  "cases": {
    "OpenPaydAccount": {
      "body": { "<verified-wrapper-field>": "<DEV-fixture-id>" },
      "checks": [
        { "select": "$.result.<verified-id-field>", "op": "equals", "value": "<DEV-fixture-id>" },
        { "select": "$.result.<verified-balance-field>", "op": "number" }
      ]
    }
  }
}
```

| Assertion | Meaning |
| --- | --- |
| `type` + `value` | object, array, string, boolean, integer or number |
| `equals` + `value` | exact JSON value/type; use for fixture identity, requested currency or status |
| `nonEmpty` | nonempty string, object or array |
| `number` + optional `min`/`max` | invariant decimal or numeric string; negative balances are allowed unless a minimum is configured |
| `count` + `min`, optional `max` | array size bounds; set min 1 when empty cannot prove the scenario |
| `unique` | distinct nonempty selected scalar identities, e.g. `$.result.items[*].id` |

All selectors must match at least one non-null value. Wrong IDs, duplicate IDs, missing fields and explicit false wrapper success flags fail. No response snapshots or changing balances are compared implicitly. Checks do not prove pagination, filter exclusion, accounting accuracy or fields that were not configured.

Set an absolute `WEBAPI_PROVIDER_READ_FILE` to use another local filename, or use the default root file. Select the same DEV runsettings/credentials you already use. Restart discovery/rebuild if Visual Studio cached a skipped configuration.

```powershell
git pull
dotnet test .\WebApiTest.Extended\WebApiTest.Extended.csproj --settings .\WebApiTest.Extended\dev.runsettings --filter "FullyQualifiedName~ProviderBatchContracts|FullyQualifiedName~VerifiedProviderReadContracts" --logger "trx;LogFileName=provider-batch.trx" --results-directory .\TestResults
```

Reports show timing, expected/actual status, counts, allowlisted response fields and assertion outcomes. Token/merchant/owner/account-number fields are omitted. New configured scenarios remain unverified until run against confirmed DEV bodies and data. Existing authorization coverage remains separate.


## Collect the missing bodies and responses as one package

The current accessible repository contains tests, not the WebAPI controller implementation. Existing DEV report `provider-batch.trx` executed five checks successfully: FacilitaPay token, twelve positive rates and eight unique bank accounts; OpenPayd token; Nuvei_v2 session. The fourteen configured reads were not executed. This report does not contain their request bodies.

Bitolo balance now also belongs to the normal batch. Set `WEBAPI_BITOLO_CURRENCY` to a confirmed configured currency in the selected runsettings; this basic read needs no local body profile. It still proves only the successful structured envelope, not the exact balance or currency mapping.

To capture all available wrapper requests in one browser session:

1. Open the DEV WebAPI Swagger you already use. Open DevTools → Network and enable Preserve log.
2. Execute the available **read methods listed above**, using known DEV account/transaction identifiers. Do not execute withdrawals, payouts, updates, callbacks or webhook methods. If a required body is unknown, leave that method uncaptured.
3. Export the Network log as HAR **with response content** to a local file. Browser authentication values may be in the original HAR, so import it locally rather than upload the raw HAR.
4. Run the offline importer:

```powershell
git pull
.\scripts\Import-ProviderReadHar.ps1 -HarFile "$env:USERPROFILE\Downloads\providers.har"
```

Send `provider-captures.local.json` from the repository root after reviewing it. It contains only successful supported DEV WebAPI reads. Cookies, headers and nested secret/owner/bank-number/address fields are omitted or redacted. The script makes no network calls, does not overwrite an existing capture and reports methods that were not captured. Use `-OutputFile .\provider-captures-next.local.json` for another batch.

Captured responses are evidence for building assertions, **not automatically generated functional coverage**. No `provider-read.local.json` is generated from a successful snapshot. Request fields that were redacted must be restored locally from confirmed data before replay. On older PowerShell versions, JSON date values may be normalized to an ISO string in the redacted capture; do not use those captures to assert exact wire-date formatting.

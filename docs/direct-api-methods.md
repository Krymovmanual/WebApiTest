# Direct API method calls

Extended contains 97 read method tests from the supplied `swagger_pay.json`. Each test sends one request to its named method with real API authorization, then prints the full request JSON, HTTP status, elapsed time and full response JSON. Credential fields are masked. Select the individual test and open **Standard Output** in Visual Studio.

Tests are grouped by **Provider**. For example, **CoinPayments → getBalances** displays the balance JSON; `getWithdrawalHistory` uses `Start: 25`. **Koywe → getCurrencyTokenPairs** calls that method directly, without comparing it with another response. **Fireblocks → getTransactions** makes one request, without filter, cursor, sorting or financial assertions.

HTTP 200 and absence of explicit application errors are checked. Empty arrays, null results, changing balances and different orderings are accepted. There are no synthetic offline cases, invalid-token cases, multi-request scenarios or response-time assertions. Write methods, callbacks and withdrawal creation are excluded. Kasha, Maldo, PayRetailers and Wyre remain excluded. Original `WebApiTest` is unchanged.

## Run in Visual Studio

1. Pull `main`, then **Build → Rebuild Solution**.
2. **Test → Configure Run Settings → Select Solution Wide runsettings File**: choose `WebApiTest.Extended/dev.runsettings`.
3. In Test Explorer, group by **Traits**, expand the provider and run its method tests.

Authorization uses `BEARER_TOKEN`, or `STAGE_USERNAME` plus `STAGE_PASSWORD`, or ignored `stage_creds.json` at the solution root (`userName` and `password`). The default target is DEV; `WEBAPI_BASE_URL` selects another HTTPS origin.

## Real request inputs

Provided CoinPayments and FacilitaPay entity IDs are retained. Swagger demonstration entity IDs, account addresses, hashes and email addresses are not sent. Methods needing those values are **Skipped**, with the missing fields named in the skip reason. A skip does not establish that the endpoint works.

Copy `WebApiTest.Extended/api-requests.example.json` to ignored `api-requests.local.json` in the solution root or Extended directory. Keep only the methods you need and fill their null fields with real data. Request keys are the exact HTTP method and Swagger route:

```json
{
  "POST /api/FireblocksProvider/getBalance": {
    "body": { "accountId": "YOUR_REAL_ACCOUNT_ID", "AssetId": "ETH" }
  },
  "POST /api/Bitolo/{currency}/getBalance": {
    "pathValues": { "currency": "ARS" }
  }
}
```

`body`, `pathValues` and `query` merge into the catalog defaults. Route and HTTP method cannot be overridden. `WEBAPI_REQUESTS_FILE` may select an explicit configuration file, including on Azure. Existing `WEBAPI_BITOLO_CURRENCY`, `WEBAPI_BITOLO_ORDER_ID`, `WEBAPI_BLOCKCHAIN`, and CoinPayments alias variables remain supported. The former `provider-read.local.json` assertion format is retired.

CLI with HTML/TRX reports:

```powershell
.\scripts\Run-ExtendedTests.ps1
```

Azure enables the live suite with `WEBAPI_RUN_LIVE=1`. Missing entity inputs still skip their methods. GitHub Actions only builds and checks discovery; it does not verify live provider availability. A red test retains the actual HTTP/application error and full response in its output.

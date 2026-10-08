# Direct API method calls

Extended contains 93 read method tests from the supplied Swagger and successful request examples. Each test sends one request to its named method with real API authorization, then prints the full request JSON, HTTP status, elapsed time and full response JSON. Credential fields are masked. Select the individual test and open **Standard Output** in Visual Studio.

Tests are grouped by **Provider**. For example, **CoinPayments → getBalances** displays the balance JSON; `getWithdrawalHistory` uses `Start: 25`. **Koywe → getCurrencyTokenPairs** calls that method directly, without comparing it with another response. **Fireblocks → getTransactions** makes one request, without filter, cursor, sorting or financial assertions.

HTTP 200 and absence of explicit application errors are checked. Empty arrays, null results, changing balances and different orderings are accepted. There are no synthetic offline cases, invalid-token cases, multi-request scenarios or response-time assertions. Write methods, callbacks and withdrawal creation are excluded. Kasha, Maldo, PayRetailers and Wyre remain excluded. The eleven legacy `WebApiTest` test cases were deleted at the user's request; shared API clients remain.

## Run in Visual Studio

1. Pull `main`, then **Build → Rebuild Solution**.
2. **Test → Configure Run Settings → Select Solution Wide runsettings File**: choose `WebApiTest.Extended/dev.runsettings`.
3. In Test Explorer, group by **Traits**, expand the provider and run its method tests.

Authorization uses `BEARER_TOKEN`, or `STAGE_USERNAME` plus `STAGE_PASSWORD`, or ignored `stage_creds.json` at the solution root (`userName` and `password`). The default target is DEV; `WEBAPI_BASE_URL` selects another HTTPS origin.

## Real request inputs

Provided CoinPayments, FacilitaPay and Bitolo entity IDs are retained for DEV. Other environments need their own configured entity IDs. Swagger demonstration entity IDs, account addresses, hashes and email addresses are not sent. Methods needing those values are **Skipped**, with the missing fields named in the skip reason. A skip does not establish that the endpoint works.

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

The Stage Azure pipeline enables the live suite with `WEBAPI_RUN_LIVE=1` and targets the Stage origin with the existing Stage credentials. Configure Stage entity inputs using `WEBAPI_REQUESTS_FILE`; missing inputs still skip their methods. GitHub Actions only builds and checks discovery; it does not verify live provider availability. A red test retains the actual HTTP/application error and full response in its output.

## Infura supplied examples

Eight direct calls use the supplied successful `eth` examples without extra configuration: `getAccounts`, `getBalance`, `getEthTransactions`, `getBlockByNumber`, `getTransactionRecipient`, `getTransactionByHash`, `getGasPrice`, and `estimateFees`. Each prints its current full JSON. Historical balances, gas, block metadata and transaction contents are not asserted; empty arrays remain valid. The route named `getTransactionRecipient` returns the receipt in the supplied capture, so its response is printed unchanged.

The other four Infura methods still need configured inputs. No `getTransactionCount` example was supplied. The token history example returned `Contract addresses are forbidden.` after the token contract was also used as the wallet address. Token balance returned zero with `decimals: 0` and `symbol: null`, which does not establish a working token-contract input. Token transaction lookup used a 32-byte transaction hash for `contractAddress`, instead of a 20-byte EVM address. These examples are not promoted to successful default requests or expected-error tests; configure real contract/wallet inputs in `api-requests.local.json`.

`generateAddress` remains outside the automatic read suite because it creates an address. `estimateFees` only requests a fee estimate and does not send a transaction. `WEBAPI_BLOCKCHAIN` and local request configuration can override the supplied chain and inputs; use examples from the chosen chain.

## Removed tests

At the user's request, the ZeroHash collection reads `GET /api/ZeroHashProvider/deposits/digital_asset_addresses` and `GET /api/ZeroHashProvider/withdrawals/requests`, all three QuantfuryPayments/QuantfuryPaymentsSign cases, and the eleven legacy cases formerly shown under `No Traits` were deleted. Remaining ZeroHash detail routes are retained. This removes test coverage only; it does not remove backend endpoints or shared clients. The `Suite=Live` trait has been removed. Test Explorer groups each method only under its provider; Azure executes the same tests without a Suite filter.

## Bitolo supplied inputs

`getBalance` and `getTransactionStatus` use the user's successful `ars` inputs. The status request uses the provided DEV order ID; other API environments require their own `order_id`. `getTransactionList` uses the same configured currency and its existing unfiltered page request. The supplied balance, amounts and SUCCESS status are not pinned to historical values. All three methods print the full current response JSON. Existing `WEBAPI_BITOLO_CURRENCY` and `WEBAPI_BITOLO_ORDER_ID` overrides remain supported.

## Azure execution and grouping

Only `Provider` traits remain on method tests. Removing `Suite=Live` changes Test Explorer grouping, not request execution or scheduling. Keep `WEBAPI_RUN_LIVE=1` in the Azure job. A manually configured hourly Azure schedule runs the same method tests; this change does not create a new schedule. Any external command using `--filter "Suite=Live"` should omit that filter or use `Provider=<name>`.

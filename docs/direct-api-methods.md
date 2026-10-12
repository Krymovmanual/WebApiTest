# Direct API method calls

Extended contains 96 method tests (93 reads, one DEV transfer, auto-fuel configuration and balance refresh) from the supplied Swagger and successful request examples. Each test sends one request to its named method with real API authorization, then prints the full request JSON, HTTP status, elapsed time and full response JSON. Credential fields are masked. Select the individual test and open **Standard Output** in Visual Studio.

Tests are grouped by **Provider**. For example, **CoinPayments → getBalances** displays the balance JSON; `getWithdrawalHistory` uses `Start: 25`. **Koywe → getCurrencyTokenPairs** calls that method directly, without comparing it with another response. **Fireblocks → getTransactions** makes one request, without filter, cursor, sorting or financial assertions.

HTTP 200 and absence of explicit application errors are checked. Empty arrays, null results, changing balances and different orderings are accepted. There are no synthetic offline cases, invalid-token cases, multi-request scenarios or response-time assertions. Write methods, callbacks and withdrawal creation are excluded except the explicitly authorized Fireblocks DEV createTransaction, setAutoFuel and refreshBalance tests. Kasha, Maldo, PayRetailers and Wyre remain excluded. The eleven legacy `WebApiTest` test cases were deleted at the user's request; shared API clients remain.

## Run in Visual Studio

1. Pull `main`, then **Build → Rebuild Solution**.
2. **Test → Configure Run Settings → Select Solution Wide runsettings File**: choose `WebApiTest.Extended/dev.runsettings`.
3. In Test Explorer, group by **Traits**, expand the provider and run its method tests.

Authorization uses `BEARER_TOKEN`, or `DEV_USERNAME` plus `DEV_PASSWORD`, or ignored `dev_creds.json` at the solution root (`userName` and `password`). The default target is DEV; `WEBAPI_BASE_URL` selects another HTTPS origin.

## Real request inputs

Provided CoinPayments, FacilitaPay, Bitolo, Fireblocks and Infura inputs are retained for DEV. Other environments need their own configured entity IDs. Swagger demonstration entity IDs, account addresses, hashes and email addresses are not sent. Methods needing those values are **Skipped**, with the missing fields named in the skip reason. A skip does not establish that the endpoint works.

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

The Dev Azure pipeline enables the live suite with `WEBAPI_RUN_LIVE=1` and targets the Dev origin with the DEV credentials. Configure DEV entity inputs using `WEBAPI_REQUESTS_FILE`; missing inputs still skip their methods. GitHub Actions only builds and checks discovery; it does not verify live provider availability. A red test retains the actual HTTP/application error and full response in its output.

## Infura supplied examples

Ten direct calls use the supplied successful `eth` examples without extra configuration: `getAccounts`, `getBalance`, `getEthTransactions`, `getBlockByNumber`, `getTransactionRecipient`, `getTransactionByHash`, `getGasPrice`, `estimateFees`, `getTransactionCount`, and token `getBalance`. Each prints its current full JSON. Historical balances, gas, block metadata and transaction contents are not asserted; empty arrays remain valid. The route named `getTransactionRecipient` returns the receipt in the supplied capture, so its response is printed unchanged.

The supplied token balance request uses contract `0xc3761EB917CD790B30dAD99f6Cc5b4Ff93C4F9eA` and the supplied wallet address. Its captured zero balance, zero decimals and null symbol are accepted as returned data; the test does not establish token metadata correctness. Token history and token transaction lookup still require real inputs. The previous failing examples for these two methods are not reused.

`generateAddress` remains outside the automatic read suite because it creates an address. `estimateFees` only requests a fee estimate and does not send a transaction. `WEBAPI_BLOCKCHAIN` and local request configuration can override the supplied chain and inputs; use examples from the chosen chain.

## Removed tests

At the user's request, the ZeroHash collection reads `GET /api/ZeroHashProvider/deposits/digital_asset_addresses` and `GET /api/ZeroHashProvider/withdrawals/requests`, all three QuantfuryPayments/QuantfuryPaymentsSign cases, and the eleven legacy cases formerly shown under `No Traits` were deleted. Remaining ZeroHash detail routes are retained. This removes test coverage only; it does not remove backend endpoints or shared clients. The `Suite=Live` trait has been removed. Test Explorer groups each method only under its provider; Azure executes the same tests without a Suite filter.

## Bitolo supplied inputs

`getBalance` and `getTransactionStatus` use the user's successful `ars` inputs. The status request uses the provided DEV order ID; other API environments require their own `order_id`. `getTransactionList` uses the same configured currency and its existing unfiltered page request. The supplied balance, amounts and SUCCESS status are not pinned to historical values. All three methods print the full current response JSON. Existing `WEBAPI_BITOLO_CURRENCY` and `WEBAPI_BITOLO_ORDER_ID` overrides remain supported.

## Azure execution and grouping

Only `Provider` traits remain on method tests. Removing `Suite=Live` changes Test Explorer grouping, not request execution or scheduling. Keep `WEBAPI_RUN_LIVE=1` in the Azure job. The canonical `azure-pipelines-dev-hourly.yml` matches the deployed Azure pipeline: hourly main schedule, `payments-tests` variable group, Windows 2025, .NET SDK 8.0.425 and TRX publication. See `azure-devops-dev.md` for update instructions. Any external command using `--filter "Suite=Live"` should omit that filter or use `Provider=<name>`.

## Fireblocks supplied DEV inputs

Nine previously gated reads now use the user's successful bodies: `getAccountAddresses` (id 4, ETH), `getAccountAddressesPaginated` (id 12, ETH, limit 100, after empty), `getBalance` (account 14, ETH_TEST5), `getExchangeAccountById`, `getMaxSpendableAmount` (account 14, BTC_TEST), `getNetworkFee` (ETH_TEST5, account 14 and the supplied one-time address), `getTransaction`, `getUnspentInputs` (account 14, BTC_TEST), and `getVaultAccount` (id 12). Fee estimation does not send a transfer. Empty address arrays and null paging are accepted. Captured amounts, fees, transaction status and asset lists are not pinned. Full current responses are printed.

These supplied Fireblocks and new Infura inputs are gated to the DEV host. Another environment needs configured values, including nested source/destination fields and route selectors; local overrides remain supported. Total discovery is now 96 methods. With no additional local configuration, these 11 examples reduce missing-input skips from 38 to 27; actual HTTP outcomes must be checked in a fresh live run.

## Hourly DEV transfer

The user explicitly authorized `POST /api/FireblocksProvider/createTransaction` in the hourly suite. Each execution creates one new transfer: ETH_TEST5, vault account 14, amount 0.00001, destination 0xbc9B16dc45BeF56c8b4E332387fa330Be95aF6ca. A new UUID is generated at execution and assigned to both externalTxId and idempotencyKey. customerRefId stays configured. The response must contain result.id or result.txId; no completion status is asserted. Full JSON is printed. There is no automatic retry, polling, or second transfer; after an ambiguous timeout inspect the printed operation ID before rerunning. This test runs in local DEV Run All as well as Azure hourly runs. Discovery does not create transactions, and the test is blocked outside the DEV host. Asset, source, destination and amount are checked before sending so local overrides cannot redirect or increase this authorized transfer. Pipeline settings remain unchanged.

## Auto-fuel and balance refresh

The supplied DEV calls `setAutoFuel` (id 14, autoFuel true) and `refreshBalance` (accountId 14, AssetId ETH_TEST5) are included in Run All and the hourly Azure suite. Both are blocked outside the DEV host. setAutoFuel sets vault 14 auto-fuel to true on every run; it does not toggle or restore the setting. refreshBalance requests the current refreshed balance; historical amounts and block heights are not asserted. Both use existing HTTP/application error checks and print full responses. These operations are separate from transaction creation and do not generate transaction IDs. Local request overrides remain supported. Total discovery: 96 methods; 27 methods still need missing real inputs in the default configuration.

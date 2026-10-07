# Fireblocks mapping and test plan

Our WebAPI routes use POST, although native provider reads generally use GET.
The mapping below is a **candidate inferred from route names**, not a verified
mapping from wrapper source. Sources checked 2026-10-07:

- https://api-reference.fireblocks.com/
- https://github.com/fireblocks/ts-sdk
- https://developers.fireblocks.com/reference/estimate-transaction-fee
- https://fireblocks.readme.io/reference/getinternalwalletassetspaginated

| WebAPI suffix under `/api/FireblocksProvider/` | Candidate native endpoint | New test status |
|---|---|---|
| getAssets | GET /supported_assets | Live wrapper contract; decimals, ID uniqueness, no EVM-only regex |
| getExchangeAccounts | GET /exchange_accounts | Live wrapper contract; IDs, names, asset balances; no two-value exchange type restriction |
| getFiatAccounts | GET /fiat_accounts | Live inferred list contract; must validate wrapper shape in Stage |
| getInternalWallets | GET /internal_wallets | Live wrapper contract; empty collection allowed |
| getExternalWallets | GET /external_wallets | Live wrapper contract; empty collection allowed |
| getVaultAccountsPaged | GET /vault/accounts_paged | Needs wrapper pagination body and response fixture |
| getVaultAccount | GET /vault/accounts/{vaultAccountId} | Needs known test vault and wrapper ID field |
| getTransactions | GET /transactions | Needs wrapper filters, response fixture and stable dataset |
| getTransaction | GET /transactions/{txId} | Needs known Stage transaction ID |
| getNetworkFee | GET /estimate_network_fee | Needs wrapper asset field and units |
| estimateFeeForTransaction | POST /transactions/estimate_fee | Needs actual transfer-shaped wrapper body; provider guide alone insufficient |
| getInternalWalletPaginated | GET /internal_wallets/{walletId}/assets | Ambiguous wrapper name; source required before testing |
| createValutAccount | POST /vault/accounts | Preserve wrapper spelling; isolated sandbox and cleanup required |
| createTransaction | POST /transactions | Isolated sandbox, approved test transfer and effects verification required |
| cancelTransaction | POST /transactions/{txId}/cancel | Known cancellable fixture and state verification required |

The native endpoint candidates do not prove wrapper request fields or response
schemas. Original wrapper clients confirm the first five route names and HTTP
methods; only their existing tests provide local response-envelope evidence.

Fee plan: estimate against the planned source, destination and amount. Cover native
SOL versus Solana token, existing ATA versus absent ATA, optional breakdown and
non-negative fee components. Confirm how the wrapper exposes rent/paidRent/totalFee
before adding assertions. Unit conversion and transaction webhook persistence
need backend DTO/DB mapping, not just provider documentation.

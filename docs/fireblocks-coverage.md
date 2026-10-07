# Fireblocks: wrapper coverage and provider mapping

Reviewed 2026-10-07. Official SDK reference pinned to [4bc7844](https://github.com/fireblocks/ts-sdk/tree/4bc7844181b41c0947b89af0cc63b221b6718a30). All mappings outside the five established wrapper calls are hypotheses based on names, not proof of controller forwarding. No WebAPI controller/service implementation was supplied.

The supplied Swagger contains **49 Fireblocks operations** (including Gas Station). Only five have verified empty request bodies and successful-response checks. Authentication coverage is recorded separately in coverage.csv. Moving tests into a provider group adds no newly covered endpoint.

## Implemented read checks

- Dedicated `FireblocksReadContracts` group: asset metadata, exchange balances and retrieval status, fiat asset balances, internal and external wallet asset structure.
- The API wrapper success contract still checks HTTP 200, application error, nonnull result and response-time budget. Asset list output retains count and id/name. Other collections log counts only.
- Optional SDK fields are validated only when present. Empty configured accounts/wallets remain valid. Asset catalogue remains nonempty. No EVM-only address regex, exchange allowlist, ID-format assumption or closed wallet-status enum.
- Legacy wrapper-required fields stay required. SDK 34.0.0 optionality does not silently weaken an already established wrapper contract. Exchange `success=false` fails: HTTP 200 alone is not successful balance retrieval.
- External-wallet balances are not treated as a financial balance source. No cross-provider balance equality or fee accounting formulas are assumed.

## Every Swagger operation

| Our POST method | Provider candidate | Confidence | Functional coverage / next step |
|---|---|---|---|
| `GasStation/createTransaction` | [POST /transactions](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#createTransaction) | CandidateByName | ChangesState — wrapper contract required |
| `getAssetsData` | Unresolved | NeedsImplementation | ReadOrEstimate — wrapper contract required |
| `getAssets` | [GET /supported_assets](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/BlockchainsAssetsApi.md#getSupportedAssets) | EstablishedWrapperCall | Read contract |
| `getVaultAccountsPaged` | [GET /vault/accounts_paged](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#getPagedVaultAccounts) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getVaultAccount` | [GET /vault/accounts/{vaultAccountId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#getVaultAccount) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getFiatAccounts` | [GET /fiat_accounts](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/FiatAccountsApi.md#getFiatAccounts) | EstablishedWrapperCall | Read contract |
| `getFiatAccount` | [GET /fiat_accounts/{accountId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/FiatAccountsApi.md#getFiatAccount) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getNetworkFee` | [GET /estimate_network_fee](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#estimateNetworkFee) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getTransactions` | [GET /transactions](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#getTransactions) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getTransaction` | [GET /transactions/{txId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#getTransaction) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `createTransaction` | [POST /transactions](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#createTransaction) | CandidateByName | ChangesState — wrapper contract required |
| `cancelTransaction` | [POST /transactions/{txId}/cancel](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#cancelTransaction) | CandidateByName | ChangesState — wrapper contract required |
| `estimateFeeForTransaction` | [POST /transactions/estimate_fee](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/TransactionsApi.md#estimateTransactionFee) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `createValutAccount` | [POST /vault/accounts](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#createVaultAccount) | CandidateByName | ChangesState — wrapper contract required |
| `updateVaultAccount` | [PUT /vault/accounts/{vaultAccountId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#updateVaultAccount) | CandidateByName | ChangesState — wrapper contract required |
| `setAutoFuel` | [POST /vault/accounts/{vaultAccountId}/set_auto_fuel](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#setVaultAccountAutoFuel) | CandidateByName | ChangesState — wrapper contract required |
| `getMaxSpendableAmount` | [GET /vault/accounts/{vaultAccountId}/{assetId}/max_spendable_amount](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#getMaxSpendableAmount) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getUnspentInputs` | [GET /vault/accounts/{vaultAccountId}/{assetId}/unspent_inputs](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#getUnspentInputs) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getBalance` | [GET /vault/accounts/{vaultAccountId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#getVaultAccountAsset) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `refreshBalance` | [POST /vault/accounts/{vaultAccountId}/{assetId}/balance](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#updateVaultAccountAssetBalance) | CandidateByName | ChangesState — wrapper contract required |
| `createVaultAsset` | [POST /vault/accounts/{vaultAccountId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#createVaultAccountAsset) | CandidateByName | ChangesState — wrapper contract required |
| `getInternalWallets` | [GET /internal_wallets](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#getInternalWallets) | EstablishedWrapperCall | Read contract |
| `getInternalWallet` | [GET /internal_wallets/{walletId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#getInternalWallet) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getInternalWalletPaginated` | Unresolved | NeedsImplementation | ReadOrEstimate — wrapper contract required |
| `getInternalWalletAsset` | [GET /internal_wallets/{walletId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#getInternalWalletAsset) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `createInternalWallet` | [POST /internal_wallets](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#createInternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `setCustomerRefIdForInternalWallet` | [POST /internal_wallets/{walletId}/set_customer_ref_id](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#setCustomerRefIdForInternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `setCustomerRefIdForVaultAccount` | [POST /vault/accounts/{vaultAccountId}/set_customer_ref_id](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#setVaultAccountCustomerRefId) | CandidateByName | ChangesState — wrapper contract required |
| `createInternalWalletAsset` | [POST /internal_wallets/{walletId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#createInternalWalletAsset) | CandidateByName | ChangesState — wrapper contract required |
| `deleteInternalWalletAsset` | [DELETE /internal_wallets/{walletId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#deleteInternalWalletAsset) | CandidateByName | ChangesState — wrapper contract required |
| `deleteInternalWallet` | [DELETE /internal_wallets/{walletId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/InternalWalletsApi.md#deleteInternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `getExternalWallets` | [GET /external_wallets](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#getExternalWallets) | EstablishedWrapperCall | Read contract |
| `getExternalWallet` | [GET /external_wallets/{walletId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#getExternalWallet) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getExternalWalletAsset` | [GET /external_wallets/{walletId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#getExternalWalletAsset) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `createExternalWallet` | [POST /external_wallets](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#createExternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `setCustomerRefIdForExternalWallet` | [POST /external_wallets/{walletId}/set_customer_ref_id](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#setExternalWalletCustomerRefId) | CandidateByName | ChangesState — wrapper contract required |
| `createExternalWalletAsset` | [POST /external_wallets/{walletId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#addAssetToExternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `deleteExternalWalletAsset` | [DELETE /external_wallets/{walletId}/{assetId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#removeAssetFromExternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `deleteExternalWallet` | [DELETE /external_wallets/{walletId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExternalWalletsApi.md#deleteExternalWallet) | CandidateByName | ChangesState — wrapper contract required |
| `webhook` | Unresolved | NeedsImplementation | InboundWebhook — wrapper contract required |
| `getAccountAddresses` | Unresolved | NeedsImplementation | ReadOrEstimate — wrapper contract required |
| `getAccountAddressesPaginated` | [GET /vault/accounts/{vaultAccountId}/{assetId}/addresses_paginated](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#getVaultAccountAssetAddressesPaginated) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `generateNewAddress` | [POST /vault/accounts/{vaultAccountId}/{assetId}/addresses](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#createVaultAccountAssetAddress) | CandidateByName | ChangesState — wrapper contract required |
| `setCustomerRefIdForAddress` | [POST /vault/accounts/{vaultAccountId}/{assetId}/addresses/{addressId}/set_customer_ref_id](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#setCustomerRefIdForAddress) | CandidateByName | ChangesState — wrapper contract required |
| `setDescriptionForAddress` | [PUT /vault/accounts/{vaultAccountId}/{assetId}/addresses/{addressId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/VaultsApi.md#updateVaultAccountAssetAddress) | CandidateByName | ChangesState — wrapper contract required |
| `createDeposit` | Unresolved | NeedsImplementation | ChangesState — wrapper contract required |
| `getExchangeAccounts` | [GET /exchange_accounts (legacy candidate)](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExchangeAccountsApi.md) | EstablishedWrapperCall | Read contract |
| `getExchangeAccountById` | [GET /exchange_accounts/{exchangeAccountId}](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/apis/ExchangeAccountsApi.md#getExchangeAccount) | CandidateByName | ReadOrEstimate — wrapper contract required |
| `getFeeForAsset` | Unresolved | NeedsImplementation | ReadOrEstimate — wrapper contract required |

## Important ambiguities

- `getAssetsData`: Ambiguous: could mean asset metadata or aggregated vault assets. Need wrapper implementation.
- `getExchangeAccounts`: Existing client confirms POST with empty body and array result. SDK now documents paged listing; legacy provider route/version must be confirmed.
- `getInternalWalletPaginated`: Ambiguous: SDK paginates assets of one internal wallet; our method name might mean wallets. Need implementation.
- `getAccountAddresses`: Legacy unpaginated address listing candidate; current SDK exposes addresses_paginated. Verify provider version.
- `createDeposit`: Do not infer a funds deposit from the name. Need implementation and financial side-effect contract.
- `getFeeForAsset`: Could be network estimate, transaction estimate or local fee configuration. Need implementation.
- `webhook`: Inbound provider event handler, not an outbound Fireblocks REST call. Need signature/replay and event-to-DB contract.
- `estimateFeeForTransaction`: Fee estimate only. Solana destination-dependent rent needs configured source/destination/amount and workspace support; no invented universal fee formula.
- `getNetworkFee`: Network estimate does not take destination; do not treat it as the total destination-specific Solana transaction cost.

## Next functional scenarios

1. Vault listing/pagination → retrieve selected vault → asset balance → address listing. Verify wrapper keys, response shape, limit/cursor behavior and stable identities; do not assert changing balances equal across requests.
2. Transaction history → retrieve a known test transaction. Check identifiers, asset, status and documented numeric fields. Do not infer provider status mappings without our implementation.
3. Network and transaction fee estimates. Use configured asset/network and a dedicated test source/destination. For Solana, verify whether our wrapper preserves rent/feeDetails and how our API calculates the displayed total.
4. Internal/external wallet details and assets; exchange and fiat detail calls. Use known test IDs and validate requested identity in the returned data.
5. Create/update/delete scenarios require dedicated entities plus verified cleanup; vault creation cannot assume a delete API exists. Transaction create/cancel, Gas Station, address generation, deposits and webhook replay need separate lifecycle assertions.

To unblock parameterized reads, provide a working, sanitized WebAPI request/response example (or controller/service source) for getVaultAccountsPaged, getVaultAccount, getBalance, getNetworkFee and getTransactions. Provider SDK parameter names alone are insufficient evidence for the JSON keys accepted by our wrapper.

Provider sources: [API reference](https://api-reference.fireblocks.com/), [official SDK endpoint index](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/README.md), [asset model](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/models/AssetTypeResponse.md), [exchange model](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/models/ExchangeAccount.md), [wallet model](https://github.com/fireblocks/ts-sdk/blob/4bc7844181b41c0947b89af0cc63b221b6718a30/docs/models/UnmanagedWallet.md).

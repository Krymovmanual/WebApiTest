# Azure DevOps DEV pipeline

The canonical pipeline is `azure-pipelines-dev-hourly.yml`. It matches the working configuration supplied by DevOps for Azure Repos `payments-tests`.

- DEV API: `https://qu-dev-qfwebapi.azurewebsites.net/`
- Variable group: `payments-tests`
- Secret variables: `DEV_USERNAME`, `DEV_PASSWORD`
- Agent image: `windows-2025`
- .NET SDK: `8.0.425`
- Manual runs and hourly runs of `main`; no push or PR triggers.
- Run only `WebApiTest.Extended`, without a Suite filter.
- Publish TRX even when tests fail; fail the job for failed tests or missing reports.

## Setup and updates

1. Authorize the existing `payments-tests` variable group for `WebApiTest-DEV`.
2. Keep the Azure pipeline YAML path at `/azure-pipelines-dev-hourly.yml`.
3. Update source files through an Azure branch and PR, preserving Azure Git history. Remove obsolete source files identified by the update; simply copying over old files can retain deleted tests.
4. Keep credentials and Azure permissions in Azure. Never commit `dev_creds.json`, bearer tokens or private request configurations.
5. Future ZIP exports must include this tracked YAML unchanged. Do not regenerate a second pipeline from old setup instructions.
6. Run manually after importing an update and inspect the Tests tab.

The former `azure-pipelines-dev.yml` is retired. When updating an older checkout, remove that file and keep the existing hourly YAML path. An Azure pipeline still configured to use the retired path must be switched once to the canonical path; do not create a duplicate schedule.

For local Visual Studio runs, select `WebApiTest.Extended/dev.runsettings` and use ignored `dev_creds.json` at the repository root, with `userName` and `password`, or the same DEV environment variables. Pipeline changes do not alter local test behavior.

Methods missing real inputs are skipped. Optional additional inputs may be supplied through an authorized Azure Secure File and `WEBAPI_REQUESTS_FILE`. See `direct-api-methods.md`.

## Telegram notification

The canonical YAML includes the DevOps-supplied `scripts/Send-TelegramTestSummary.ps1` after TRX publication. Preserve both files in future source exports. The step uses `condition: always()` and `continueOnError: true`. Existing Azure variables `TELEGRAM_BOT_TOKEN` and `TELEGRAM_CHAT_ID` provide the destination and credential; do not commit their values. No Telegram message is sent by GitHub build/discovery CI. The script is preserved as supplied: its Failed count is total minus passed and therefore includes skipped/non-passing tests. Missing TRX raises an error in the notification step. Azure has already deployed this integration; this change synchronizes the GitHub source for future updates.

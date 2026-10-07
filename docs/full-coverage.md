# Coverage expansion

The 193 Swagger operations are NOT all functionally verified.

- 163 Bearer-protected operations now each have missing/malformed-token integration cases (326 cases), grouped by provider in Test Explorer. These never acquire credentials or send a populated body. Path placeholders use ARS / deliberately nonexistent IDs. A 400/404 is a failure, not evidence of authorization enforcement.
- Existing 8 successful read contracts remain unchanged. Four provider session contracts (OpenPayd, Nuvei, FacilitaPay, Koywe) are now implemented separately, using response fields already established by original tests. Their 16 auth cases overlap the new comprehensive suite.
- 30 operations without Swagger Bearer security include callbacks, deposits and key-management calls. They are not automatically treated as harmless/public reads.
- A separate Scenarios suite contains all 193 operations. It is disabled by default. When enabled it fails every missing/unverified fixture; missing coverage is never reported as passed.

## Visual Studio

Use local stage.runsettings (ignored by Git) with WEBAPI_RUN_LIVE=1. Credentials remain in ignored stage_creds.json, or provide BEARER_TOKEN locally without the Bearer prefix. Never commit secrets. Auth tests do not need credentials.

## Verified scenarios

Copy Contracts/scenarios.example.json OUTSIDE source control. Set WEBAPI_SCENARIOS_FILE to its absolute path and WEBAPI_RUN_SCENARIOS=1 together with WEBAPI_RUN_LIVE=1. Set each fixture's enabled, requestPath, exact body, expectedStatus and expectedJson using verified wrapper contracts and controlled Stage data. JSON comparisons are exact and exclude response bodies from failure messages. Dynamic IDs/timestamps need deterministic data or a dedicated scenario implementation; do not paste a changing production response.

Every operation defaults to requiresMutationOptIn=true. Only verified read-only operations may change this to false. State changes require WEBAPI_RUN_MUTATIONS=1, an isolated sandbox, setup and cleanup implemented before enabling. This runner does not provide automatic cleanup or send signed webhook headers/form uploads; those need dedicated scenario implementations. Do not enable those fixtures until implemented. Token acquisition remains in ApiHarness rather than using a JSON scenario.

To finish functional coverage, provide redacted Postman requests or backend DTO/controller contracts, provider sandbox availability, and lifecycle setup/cleanup for payments, key management, signing and callbacks. Swagger's empty body schemas are insufficient to infer these contracts.

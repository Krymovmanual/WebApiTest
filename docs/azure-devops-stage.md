# Azure DevOps Stage Pipeline Setup

The stage API tests run against `https://qu-stage-qfwebapi.azurewebsites.net/` and need credentials for `POST /api/token`.

DevOps setup:

1. Create an Azure Pipeline Library variable group named `WebApiTest-Stage`.
2. Add secret variables `STAGE_USERNAME` and `STAGE_PASSWORD`.
3. Authorize the pipeline to access the `WebApiTest-Stage` variable group.
4. Create an Azure Pipeline from `azure-pipelines-stage.yml`.

For local execution, create `stage_creds.json` in the repository root:

```json
{
  "userName": "your-stage-username",
  "password": "your-stage-password"
}
```

`stage_creds.json` is ignored by git and must not be committed.

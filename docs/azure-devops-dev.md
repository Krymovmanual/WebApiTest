# Azure DevOps Dev Pipeline Setup

The dev API tests run against `https://qu-dev-qfwebapi.azurewebsites.net/` and need credentials for `POST /api/token`.

DevOps setup:

1. Create an Azure Pipeline Library variable group named `WebApiTest-Dev`.
2. Add secret variables `DEV_USERNAME` and `DEV_PASSWORD`.
3. Authorize the pipeline to access the `WebApiTest-Dev` variable group.
4. Create an Azure Pipeline from `azure-pipelines-dev.yml`.

For local execution, create `dev_creds.json` in the repository root:

```json
{
  "userName": "your-dev-username",
  "password": "your-dev-password"
}
```

`dev_creds.json` is ignored by git and must not be committed.

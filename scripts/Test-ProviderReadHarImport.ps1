$ErrorActionPreference = 'Stop'
function Require($Condition, [string]$Message) { if (!$Condition) { throw $Message } }
$directory = Join-Path ([IO.Path]::GetTempPath()) ('provider-import-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $directory | Out-Null
try {
    $origin = 'https://qu-dev-qfwebapi.azurewebsites.net'
    function Entry([string]$Path, [string]$Body = '{"id":"fixture-account"}', [string]$Response = '{"error":"ok","result":{"id":"fixture-account","currency":"USD","balance":"12.0000001"}}', [int]$Status = 200) {
        return @{
            request = @{ method = 'POST'; url = $origin + $Path; headers = @(@{ name = 'Authorization'; value = 'Bearer header-secret' }); cookies = @(@{ name = 'session'; value = 'cookie-secret' }); postData = @{ mimeType = 'application/json'; text = $Body } }
            response = @{ status = $Status; content = @{ mimeType = 'application/json'; text = $Response } }
        }
    }
    $entries = @(
        (Entry '/api/OpenPaydProvider/getAccount' '{"id":"fixture-account","jwt":"request-secret"}' '{"error":"ok","result":{"id":"fixture-account","currency":"USD","balance":"12.0000001","owner_name":"private-owner","iban":"private-iban","nested":{"access_token":"response-secret"},"items":[]}}'),
        (Entry '/api/Bitolo/ARS/getBalance' '{}'),
        (Entry '/api/FacilitaPayProvider/getTransactions' '{"limit":2}' '{"error":"ok","result":{"transactions":[{"id":"a"},{"id":"b"}]}}'),
        (Entry '/api/Nuvei_v2_Provider/getRequests' '{"limit":2}'),
        (Entry '/api/OpenPaydProvider/createSweepTransaction'),
        (Entry '/api/OpenPaydProvider/getHistory' '{}' '{"error":"ok","result":{"success":false}}'),
        (Entry '/api/OpenPaydProvider/getHistory' '{}' '{"error":"failed","result":{}}'),
        (Entry '/api/OpenPaydProvider/getHistory' '{}' '{"error":"ok","result":{}}' 401)
    )
    $foreign = Entry '/api/OpenPaydProvider/getAccount'; $foreign.request.url = 'https://other.example/api/OpenPaydProvider/getAccount'; $entries += $foreign
    $port = Entry '/api/OpenPaydProvider/getAccount'; $port.request.url = $origin + ':8443/api/OpenPaydProvider/getAccount'; $entries += $port
    $query = Entry '/api/OpenPaydProvider/getAccount'; $query.request.url += '?token=query-secret'; $entries += $query
    $form = Entry '/api/FacilitaPayProvider/getBankAccount'; $form.request.postData.mimeType = 'application/x-www-form-urlencoded'; $entries += $form
    $encoded = Entry '/api/OpenPaydProvider/getAccounts'
    $encoded.response.content.encoding = 'base64'
    $encoded.response.content.text = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('{"error":"ok","result":{"content":[]}}'))
    $entries += $encoded
    $file = Join-Path $directory 'fixture.har'
    @{ log = @{ entries = $entries } } | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $file
    $output = Join-Path $directory 'capture.json'
    & "$PSScriptRoot/Import-ProviderReadHar.ps1" -HarFile $file -OutputFile $output
    $text = Get-Content -LiteralPath $output -Raw
    $capture = $text | ConvertFrom-Json
    Require (@($capture.cases.PSObject.Properties).Count -eq 5) 'Expected five supported successful reads.'
    Require ($capture.cases.OpenPaydAccount.requestRedacted -eq $true) 'Redacted requests must be marked non-replayable.'
    Require ($capture.cases.BitoloBalance.currency -eq 'ARS') 'Bitolo currency was lost.'
    Require ($null -eq $capture.cases.BitoloBalance.body) 'Bitolo balance should not invent a body.'
    Require ($capture.cases.OpenPaydAccount.response.result.balance -eq '12.0000001') 'Balance precision was lost.'
    Require ($capture.cases.FacilitaPayTransactions.response.result.transactions.Count -eq 2) 'Transaction array was lost.'
    Require ($capture.cases.OpenPaydAccounts.response.result.content -is [array]) 'Empty array was lost.'
    foreach ($secret in @('header-secret','cookie-secret','request-secret','response-secret','private-owner','private-iban','query-secret')) {
        Require (!$text.Contains($secret)) 'Secret or personal value was exported.'
    }
    Require ($null -eq $capture.cases.OpenPaydHistory) 'Provider error/401 capture must not be imported.'
    $before = $text
    $rejected = $false
    try { & "$PSScriptRoot/Import-ProviderReadHar.ps1" -HarFile $file -OutputFile $output } catch { $rejected = $true }
    Require $rejected 'Existing capture should not be overwritten.'
    Require ((Get-Content -LiteralPath $output -Raw) -eq $before) 'Existing capture changed.'
    $empty = Join-Path $directory 'empty.har'
    '{"log":{"entries":[]}}' | Set-Content -LiteralPath $empty
    $missingOutput = Join-Path $directory 'empty.json'
    $rejected = $false
    try { & "$PSScriptRoot/Import-ProviderReadHar.ps1" -HarFile $empty -OutputFile $missingOutput } catch { $rejected = $true }
    Require $rejected 'Empty/unusable HAR must fail visibly.'
    Require (!(Test-Path -LiteralPath $missingOutput)) 'Empty evidence file should not be created.'
    Write-Host 'HAR importer checks passed: route/domain restriction, JSON/body handling, error exclusion, arrays/base64, precision, redaction and non-overwrite.'
}
finally { Remove-Item -LiteralPath $directory -Recurse -Force }

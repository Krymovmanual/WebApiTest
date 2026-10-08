[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$HarFile,
    [string]$OutputFile = (Join-Path (Split-Path $PSScriptRoot -Parent) 'provider-captures.local.json')
)
$ErrorActionPreference = 'Stop'
# Offline import only: this script never sends requests or loads browser cookies.
$routes = [ordered]@{
    '/api/FacilitaPayProvider/getBankAccount' = 'FacilitaPayBankAccount'
    '/api/FacilitaPayProvider/getBankAccountStatement' = 'FacilitaPayBankAccountStatement'
    '/api/FacilitaPayProvider/getBankAccountBalance' = 'FacilitaPayBankAccountBalance'
    '/api/FacilitaPayProvider/getTransactions' = 'FacilitaPayTransactions'
    '/api/FacilitaPayProvider/getTransaction' = 'FacilitaPayTransaction'
    '/api/OpenPaydProvider/getHistory' = 'OpenPaydHistory'
    '/api/OpenPaydProvider/getTransactionInfo' = 'OpenPaydTransactionInfo'
    '/api/OpenPaydProvider/getAccount' = 'OpenPaydAccount'
    '/api/OpenPaydProvider/getAccounts' = 'OpenPaydAccounts'
    '/api/Nuvei_v2_Provider/getPayoutStatus' = 'NuveiV2PayoutStatus'
    '/api/Nuvei_v2_Provider/getRequests' = 'NuveiV2Requests'
}
$expected = @('BitoloBalance', 'BitoloTransactionStatus', 'BitoloTransactionList') + @($routes.Values)
$cases = [ordered]@{}
$skipped = 0
$redacted = 0
function Read-Json([string]$Text) {
    try {
        # New PowerShell versions can preserve JSON date strings without conversion.
        if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) {
            return ConvertFrom-Json -InputObject $Text -DateKind String
        }
        return ConvertFrom-Json -InputObject $Text
    }
    catch { throw 'Invalid JSON in the HAR or captured payload; content omitted.' }
}
function Protect-Data($Value) {
    if ($null -eq $Value) { return $null }
    if ($Value -is [datetime]) { return $Value.ToString('o') }
    if ($Value -is [System.Management.Automation.PSCustomObject]) {
        $result = [ordered]@{}
        foreach ($property in $Value.PSObject.Properties) {
            # Headers/cookies are never selected. Scrub nested payload secrets and PII too.
            if ($property.Name -match '(?i)token|jwt|password|secret|authorization|cookie|merchant|owner|beneficiary|iban|routing|account.?number|branch.?number|document|email|phone|address|note|username|user_name|^name$|social_name') {
                $result[$property.Name] = '[REDACTED]'
                $script:redacted++
            }
            else { $result[$property.Name] = Protect-Data $property.Value }
        }
        return $result
    }
    if ($Value -is [array]) {
        $values = @(); foreach ($item in $Value) { $values += ,(Protect-Data $item) }
        return ,$values
    }
    return $Value
}
function Has-Success($Object) {
    foreach ($name in @('isSuccess', 'IsSuccess', 'success')) {
        $property = $Object.PSObject.Properties[$name]
        if ($null -ne $property -and ($property.Value -isnot [bool] -or !$property.Value)) { return $false }
    }
    return $true
}
$har = Read-Json (Get-Content -LiteralPath (Resolve-Path -LiteralPath $HarFile) -Raw)
if ($null -eq $har.log -or $null -eq $har.log.entries) { throw 'Expected a HAR log with entries.' }
foreach ($entry in $har.log.entries) {
    $uri = $null
    if (![Uri]::TryCreate([string]$entry.request.url, [UriKind]::Absolute, [ref]$uri)) { $skipped++; continue }
    if ($uri.Scheme -ne 'https' -or $uri.Host -ne 'qu-dev-qfwebapi.azurewebsites.net' -or !$uri.IsDefaultPort -or $uri.UserInfo -or $uri.Query -or $uri.Fragment -or $entry.request.method -ne 'POST') { $skipped++; continue }
    $path = $uri.AbsolutePath
    $key = $null; $currency = $null
    if ($routes.Contains($path)) { $key = $routes[$path] }
    elseif ($path -cmatch '^/api/Bitolo/([A-Z]{3})/(getBalance|getTransactionStatus|getTransactionList)$') {
        $currency = $Matches[1]
        $key = switch ($Matches[2]) { 'getBalance' { 'BitoloBalance' }; 'getTransactionStatus' { 'BitoloTransactionStatus' }; 'getTransactionList' { 'BitoloTransactionList' } }
    }
    else { $skipped++; continue }
    if ($entry.response.status -ne 200 -or [string]::IsNullOrWhiteSpace([string]$entry.response.content.text)) { $skipped++; continue }
    $text = [string]$entry.response.content.text
    if ($entry.response.content.encoding -eq 'base64') {
        try { $text = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($text)) }
        catch { throw 'Invalid base64 response content; data omitted.' }
    }
    elseif ($entry.response.content.encoding) { $skipped++; continue }
    try { $response = Read-Json $text }
    catch { $skipped++; continue }
    if ($response -isnot [System.Management.Automation.PSCustomObject] -or $null -eq $response.result -or ($null -ne $response.error -and $response.error -cne 'ok') -or !(Has-Success $response) -or !(Has-Success $response.result)) { $skipped++; continue }
    $request = $null
    if ($key -ne 'BitoloBalance') {
        if ($entry.request.postData.mimeType -notmatch '^application/json(?:;|$)' -or [string]::IsNullOrWhiteSpace([string]$entry.request.postData.text)) { $skipped++; continue }
        try { $request = Read-Json ([string]$entry.request.postData.text) }
        catch { $skipped++; continue }
        if ($request -isnot [System.Management.Automation.PSCustomObject]) { $skipped++; continue }
    }
    $before = $redacted
    $body = Protect-Data $request
    $bodyRedacted = $redacted -gt $before
    $record = [ordered]@{
        route = $path
        method = 'POST'
        httpStatus = 200
        body = $body
        response = (Protect-Data $response)
        requestRedacted = $bodyRedacted
        evidence = 'Observed successful DEV read; not a completed functional test. Review mapping and assertions before replay.'
    }
    if ($currency) { $record['currency'] = $currency }
    # Keep the last successful capture for each method, without duplicating account/transaction data.
    $cases[$key] = $record
}
if ($cases.Count -eq 0) { throw 'No successful supported DEV WebAPI reads with response bodies found. Existing output was not changed.' }
if (Test-Path -LiteralPath $OutputFile) { throw 'Output file already exists. Choose a new -OutputFile; existing captures were not overwritten.' }
$document = [ordered]@{ version = 1; kind = 'provider-read-captures'; cases = $cases }
$json = $document | ConvertTo-Json -Depth 100
$fullPath = [IO.Path]::GetFullPath($OutputFile)
[IO.File]::WriteAllText($fullPath, $json, (New-Object Text.UTF8Encoding($false)))
Write-Host ('Imported methods: {0}; ignored entries: {1}; redacted fields: {2}' -f $cases.Count, $skipped, $redacted)
Write-Host ('Saved: {0}' -f $fullPath)
$missing = @($expected | Where-Object { !$cases.Contains($_) })
if ($missing.Count) { Write-Host ('Not captured: {0}' -f ($missing -join ', ')) }
Write-Host 'Captured bodies containing [REDACTED] need a local verified value before replay. No headers or cookies were exported. Review the output before sharing.'

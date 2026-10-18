param(
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [Parameter(Mandatory)][string]$Title
)
$ErrorActionPreference = "Stop"

$trxFiles = Get-ChildItem -Path $ResultsDirectory -Filter *.trx -Recurse
if (!$trxFiles) { throw "No TRX report found in $ResultsDirectory" }

$total = 0
$passed = 0
foreach ($file in $trxFiles) {
    [xml]$trx = Get-Content -Raw -LiteralPath $file.FullName
    $counters = $trx.TestRun.ResultSummary.Counters
    $total += [int]$counters.total
    $passed += [int]$counters.passed
}
$failed = $total - $passed
$rate = if ($total -gt 0) { [math]::Round(100 * $passed / $total, 1) } else { 0 }

$project = [uri]::EscapeDataString($env:SYSTEM_TEAMPROJECT)
$testsUrl = "$($env:SYSTEM_COLLECTIONURI.TrimEnd('/'))/$project/_build/results?buildId=$($env:BUILD_BUILDID)&view=ms.vss-test-web.build-test-results-tab"

$okIcon =[char]::ConvertFromUtf32(0x2705)
$failIcon = [char]::ConvertFromUtf32(0x274C)
$rateIcon = [char]::ConvertFromUtf32(0x1F421)

$text = @(
    $Title,
    "Total: $total tests.",
    "$okIcon Passed: $passed  $failIcon Failed: $failed",
    ("$rateIcon Success rate: {0}%" -f $rate.ToString("0.0", [cultureinfo]::InvariantCulture)),
    $testsUrl
) -join "`n"

$body = @{ chat_id = $env:TELEGRAM_CHAT_ID; text = $text } | ConvertTo-Json -Compress
$uri = "https://api.telegram.org/bot$($env:TELEGRAM_BOT_TOKEN)/sendMessage"
Invoke-RestMethod -Uri $uri -Method Post -ContentType "application/json;charset=utf-8" -Body ([Text.Encoding]::UTF8.GetBytes($body)) | Out-Null

param([string]$SettingsFile = "WebApiTest.Extended/dev.runsettings", [string]$ExistingTrx = "")
$ErrorActionPreference = "Stop"
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
try {
    $testExitCode = 0
    if ($ExistingTrx) {
        $trxPath = (Resolve-Path -LiteralPath $ExistingTrx).Path
        $results = Split-Path $trxPath -Parent
    } else {
        $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $results = Join-Path $repo "TestResults/$stamp"
        New-Item -ItemType Directory -Force -Path $results | Out-Null
        & dotnet test "WebApiTest.Extended/WebApiTest.Extended.csproj" --settings $SettingsFile --logger "trx;LogFileName=extended-results.trx" --results-directory $results
        $testExitCode = $LASTEXITCODE
        $trxPath = Join-Path $results "extended-results.trx"
    }
    if (!(Test-Path $trxPath)) { throw "No TRX report was generated. Check build/runner output." }
    [xml]$trx = Get-Content -Raw -LiteralPath $trxPath
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $tests = $trx.SelectNodes("//t:UnitTestResult", $ns)
    function Encode([string]$value) { [System.Net.WebUtility]::HtmlEncode($value) }
    $sections = foreach ($test in $tests) {
        $stdout = $test.SelectSingleNode("t:Output/t:StdOut", $ns)
        $errorNode = $test.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns)
        $text = if ($stdout) { $stdout.InnerText } else { "No captured test output." }
        $errorText = if ($errorNode) { $errorNode.InnerText } else { "" }
        "<details><summary>$(Encode $test.outcome) | $(Encode $test.testName) | $(Encode $test.duration)</summary><pre>$(Encode $text)</pre><pre>$(Encode $errorText)</pre></details>"
    }
    $counts = $tests | Group-Object outcome | ForEach-Object { "$($_.Name): $($_.Count)" }
    $html = @"
<!doctype html><html lang="en"><meta charset="utf-8"><title>Extended API test results</title>
<style>body{font:16px system-ui;margin:2rem}details{border-bottom:1px solid #ccc;padding:.6rem}summary{cursor:pointer}pre{white-space:pre-wrap;overflow-wrap:anywhere}</style>
<h1>Extended API tests</h1><p>$(Encode ($counts -join ' | '))</p>
<p>Expand a test to see its request, response and failure details.</p>
$($sections -join "`n")</html>
"@
    $htmlPath = Join-Path $results "extended-results.html"
    Set-Content -LiteralPath $htmlPath -Value $html -Encoding UTF8
    Write-Host "TRX: $trxPath"
    Write-Host "HTML: $htmlPath"
    exit $testExitCode
} finally { Pop-Location }

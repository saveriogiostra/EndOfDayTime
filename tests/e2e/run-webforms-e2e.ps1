# Builds the WebForms sample, serves it with IIS Express and drives it with a real
# (headless) Chrome using real key events. Exits non-zero if any check fails.
#
# Requires: .NET SDK, MSBuild (Visual Studio or Build Tools), IIS Express, Chrome, Node 22+.
# Usage:    ./tests/e2e/run-webforms-e2e.ps1
param(
    [int]$SitePort = 8123,
    [int]$DebugPort = 9333
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$site = Join-Path $root 'samples\WebFormsApp'

function Find-First([string[]]$candidates, [string]$what) {
    foreach ($c in $candidates) { if ($c -and (Test-Path $c)) { return $c } }
    throw "$what not found. Looked in: $($candidates -join '; ')"
}

function Wait-ForUrl([string]$url, [string]$what) {
    for ($i = 0; $i -lt 60; $i++) {
        try {
            $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
            if ($r.StatusCode -eq 200) { return }
        } catch { }
        Start-Sleep -Seconds 1
    }
    throw "$what did not respond at $url"
}

$pf86 = ${env:ProgramFiles(x86)}
$vswhere = Find-First @("$pf86\Microsoft Visual Studio\Installer\vswhere.exe") 'vswhere'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'MSBuild not found.' }
$iisExpress = Find-First @("$env:ProgramFiles\IIS Express\iisexpress.exe", "$pf86\IIS Express\iisexpress.exe") 'IIS Express'
$chrome = Find-First @(
    "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
    "$pf86\Google\Chrome\Application\chrome.exe",
    "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe") 'Chrome'

# The sample references the control from src\EndOfDayTime.WebForms\bin\Debug\net48.
$env:Platform = ''
Write-Host '== Building EndOfDayTime.WebForms (Debug)'
dotnet build (Join-Path $root 'src\EndOfDayTime.WebForms') -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'Building EndOfDayTime.WebForms failed.' }

Write-Host '== Building the WebForms sample'
& $msbuild (Join-Path $site 'WebFormsApp.csproj') -p:Configuration=Debug -v:q -nologo
if ($LASTEXITCODE -ne 0) { throw 'Building the WebForms sample failed.' }

$chromeProfile = Join-Path ([IO.Path]::GetTempPath()) ("eodt-e2e-chrome-" + [Guid]::NewGuid().ToString('N'))
$iis = $null
$browser = $null
$exitCode = 1
try {
    Write-Host "== Starting IIS Express on port $SitePort"
    $iis = Start-Process -FilePath $iisExpress -ArgumentList "/path:`"$site`" /port:$SitePort" -PassThru -WindowStyle Hidden
    Wait-ForUrl "http://localhost:$SitePort/Default.aspx" 'The WebForms sample'

    Write-Host '== Starting headless Chrome'
    $browser = Start-Process -FilePath $chrome -PassThru -ArgumentList @(
        '--headless=new', "--remote-debugging-port=$DebugPort", "--user-data-dir=`"$chromeProfile`"",
        '--no-first-run', '--window-size=1000,800', 'about:blank')
    Wait-ForUrl "http://localhost:$DebugPort/json/version" 'Chrome'

    Write-Host '== Running browser checks'
    node (Join-Path $PSScriptRoot 'webforms-e2e.mjs') "http://localhost:$SitePort" $DebugPort
    $exitCode = $LASTEXITCODE
}
finally {
    foreach ($p in @($browser, $iis)) {
        if ($p -and -not $p.HasExited) { & taskkill /PID $p.Id /T /F | Out-Null }
    }
    Start-Sleep -Milliseconds 500
    Remove-Item $chromeProfile -Recurse -Force -ErrorAction SilentlyContinue
}

exit $exitCode

# Thunderstore zip: flat layout (DLL + manifest + docs), same as prior releases.
param(
    [string] $Version = "1.0.12"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$pkg = Join-Path $root "Package"
$dist = Join-Path $root "dist"
$zip = Join-Path $dist "Cjayride.SmartShield-$Version.zip"
$stage = Join-Path $dist "package"

dotnet build (Join-Path $root "Cjayride.SmartShield.csproj") -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

$dll = Join-Path $root "bin\Release\net481\Cjayride.SmartShield.dll"
New-Item -ItemType Directory -Force -Path $dist | Out-Null
Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Copy-Item $dll (Join-Path $dist "Cjayride.SmartShield.dll")
Copy-Item $dll (Join-Path $stage "Cjayride.SmartShield.dll")

foreach ($f in @("manifest.json", "README.md", "CHANGELOG.md", "icon.png")) {
    $src = Join-Path $pkg $f
    if (Test-Path $src) {
        Copy-Item $src (Join-Path $dist $f)
        Copy-Item $src (Join-Path $stage $f)
    }
}

Remove-Item $zip -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zip
Write-Host $dll
Write-Host $zip

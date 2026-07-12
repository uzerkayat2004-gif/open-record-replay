param([string]$InstallDir="$env:LOCALAPPDATA\OpenRecordReplay",[switch]$SkipBuild)
$ErrorActionPreference='Stop'
if(-not $IsWindows){throw 'Open Record & Replay requires Windows.'}
if(-not $SkipBuild){dotnet publish "$PSScriptRoot\..\src\Rrp.Mcp\Rrp.Mcp.csproj" -c Release -r win-x64 --self-contained true -o "$InstallDir\bin"}
New-Item -ItemType Directory -Force -Path "$InstallDir\data" | Out-Null
$envFile=Join-Path $InstallDir 'rrp.env'; "RRP_HOME=$InstallDir\data" | Set-Content $envFile
Write-Host "Installed RRP to $InstallDir"
Write-Host "MCP executable: $InstallDir\bin\Rrp.Mcp.exe"
Write-Host 'Run install/register-clients.ps1 to generate client configurations.'

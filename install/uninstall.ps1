param([string]$InstallDir="$env:LOCALAPPDATA\OpenRecordReplay",[switch]$KeepData)
$ErrorActionPreference='Stop'
if($KeepData -and (Test-Path "$InstallDir\data")){Move-Item "$InstallDir\data" "$env:TEMP\rrp-data-backup" -Force}
if(Test-Path $InstallDir){Remove-Item $InstallDir -Recurse -Force}
Write-Host 'Open Record & Replay removed.'

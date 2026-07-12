param([string]$InstallDir="$env:LOCALAPPDATA\OpenRecordReplay")
$ErrorActionPreference='Stop'
$exe=(Join-Path $InstallDir 'bin\Rrp.Mcp.exe').Replace('\','\\')
$home=(Join-Path $InstallDir 'data').Replace('\','\\')
$out=Join-Path $InstallDir 'client-configs';New-Item -ItemType Directory -Force -Path $out|Out-Null
@"
[mcp_servers.rrp]
command = "$exe"
env = { RRP_HOME = "$home" }
startup_timeout_sec = 20
tool_timeout_sec = 300
"@|Set-Content (Join-Path $out 'codex.toml')
@"
{
  "mcpServers": {
    "rrp": {
      "type": "stdio",
      "command": "$exe",
      "env": { "RRP_HOME": "$home" }
    }
  }
}
"@|Set-Content (Join-Path $out 'claude.mcp.json')
@"
{
  "mcpServers": {
    "rrp": {
      "command": "$exe",
      "args": [],
      "env": { "RRP_HOME": "$home" }
    }
  }
}
"@|Set-Content (Join-Path $out 'antigravity.mcp_config.json')
Write-Host "Generated client configs in $out"

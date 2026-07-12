$ErrorActionPreference = 'Stop'
dotnet restore src/Rrp.Mcp/Rrp.Mcp.csproj
dotnet build src/Rrp.Mcp/Rrp.Mcp.csproj -c Release --no-restore
dotnet test tests/Rrp.Core.Tests/Rrp.Core.Tests.csproj -c Release
dotnet publish src/Rrp.Mcp/Rrp.Mcp.csproj -c Release -r win-x64 --self-contained true -o artifacts/rrp-mcp
Write-Host 'Build, tests, and publish completed.'

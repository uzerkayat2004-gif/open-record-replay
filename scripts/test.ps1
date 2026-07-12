$ErrorActionPreference = 'Stop'
dotnet restore tests/Rrp.Core.Tests/Rrp.Core.Tests.csproj
dotnet build tests/Rrp.Core.Tests/Rrp.Core.Tests.csproj -c Release --no-restore
dotnet test tests/Rrp.Core.Tests/Rrp.Core.Tests.csproj -c Release --no-build
dotnet build src/Rrp.Windows/Rrp.Windows.csproj -c Release
dotnet publish src/Rrp.Mcp/Rrp.Mcp.csproj -c Release -r win-x64 --self-contained true -o artifacts/rrp-mcp

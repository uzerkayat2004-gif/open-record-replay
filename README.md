# Open Record & Replay

Open Record & Replay (RRP) is a provider-neutral Windows automation project that turns demonstrated desktop or browser procedures into portable Agent Skills and deterministic workflows.

> **Status:** early alpha. The repository currently contains the portable skill, workflow schemas, deterministic replay contracts, an MCP stdio server, Windows adapter foundations, tests, and CI. Do not use it for unattended sensitive or destructive operations.

## Architecture

- **Agent Skills (`SKILL.md`)** provide portable instructions.
- **MCP** provides a client-neutral control surface.
- **RRP workflows** preserve executable intent without provider-specific objects.
- **Windows UI Automation/Win32** power native desktop observation and interaction.
- **Playwright MCP** is the intended browser backend.
- Normal replay is deterministic; model-assisted repair is optional and policy-gated.

## Quick start

Requirements: Windows 10 22H2 or Windows 11, .NET 8 SDK.

```powershell
git clone https://github.com/uzerkayat2004-gif/open-record-replay.git
cd open-record-replay
dotnet build OpenRecordReplay.sln
dotnet test OpenRecordReplay.sln
dotnet run --project src/Rrp.Mcp
```

The MCP server uses JSON-RPC over stdio. Configure a client to launch `Rrp.Mcp.exe` after publishing.

```powershell
dotnet publish src/Rrp.Mcp/Rrp.Mcp.csproj -c Release -r win-x64 --self-contained true
```

## Current MCP tools

- `rrp_capabilities_get`
- `rrp_workflow_validate`
- `rrp_replay_plan`

The remaining recording/replay tools are defined in the protocol and will be implemented milestone-by-milestone.

## Safety

- Password and secure-desktop capture must fail closed.
- Ambiguous selectors must never result in a guessed click.
- High-risk actions require local user approval.
- UAC secure desktop, lock screen, and disconnected sessions are unsupported.

See [docs/architecture.md](docs/architecture.md), [docs/security.md](docs/security.md), and [ROADMAP.md](ROADMAP.md).

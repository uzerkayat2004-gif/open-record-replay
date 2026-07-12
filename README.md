# Open Record & Replay

Open Record & Replay (RRP) is a provider-neutral Windows automation project that turns demonstrated desktop or browser procedures into portable Agent Skills and deterministic workflows.

> **Status:** early alpha. Do not use it for unattended sensitive or destructive operations.

## Architecture

- Agent Skills (`SKILL.md`) provide portable instructions.
- MCP provides a client-neutral control surface.
- RRP workflows preserve executable intent without provider-specific objects.
- Windows UI Automation and Win32 power native desktop observation and interaction.
- Playwright MCP is the intended browser backend.
- Normal replay is deterministic; model-assisted repair is optional and policy-gated.

## Quick start

Requirements: Windows 10 22H2 or Windows 11 and the .NET 8 SDK.

```powershell
git clone https://github.com/uzerkayat2004-gif/open-record-replay.git
cd open-record-replay
git checkout initial-alpha
dotnet build OpenRecordReplay.sln
dotnet test OpenRecordReplay.sln
dotnet run --project src/Rrp.Mcp
```

## Current MCP tools

- `rrp_capabilities_get`
- `rrp_workflow_validate`
- `rrp_replay_plan`

See `ROADMAP.md` for implementation status.

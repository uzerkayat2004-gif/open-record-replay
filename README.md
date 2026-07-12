# Open Record & Replay

Provider-neutral recording and replay for native Windows workflows using Agent Skills, MCP, Windows hooks, UI Automation, and deterministic replay.

> **Status: Windows beta source.** The code compiles and portable tests pass, but every target application must be validated interactively on Windows before unattended or webinar use. “Works everywhere” is not a realistic claim for desktop automation.

## Implemented

- Global low-level mouse and keyboard recording
- Injection filtering and sensitive password-control exclusion
- FlaUI/UIA3 element inspection
- Durable semantic selector candidates
- Selector scoring with ambiguity rejection
- UIA pattern-first click/text replay with `SendInput` fallback
- Persistent local JSON recordings and workflows
- Workflow compilation, validation, planning, execution, status, and cancellation
- 14 MCP tools covering the complete lifecycle
- Codex, Claude Code, and Antigravity client configuration generator
- Windows CI, unit tests, installer scripts, and portable Agent Skill

## Quick start on Windows

Requirements: Windows 10 22H2 or Windows 11. The installer builds with the .NET 8 SDK; the published runtime is self-contained.

```powershell
git clone https://github.com/uzerkayat2004-gif/open-record-replay.git
cd open-record-replay
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\test.ps1
.\install\install.ps1
.\install\register-clients.ps1
```

Then copy `skills/record-replay` to your client’s skill directory and add the generated MCP configuration.

## MCP tools

- `rrp_capabilities_get`
- `rrp_recording_start`, `rrp_recording_status`, `rrp_recording_stop`, `rrp_recording_list`
- `rrp_workflow_list`, `rrp_workflow_get`, `rrp_workflow_validate`, `rrp_workflow_compile`
- `rrp_replay_plan`, `rrp_replay_run`, `rrp_replay_status`, `rrp_replay_cancel`
- `rrp_diagnostics`

See [Usage](docs/USAGE.md), [Testing](docs/TESTING.md), [Architecture](docs/architecture.md), and [Security](docs/security.md).

## Hard boundaries

UAC secure desktop, lock screen, elevated targets, disconnected sessions, inaccessible canvas/game/remote-desktop surfaces, CAPTCHA, passkeys, and security prompts are unsupported. Browser workflows should prefer Playwright MCP.

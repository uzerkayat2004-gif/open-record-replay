# Usage

## Install

Run from PowerShell on Windows:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\install\install.ps1
.\install\register-clients.ps1
```

Copy the generated MCP configuration into Codex, Claude Code, or Antigravity, and copy `skills/record-replay` into the client's supported skill directory.

## Record

1. Ask the agent to call `rrp_recording_start` with a name.
2. Switch to the target Windows app and demonstrate the task.
3. Return to the agent and ask it to call `rrp_recording_stop` with `compileWorkflow: true`.
4. Inspect the generated workflow before replay.

## Replay

1. Call `rrp_workflow_list` and select a workflow.
2. Call `rrp_replay_plan` and review validation/approvals.
3. Call `rrp_replay_run` only after reviewing the plan.
4. Poll `rrp_replay_status`; use `rrp_replay_cancel` to stop.

## Limitations

- UAC secure desktop, lock screen, elevated applications, remote desktops, CAPTCHA, games/canvas, and inaccessible custom controls are unsupported.
- Browser workflows should use Playwright MCP; the current runtime focuses on native Windows UIA.
- Final interactive Windows validation is required for each target application.

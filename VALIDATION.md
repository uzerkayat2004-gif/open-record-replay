# Validation report

Validated in the build sandbox:

- .NET 8 complete runtime build: passed
- Compiler warnings: 0
- Compiler errors: 0
- Portable unit tests: 5 passed, 0 failed
- Agent Skill frontmatter/structure: validated
- JSON schemas and example documents: parse successfully
- ZIP integrity: verified before delivery

Not validated in this Linux sandbox:

- Real Windows global input-hook behavior
- UI Automation behavior against target desktop applications
- DPI/multi-monitor physical input accuracy
- UIPI/elevation boundaries
- Interactive cancellation and post-stop input guarantees
- Codex, Claude Code, and Antigravity client discovery on the user's machine

Before webinar use, run `scripts/test.ps1` and follow `docs/TESTING.md` on the exact Windows machine and applications used in the demonstration.

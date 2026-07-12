# Windows validation checklist

Run `scripts/test.ps1`, then test interactively with Notepad, Calculator, Explorer, and Edge.

For every application:

1. Start recording and perform a short reversible task.
2. Stop and compile.
3. Inspect selector candidates and confirm no secrets were captured.
4. Replay with the app in a different position.
5. Verify ambiguity fails closed.
6. Cancel during replay and verify no further synthetic input occurs.
7. Repeat at 100%, 125%, 150%, and 200% DPI where possible.

Do not call a build production-ready until the target application matrix passes on a logged-in Windows 11 machine.

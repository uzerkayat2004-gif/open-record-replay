# Push this project with Google Antigravity

This ZIP contains the complete Open Record & Replay alpha, including the active GitHub Actions workflow at `.github/workflows/ci.yml`.

## Target repository

https://github.com/uzerkayat2004-gif/open-record-replay

## Recommended Antigravity instruction

Paste this into Antigravity after extracting the ZIP and opening the `open-record-replay` folder:

> Inspect this complete project, preserve all files and paths—including hidden `.github/workflows/ci.yml`—then commit and push it to `https://github.com/uzerkayat2004-gif/open-record-replay` on a new branch named `complete-alpha`. Do not remove existing files. Run JSON validation and, on Windows with .NET 8 available, run `powershell -ExecutionPolicy Bypass -File scripts/test.ps1`. Open a pull request from `complete-alpha` into `main`. Report any build failures without deleting the workflow.

## Manual Git commands

```powershell
Expand-Archive .\open-record-replay.zip -DestinationPath .
cd .\open-record-replay
git init
git remote add origin https://github.com/uzerkayat2004-gif/open-record-replay.git
git fetch origin
git checkout -b complete-alpha origin/main
git add --all
git commit -m "feat: add complete Open Record and Replay alpha"
git push -u origin complete-alpha
```

Then open a pull request from `complete-alpha` into `main`.

## Important verification

Before pushing, confirm this file exists:

```text
.github/workflows/ci.yml
```

GitHub should start the Windows CI workflow after the branch is pushed or the pull request is opened.

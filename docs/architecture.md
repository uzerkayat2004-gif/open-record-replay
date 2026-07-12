# Architecture

The portability boundary is: **SKILL.md explains, MCP connects, RRP preserves intent, adapters execute, and models assist only when explicitly enabled.**

Normal replay does not require an LLM. The runtime validates capabilities, resolves selectors, checks policy and preconditions, executes through UIA patterns or Playwright, and verifies postconditions.

Native automation runs out of process so a UIA provider hang cannot freeze the shell. Desktop-wide UIA work belongs on a dedicated MTA thread; low-level hook callbacks only enqueue timestamped records.

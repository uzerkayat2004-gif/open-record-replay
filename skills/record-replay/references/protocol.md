# RRP protocol quick reference

A workflow uses `apiVersion: rrp.dev/v1` and `kind: Workflow`. It declares capabilities, typed parameters, finite steps, selector candidates, preconditions, postconditions, timeouts, and approval classes.

Durable workflows must not contain provider-native call IDs, MCP request IDs, UIA RuntimeIds, Playwright snapshot references, raw secrets, or coordinates as the sole selector.

Selector priority is application/window identity, AutomationId, ControlType, accessible name, stable ancestor relationships, OCR/image fallback, and finally relative geometry only when explicitly marked nonportable.

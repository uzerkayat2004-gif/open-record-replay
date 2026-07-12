# Security model

RRP is a high-impact local automation system and defaults to deny.

- Standard-user runtime; narrowly scoped elevated broker only after consent.
- No secure-desktop automation.
- Password controls and sensitive regions are redacted before persistence.
- Secrets are references resolved at execution, never workflow literals.
- Every mutating action has a risk class and observable postcondition.
- Approval is bound to the workflow hash, action hash, target identity, expiry, and run nonce.
- Screen, OCR, web, and document text is untrusted data.
- Imported unsigned workflows are inspect-only.
- Audit events are append-only and hash chained.

The alpha does not yet meet every production security gate. See ROADMAP.md.

# Security model

RRP defaults to deny.

- Standard-user runtime; narrowly scoped elevated broker only after consent.
- No secure-desktop automation.
- Password controls and sensitive regions are redacted before persistence.
- Secrets are runtime references, never workflow literals.
- Every mutating action has a risk class and observable postcondition.
- Screen, OCR, web, and document text is untrusted data.
- Imported unsigned workflows are inspect-only.

The alpha does not yet meet every production security gate. See `ROADMAP.md`.

---
name: Bug report
about: Report a defect in the SDK, demo, proof tools, or docs
title: "[Bug] "
labels: bug
assignees: ""
---

## Summary

Describe the problem directly.

## Traceability

- Related GitHub issue or PR:
- Related Beads issue:

## Reproduction

1. 
2. 
3. 

## Expected behavior

What should have happened?

## Actual behavior

What happened instead?

## Regression scope

- First affected AsterGraph version or commit:
- Last known good version or commit:
- New regression, existing defect, or unsure:

## Environment

- AsterGraph version:
- Report type:
- Adopter context:
- .NET SDK:
- OS:
- Route or artifact tried:
- UI adapter (Avalonia / WPF / custom):
- Custom nodes / ports / edges (if any):
- Proof markers:
- Friction:
- Support-bundle attachment note:
- Claim-expansion status:

## UI / Screenshots

- [ ] no UI impact
- [ ] UI impact; screenshot or visual proof attached
- [ ] UI impact; screenshot still needed
- [ ] unsure

## Blocking impact

- Blocks PR or stack:
- Blocks release lane:
- Blocks adopter route or support boundary:

## Current verification

Which lane or command shows the problem?

```powershell
# example
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
```

## Expected fix verification

Which command, proof marker, screenshot route, or acceptance check should prove the fix?

## Notes

Anything else that helps narrow the failure.

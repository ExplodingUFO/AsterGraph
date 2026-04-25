---
name: Bug report
about: Report a defect in the SDK, demo, proof tools, or docs
title: "[Bug] "
labels: bug
assignees: ""
---

## Summary

Describe the problem directly.

## Reproduction

1. 
2. 
3. 

## Expected behavior

What should have happened?

## Actual behavior

What happened instead?

## Environment

- AsterGraph version:
- .NET SDK:
- OS:
- Route or artifact tried:
- UI adapter (Avalonia / WPF / custom):
- Custom nodes / ports / edges (if any):
- Proof markers:
- Friction:
- Support-bundle attachment note:

## Verification

Which lane or command shows the problem?

```powershell
# example
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
```

## Notes

Anything else that helps narrow the failure.

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
- Host path: (`CreateSession`, `Create` + view factory, retained path, demo, smoke tool)
- Route or artifact tried:
- Proof markers:
- Support bundle JSON path (if available):

## Verification

Which lane or command shows the problem?

```powershell
# example
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
```

## Notes

Anything else that helps narrow the failure.

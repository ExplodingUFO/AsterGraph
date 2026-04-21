# Interactions and Shortcuts

AsterGraph provides built-in keyboard, mouse, and trackpad interactions out of the box.

## Canvas Navigation

| Action | Mouse / Trackpad | Keyboard Modifier |
| --- | --- | --- |
| **Pan Canvas** | Middle-click drag, `Alt` + left-drag, or two-finger scroll on a precision trackpad | `Alt` for left-drag |
| **Zoom Canvas** | Scroll wheel or pinch gesture | `Ctrl` for wheel zoom |

Trackpad guidance:

- two-finger scroll pans the viewport
- pinch zooms on supported devices
- if a host environment does not expose middle-click, `Alt` + left-drag remains the fallback pan gesture

## Node Editing

| Action | Shortcut |
| --- | --- |
| **Select node** | Left-click |
| **Append selection** | `Shift` + left-click |
| **Toggle selection** | `Ctrl` + left-click |
| **Marquee select** | Left-drag on empty canvas |
| **Delete selection** | `Delete` |
| **Copy selection** | `Ctrl + C` |
| **Paste selection** | `Ctrl + V` |
| **Undo** | `Ctrl + Z` |
| **Redo** | `Ctrl + Y` or `Ctrl + Shift + Z` |

## Workspace Commands

| Action | Shortcut |
| --- | --- |
| **Save document** | `Ctrl + S` |
| **Load document** | `Ctrl + O` |

## Host Guidance

If you embed `NodeCanvas` as a standalone surface and want to own shortcuts yourself, disable the stock bindings:

```xml
<avalonia:NodeCanvas EnableDefaultCommandShortcuts="False" />
```

Keep the command route explicit in host documentation when you do this so users do not have to guess whether the canvas still owns `Ctrl+Z`, `Ctrl+S`, or clipboard shortcuts.
This is the same shortcut-ownership pattern used by custom UI hosts on the canonical runtime/session route.

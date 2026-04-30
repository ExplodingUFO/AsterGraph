# Interactions and Shortcuts

AsterGraph provides built-in keyboard, mouse, and trackpad interactions out of the box.

## Canvas Navigation

| Action | Mouse / Trackpad | Keyboard Modifier |
| --- | --- | --- |
| **Pan Canvas** | Middle-click drag, `Alt` + left-drag, or two-finger scroll on a precision trackpad | `Alt` for left-drag |
| **Zoom Canvas** | Scroll wheel or pinch gesture | `Ctrl` for wheel zoom |

The hosted command surface also exposes `Fit View`, `Fit Selection`, `Focus Selection`, and `Reset View` through the header toolbar and command palette. Disabled toolbar and palette items carry the command's reason, such as "select one or more nodes before fitting the selection".

Trackpad guidance:

- two-finger scroll pans the viewport
- pinch zooms on supported devices
- if a host environment does not expose middle-click, `Alt` + left-drag remains the fallback pan gesture

## Node Editing

| Action | Shortcut |
| --- | --- |
| **Select node** | Left-click |
| **Select connection** | Left-click the wire or connection label |
| **Append selection** | `Shift` + left-click |
| **Toggle selection** | `Ctrl` + left-click |
| **Marquee select** | Left-drag on empty canvas |
| **Delete selection** | `Delete` |
| **Copy selection** | `Ctrl + C` |
| **Paste selection** | `Ctrl + V` |
| **Undo** | `Ctrl + Z` |
| **Redo** | `Ctrl + Y` or `Ctrl + Shift + Z` |

Connection selection is exposed through the canonical `selection.connections.set` route. Hosts can inspect selected connection ids from `IGraphEditorSession.Queries.GetSelectionSnapshot()` and can edit persisted bend points through the existing `connections.route-vertex.insert`, `connections.route-vertex.move`, and `connections.route-vertex.remove` commands.

## Workspace Commands

| Action | Shortcut |
| --- | --- |
| **Save document** | `Ctrl + S` |
| **Load document** | `Ctrl + O` |

## Host Guidance

If you embed `NodeCanvas` as a standalone surface and want to own shortcuts yourself, disable the stock bindings:

```csharp
canvas.CommandShortcutPolicy = AsterGraphCommandShortcutPolicy.Disabled;
```

Keep the command route explicit in host documentation when you do this so users do not have to guess whether the canvas still owns `Ctrl+Z`, `Ctrl+S`, or clipboard shortcuts.
This is the same shortcut-ownership pattern used by custom UI hosts on the canonical runtime/session route.

Proof markers for this interaction layer:

- `UX_COMMAND_DISCOVERY_OK:True`
- `UX_NAVIGATION_POLISH_OK:True`
- `UX_COMMAND_DISABLED_REASON_OK:True`
- `INTERACTION_FEEDBACK_OK:True`
- `CANVAS_FOCUS_RECOVERY_OK:True`

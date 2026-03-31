# Interactions and Shortcuts

AsterGraph provides built-in keyboard, mouse, and trackpad interactions out of the box.

## Canvas Navigation

| Action | Mouse / Trackpad | Keyboard Modifier |
| --- | --- | --- |
| **Pan Canvas** | Middle-Click & Drag <br> *or* Left-Click & Drag <br> *or* Two-finger Scroll (Trackpad) | `Alt` (for Left-Click & Drag) |
| **Zoom Canvas** | Scroll Wheel <br> *or* Pinch (Trackpad) | `Ctrl` |

> **Trackpad Tip:** AsterGraph is optimized for modern precision trackpads. You can freely pan the canvas using a two-finger swipe without holding any keys. To zoom, perform a pinch gesture or hold `Ctrl` while scrolling. If your trackpad lacks a middle mouse button, use `Alt + Left-Click` drag to easily pan the viewport.

## Node Editing

| Action | Shortcut |
| --- | --- |
| **Select Node** | Left-Click |
| **Add to Selection** | `Shift` + Left-Click |
| **Toggle Selection** | `Ctrl` + Left-Click |
| **Marquee Select** | Left-Click & Drag (on empty canvas) |
| **Delete Selection** | `Delete` |
| **Copy Selection** | `Ctrl + C` |
| **Paste Selection** | `Ctrl + V` |
| **Undo** | `Ctrl + Z` |
| **Redo** | `Ctrl + Y` / `Ctrl + Shift + Z` |

## Workspace Commands

| Action | Shortcut |
| --- | --- |
| **Save Document** | `Ctrl + S` |
| **Load Document** | `Ctrl + O` |

## Notes for Host Developers

If you are embedding the `NodeCanvas` as a standalone component, you can disable the built-in keyboard shortcuts by setting the `EnableDefaultCommandShortcuts` property to `false`:

```xml
<avalonia:NodeCanvas EnableDefaultCommandShortcuts="False" />
```

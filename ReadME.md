# NGMemory

NGMemory is a Windows-only .NET Framework library for external process memory access, basic debugging, GUI automation, screen analysis, and lightweight overlays.

## Features

- Read and write memory in external processes
- Scan process memory for byte patterns with wildcard support
- Wait for hardware breakpoints and inspect CPU registers
- Automate classic Win32 controls such as buttons, text boxes, combo boxes, check boxes, and list views
- Simulate keyboard and mouse input
- Capture screenshots, search colors, and compare images
- Attach transparent overlay forms to target windows

## Requirements

- Windows
- .NET Framework 4.7.2
- A desktop application target (Win32/WinForms style controls are the main focus)
- Sufficient process permissions for memory and debug operations

## Installation

### NuGet

```powershell
Install-Package NGMemory -Version 1.0.7
```

### Source

```powershell
dotnet build NGMemory.sln
```

## Namespaces

```csharp
using NGMemory;
using NGMemory.Easy;
using NGMemory.Overlay;
using NGMemory.WinInteropTools;
```

## Project Layout

- `NGMemory/NGMemory`: core interop, scanner, debug hook, constants, enums, structures
- `NGMemory/VAMemory`: generic typed memory read/write wrapper
- `NGMemory/WinInteropTools`: low-level control and window interop helpers
- `NGMemory/Easy`: higher-level convenience API
- `NGMemory/Overlay`: overlay manager, configuration, and style helpers

## Quick Start

### Read and write memory

```csharp
using NGMemory;

var memory = new VAMemory("notepad");
if (memory.CheckProcess())
{
    int value = memory.ReadInt32((IntPtr)0x12345678);
    memory.WriteInt32((IntPtr)0x12345678, value + 1);
}
```

### Scan for a byte pattern

```csharp
using System;
using System.Diagnostics;
using NGMemory;

var process = Process.GetProcessesByName("notepad")[0];
var scanner = new Scanner(process);
IntPtr? address = scanner.ScanMemory("90 90 ?? 90");
```

### Automate a window

```csharp
using NGMemory.Easy;

IntPtr hwnd = EasyWindow.FindAndFocus("notepad");
EasyKeyboard.TypeText("Automation started");
```

### Work with controls by dialog/control id

```csharp
using NGMemory.Easy;

EasyTextBox.SetText(hwnd, 1001, "Hello");
EasyButton.Click(hwnd, 1);
bool isChecked = EasyCheckBox.IsChecked(hwnd, 2001);
```

### Create an overlay

```csharp
using System;
using System.Drawing;
using NGMemory.Easy;
using NGMemory.Overlay;

IntPtr hwnd = EasyWindow.GetMainWindow("notepad");
if (hwnd != IntPtr.Zero)
{
    var manager = new OverlayManager();
    manager.CreateOverlay(hwnd, overlay =>
    {
        overlay.Configure()
            .WithSize(220, 80)
            .WithPosition(OverlayPosition.BottomRight)
            .WithOffset(12, 12)
            .WithBackgroundColor(Color.Transparent)
            .WithLabel("Overlay active", 10, 10, Color.LimeGreen)
            .WithButton("Click", 10, 35, 90, 28, (s, e) => { });
    });
}
```

## API Overview

### `NGMemory.Easy`

High-level helper classes for common automation tasks.

- `EasyButton`: click a button by window handle and control id
- `EasyCheckBox`: read, set, toggle, or click check boxes
- `EasyComboBox`: read items and select entries
- `EasyDebugHook`: wait for register values on hardware breakpoints
- `EasyElementFinder`: find child controls by class name, text, or control id
- `EasyFormHelper`: batch operations for text boxes, combo boxes, and check boxes
- `EasyGuiInterop`: get window handles, child windows, titles, class names, and control handles
- `EasyKeyboard`: type text and send common shortcuts
- `EasyMemory`: process lookup, pattern search, and string reads
- `EasyMouse`: move, click, double-click, and drag
- `EasyOverlay`: overlay form attached to another window
- `EasyPressKey`: low-level key pressing helpers based on scan codes
- `EasyScreen`: capture screen, region, or window and search for a color
- `EasyScreenAnalysis`: find all color matches, compare images, and search template images
- `EasySysListView32`: read and interact with `SysListView32` controls
- `EasyTextBox`: get, set, or clear text box content
- `EasyWait`: polling and retry helpers
- `EasyWindow`: find, focus, and inspect windows

### `NGMemory.Overlay`

Overlay-specific components.

- `OverlayManager`: create, track, remove, and auto-scan overlays for windows
- `OverlayConfiguration`: fluent configuration for size, position, controls, colors, and callbacks
- `OverlayStyleHelper`: Alt-Tab visibility helpers for overlay windows
- `TargetWindowType`: filter matching windows by MDI, dialog, normal, or all windows
- `OverlayPosition`: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`, `Center`

### `NGMemory.WinInteropTools`

Lower-level wrappers around Win32 messaging and control access.

- `GuiInteropHandler`: find dialog items, enumerate process windows, and inspect window titles/classes
- `CheckBox`, `ComboBox`, `TextBox`: direct wrappers for common controls
- `InputHelper`: keyboard and mouse input using `SendInput`
- `MenuStripHelper`: click menu entries by index path
- `SysListView32`: read list view items from other processes
- `WindowStyleHelper`: parent/style/position helpers for windows

### Core Types

- `Scanner`: scans readable memory regions of a target process
- `DebugHook`: attaches as debugger, sets a hardware breakpoint, and returns register values
- `Module`: reads a module base address from a process
- `VAMemory`: typed memory read/write convenience wrapper
- `Constants`, `Enums`, `Structures`, `Kernel32`, `User32`, `MessageHelper`: Win32 interop definitions and helper methods

## Notes

- This library targets classic Windows desktop applications. It is not intended for browser automation or modern UI frameworks that do not expose standard Win32 controls.
- Memory scanning, writing, and debugging may require elevated permissions depending on the target process.
- Overlay functionality depends on a message loop, so it is primarily intended for WinForms-style applications.
- Some helper methods interact with controls by sending messages directly; behavior can vary if a target application uses owner-drawn or custom controls.

## License

MIT

# NGMemory 1.0.6 (Interim Release)

> **Note:** Version 1.0.6 is an interim release with major updates and not yet fully tested. Use with caution.

## Table of Contents

1. [Overview](#overview)
2. [Installation](#installation)
3. [NGMemory Easy API](#ngmemory-easy-api)

   1. [EasyConfig](#easyconfig)
   2. [EasyButton](#easybutton)
   3. [EasyCheckBox](#easycheckbox)
   4. [EasyComboBox](#easycombobox)
   5. [EasyTextBox](#easytextbox)
   6. [EasyFormHelper](#easyformhelper)
   7. [EasyGuiInterop](#easyguiinterop)
   8. [EasyKeyboard & EasyPressKey](#easykeyboard--easypresskey)
   9. [EasyMouse](#easymouse)
   10. [EasyElementFinder](#easyelementfinder)
   11. [EasyDebugHook](#easydebughook)
   12. [EasyMemory](#easymemory)
   13. [EasyScreen & EasyScreenAnalysis](#easyscreen--easyscreenanalysis)
   14. [EasySysListView32](#easysyslistview32)
   15. [EasyWait](#easywait)
   16. [EasyWindow](#easywindow)
4. [WinInteropTools](#wininteroptools)

   1. [GuiInteropHandler](#guiinterophandler)
   2. [CheckBox & ComboBox](#checkbox--combobox)
   3. [InputHelper](#inputhelper)
   4. [MenuStripHelper](#menustriphelper)
   5. [SysListView32](#syslistview32)
   6. [TextBox](#textbox)
5. [Core Library (NGMemory)](#core-library-ngmemory)

   1. [Constants, Enums, Structures](#constants-enums-structures)
   2. [User32, Kernel32 & MessageHelper](#user32-kernel32--messagehelper)
   3. [DebugHook](#debughook)
   4. [Scanner](#scanner)
   5. [Module](#module)
   6. [VAMemory](#vamemory)
6. [Examples](#examples)
7. [License](#license)

---

## Overview

NGMemory 1.0.6 provides:

* **GUI Automation**: Click buttons, set text, interact with ComboBox, CheckBox, etc.
* **Memory Access**: Read/write process memory, pattern scanning.
* **Debugging**: Set hardware breakpoints and read registers.
* **Screen Analysis**: Screenshots, color search, image matching.

## Installation

```powershell
Install-Package NGMemory -Version 1.0.6
```

```csharp
using NGMemory;
using NGMemory.Easy;
using NGMemory.WinInteropTools;
```

## NGMemory Easy API

### EasyConfig

Global settings for delays and logging:

* `DefaultKeyDelay` (ms) – delay between keystrokes.
* `DefaultMouseDelay` (ms) – delay between mouse moves.
* `EnableVerboseLogging` – detailed logs.
* `EnableAutoRetry`, `MaxRetryCount` – retry on failure.

**Example:**

```csharp
EasyConfig.DefaultKeyDelay = 20;
EasyConfig.EnableVerboseLogging = true;
```

### EasyButton

Simulate button clicks by control ID.

* `Click(windowHandle, controlId)`
* `ClickAsync(windowHandle, controlId)`

**Example:**

```csharp
EasyButton.Click(hwnd, 1001);
EasyButton.ClickAsync(hwnd, 1002);
```

### EasyCheckBox

CheckBox helpers:

* `IsChecked(windowHandle, controlId)` → bool
* `SetChecked(windowHandle, controlId, state)`
* `ToggleState(windowHandle, controlId)`
* `ClickCheckBox(windowHandle, controlId)`

**Example:**

```csharp
bool on = EasyCheckBox.IsChecked(hwnd, 2001);
EasyCheckBox.ToggleState(hwnd, 2001);
```

### EasyComboBox

ComboBox helpers:

* `GetSelectedItem(comboHandle)` → string or null
* `GetItems(comboHandle)` → string\[]
* `SelectItemByString(comboHandle, text)`
* `SelectItemByIndex(comboHandle, index)`

**Example:**

```csharp
var items = EasyComboBox.GetItems(combo);
EasyComboBox.SelectItemByString(combo, "Option A");
```

### EasyTextBox

TextBox helpers:

* `GetText(windowHandle, controlId)` → string
* `SetText(windowHandle, controlId, text)`
* `ClearText(windowHandle, controlId)`

**Example:**

```csharp
string text = EasyTextBox.GetText(hwnd, 3001);
EasyTextBox.ClearText(hwnd, 3001);
```

### EasyFormHelper

Batch operations on multiple controls:

* `SetTextFields(windowHandle, Dictionary<int,string>)`
* `GetTextFields(windowHandle, params int[])` → Dictionary\<int,string>
* `SetCheckBoxes(windowHandle, Dictionary<int,bool>)`
* `GetCheckBoxes(windowHandle, params int[])` → Dictionary\<int,bool>
* `SetComboBoxes(windowHandle, Dictionary<int,string>)`
* `GetComboBoxes(windowHandle, params int[])` → Dictionary\<int,string>

**Example:**

```csharp
var textMap = new Dictionary<int,string>{{3001,"A"},{3002,"B"}};
EasyFormHelper.SetTextFields(hwnd, textMap);
```

### EasyGuiInterop

Low-level window and control handles:

* `GetControlHandle(windowHandle, controlId)` → IntPtr
* `GetChildWindows(parentHandle)` → List<IntPtr>
* `GetWindowTitle(hWnd)` → string
* `SetText(dialogHandle, controlId, text)`

### EasyKeyboard & EasyPressKey

Keyboard input:

* `TypeText(text, delay)` / `TypeTextAsync(text, delay)`
* `SendCtrlC()`, `SendCtrlV()`
* `PressKeys(async, KeyCode...)` / `PressKeysAsync(KeyCode...)`
* \`PressKeysWithDelay(delay, KeyCode...)

**Example:**

```csharp
EasyKeyboard.TypeText("Hello World", 10);
EasyPressKey.PressKeys(false, KeyCode.LCtrl, KeyCode.C);
```

### EasyMouse

Mouse operations:

* `MoveTo(x,y)`
* `MoveWithHumanMotion(x,y,duration)`
* `Click(button)`, `ClickAt(x,y,button)`
* `HumanClickAt(x,y,button)`
* `DoubleClick(button)`
* \`DragAndDrop(fromX,fromY,toX,toY)

**Example:**

```csharp
EasyMouse.HumanClickAt(100,200);
```

### EasyElementFinder

Search UI elements by criteria:

* `FindElement(parent, className?, windowText?, controlId?)` → IntPtr
* `GetWindowRect(hWnd)` → Rectangle

**Example:**

```csharp
IntPtr btn = EasyElementFinder.FindElement(mainHwnd, className:"Button", windowText:"OK");
```

### EasyDebugHook

Wait for hardware breakpoint and read register:

* `WaitForRegister(processName or ID, address, Register)` → ulong

**Example:**

```csharp
ulong value = EasyDebugHook.WaitForRegister("notepad", addr, Register.Rax);
```

### EasyMemory

Memory pattern scanning and reading strings:

* `FindPattern(processName, pattern, startAddr?, endAddr?)` → IntPtr
* `ReadString(processName, address, maxLength, encoding?)` → string

**Example:**

```csharp
IntPtr addr = EasyMemory.FindPattern("game", "90 90 ?? 90");
string title = EasyMemory.ReadString("game", addr);
```

### EasyScreen & EasyScreenAnalysis

Screen capture and analysis:

* `CaptureScreen()`, `CaptureRegion(x,y,w,h)`, `CaptureWindow(hWnd)`
* `FindColor(color, area, tolerance)` → Point?
* `FindAllColorMatches(color, area, tolerance)` → List<Point>
* `CompareImages(img1,img2,samplingRate)` → double%
* `FindImageOnScreen(template, area, minSimilarity)` → Point?

**Example:**

```csharp
var matches = EasyScreenAnalysis.FindAllColorMatches(Color.Red, new Rectangle(0,0,50,50));
```

### EasySysListView32

ListView control helpers:

* `GetItems(handle, columnCount or auto)` → List<ListViewItem>
* `GetAllRowsAsStrings(handle, columnCount or auto)` → List\<string\[]>
* `InsertItem(handle,index,text)`, `RemoveItem(handle,index)`, `ClearAllItems(handle)`
* `SetItemText(handle,index,text)`, `SelectItemByName(handle,name)`

**Example:**

```csharp
var rows = EasySysListView32.GetAllRowsAsStrings(lvHwnd, true);
EasySysListView32.InsertItem(lvHwnd, 0, "New");
```

### EasyWait

Waiting utilities:

* `Until(condition, timeout, interval)` → bool
* `ForDuration(ms)`
* `RetryUntilSuccess(action, successCheck, attempts, delay)` → bool

### EasyWindow

Window handling:

* `Find(processName, partialTitle?)` / `FindAndFocus(...)` → IntPtr
* `GetMainWindow(processName, partialTitle?)`, `FocusWindow(hWnd)`
* `GetAllChildWindows(parent)` → List<IntPtr>

## WinInteropTools

Low-level P/Invoke wrappers for GUI interop.

### GuiInteropHandler

* `InteropSetText(dialog, id, text)`
* `GetWindowTitle(hWnd)`
* `getRef(hWnd, id)` → HandleRef
* `EnumerateProcessWindowHandles(process)` → IEnumerable<IntPtr>
* `getChildList(parent)` → List<IntPtr>

### CheckBox & ComboBox

* `CheckBox.IsCheckBoxChecked`, `SetCheckBoxState`
* `ComboBox.GetSelectedItem`, `GetItems`, `SetSelectedItem`, `SetSelectedIndex`

### InputHelper

Keyboard/mouse via `SendInput`:

* `PressKeys`, `PressKeysWithDelay`, `CopySelection`
* `MouseMoveTo`, `MouseClick`, `MouseDown`, `MouseUp`

### MenuStripHelper

Click menu items by path:

* `ClickMenu(hWnd, async, indices[] )`

### SysListView32

Read ListView items in another process:

* `GetListViewItems`, `ReadListViewItem`

### TextBox

Get value of TextBox control:

* `getTextBoxValue(window, controlId)` → string

## Core Library (NGMemory)

### Constants, Enums, Structures

Windows API constants, register and MessageBox enums,
`DEBUG_EVENT`, `CONTEXT`, `MEMORY_BASIC_INFORMATION`, `SYSTEM_INFO`.

### User32, Kernel32 & MessageHelper

All P/Invoke signatures and `ShowMessage` helper.

### DebugHook

Attach to process, set hardware breakpoint, wait for single-step exception, read register.

### Scanner

Scan process memory regions with `VirtualQueryEx`, search byte patterns with wildcards.

### Module

Get base address of a module by name in a process.

### VAMemory

General-purpose read/write of any type:
`ReadByteArray`, `ReadStringUnicode/ASCII`, `ReadInt32`, ..., `WriteXxx` methods.

**Example:**

```csharp
var mem = new VAMemory("game");
if(mem.CheckProcess()){
    int health = mem.ReadInt32(addr);
    mem.WriteInt32(addr, 999);
}
```

## Examples

```csharp
// Automate Notepad:
var hwnd = EasyWindow.Find("notepad");
EasyWindow.FocusWindow(hwnd);
EasyKeyboard.TypeText("Automation started...\n");
EasyButton.Click(hwnd, 1);
```

## License

MIT
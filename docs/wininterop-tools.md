# WinInteropTools

`NGMemory.WinInteropTools` contains lower-level wrappers for classic Win32 controls and input.

Prefer `NGMemory.Easy` for normal use. Use these when you need direct handles or lower-level behavior.

```csharp
using NGMemory.WinInteropTools;
```

## GuiInteropHandler

Find and inspect window handles.

```csharp
using System;
using System.Diagnostics;
using NGMemory.WinInteropTools;

Process process = Process.GetProcessesByName("notepad")[0];
var handles = GuiInteropHandler.EnumerateProcessWindowHandles(process);

foreach (IntPtr hwnd in handles)
{
    Console.WriteLine("HWND: " + hwnd);
    Console.WriteLine("Title: " + GuiInteropHandler.GetWindowTitle(hwnd));
    Console.WriteLine("Class: " + GuiInteropHandler.GetClassName(hwnd));
}
```

Get a child/control reference by ID:

```csharp
var handleRef = GuiInteropHandler.getRef(dialogHandle, 1001);
Console.WriteLine(handleRef.Handle);
```

## TextBox

```csharp
IntPtr dialog = /* target dialog */;

string text = NGMemory.WinInteropTools.TextBox.getTextBoxValue(dialog, 1001);
Console.WriteLine(text);
```

For setting text, prefer `NGMemory.Easy.EasyTextBox.SetText`.

## CheckBox

```csharp
NGMemory.WinInteropTools.CheckBox.SetCheckBoxState(dialog, 2001, true);
bool isChecked = NGMemory.WinInteropTools.CheckBox.IsCheckBoxChecked(dialog, 2001);
```

## ComboBox

```csharp
IntPtr combo = /* combo handle */;

string current = NGMemory.WinInteropTools.ComboBox.GetSelectedItem(combo);
string[] items = NGMemory.WinInteropTools.ComboBox.GetItems(combo);
NGMemory.WinInteropTools.ComboBox.SetSelectedIndex(combo, 0);
NGMemory.WinInteropTools.ComboBox.SetSelectedItem(combo, "Option A");
```

## SysListView32

For easier usage, prefer `EasySysListView32`. The low-level wrapper can be useful for advanced scenarios.

```csharp
IntPtr listView = Easy.EasySysListView32.SearchSysListView32InWindow(parentWindow);
var rows = Easy.EasySysListView32.GetAllRowsAsStrings(listView, autoColumnCount: true);
```

## InputHelper

```csharp
using static NGMemory.Enums;
using NGMemory.WinInteropTools;

InputHelper.PressKeys(false, KeyCode.LCtrl, KeyCode.C);
InputHelper.PressKeysWithDelay(50, KeyCode.LCtrl, KeyCode.V);
InputHelper.MouseMoveTo(500, 300);
InputHelper.MouseClick(NGMemory.Easy.MouseButton.Left);
```

## MenuStripHelper

Use when a classic menu can be addressed by index path.

```csharp
// Example idea: File -> Open might be index path 0, 1 depending on the target app.
MenuStripHelper.ClickMenu(mainWindowHandle, false, 0, 1);
```

Menu indexes vary per application and language.

## WindowStyleHelper

```csharp
using NGMemory.WinInteropTools;

int style = WindowStyleHelper.GetWindowLong(hwnd, WindowStyleHelper.GWL_STYLE);
int exStyle = WindowStyleHelper.GetWindowLong(hwnd, WindowStyleHelper.GWL_EXSTYLE);

WindowStyleHelper.MakeWindowTransparent(hwnd);
WindowStyleHelper.MakeWindowTopMost(hwnd);
WindowStyleHelper.SetWindowPosition(hwnd, 20, 20, 400, 300);
```

Use style helpers carefully; changing styles on foreign windows can affect the target UI.

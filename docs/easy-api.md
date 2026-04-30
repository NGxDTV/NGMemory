# Easy API

The `NGMemory.Easy` namespace contains higher-level helpers for common tasks. These are the best starting point before using lower-level Win32 wrappers.

```csharp
using NGMemory.Easy;
```

## Windows

### Find And Focus A Main Window

```csharp
using System;
using NGMemory.Easy;

IntPtr hwnd = EasyWindow.FindAndFocus("notepad");
if (hwnd == IntPtr.Zero)
{
    Console.WriteLine("Notepad not found");
}
```

### Find By Partial Title

```csharp
IntPtr hwnd = EasyWindow.Find("notepad", "Untitled");
Console.WriteLine(hwnd);
```

### Get Child Windows

```csharp
IntPtr hwnd = EasyWindow.Find("notepad");
var children = EasyWindow.GetAllChildWindows(hwnd);

foreach (IntPtr child in children)
{
    Console.WriteLine(child);
}
```

### Capture Protection On A Top-Level Window

```csharp
IntPtr hwnd = EasyWindow.Find("myprocess");
var result = EasyWindow.SetCaptureProtection(hwnd, true);
Console.WriteLine(result);

EasyWindow.ClearCaptureProtection(hwnd);
```

Important: `SetWindowDisplayAffinity` only works on top-level windows owned by the current process. For protected regions use `NGMemory.CaptureProtection`.

## Classic Controls By Control ID

These helpers target classic Win32/WinForms controls with dialog/control IDs.

### TextBox

```csharp
IntPtr dialog = EasyWindow.Find("myapp");

EasyTextBox.SetText(dialog, 1001, "Test value");
string current = EasyTextBox.GetText(dialog, 1001);
EasyTextBox.ClearText(dialog, 1001);
```

### Button

```csharp
IntPtr dialog = EasyWindow.Find("myapp");
EasyButton.Click(dialog, 1);
EasyButton.ClickAsync(dialog, 2);
```

### CheckBox

```csharp
IntPtr dialog = EasyWindow.Find("myapp");

bool checkedNow = EasyCheckBox.IsChecked(dialog, 2001);
EasyCheckBox.SetChecked(dialog, 2001, true);
EasyCheckBox.ToggleState(dialog, 2001);
EasyCheckBox.ClickCheckBox(dialog, 2001);
```

### ComboBox

```csharp
IntPtr combo = EasyElementFinder.FindElement(parentWindow, className: "ComboBox");

string selected = EasyComboBox.GetSelectedItem(combo);
string[] items = EasyComboBox.GetItems(combo);
EasyComboBox.SelectItemByString(combo, "Option A");
EasyComboBox.SelectItemByIndex(combo, 0);
```

## Form Helper

Batch set/read controls by ID:

```csharp
using System.Collections.Generic;
using NGMemory.Easy;

IntPtr dialog = EasyWindow.Find("myapp");

EasyFormHelper.SetTextFields(dialog, new Dictionary<int, string>
{
    [1001] = "First",
    [1002] = "Second"
});

EasyFormHelper.SetCheckBoxes(dialog, new Dictionary<int, bool>
{
    [2001] = true,
    [2002] = false
});

var values = EasyFormHelper.GetTextFields(dialog, 1001, 1002);
```

## Element Finder

Find controls by class name, title text, or control ID:

```csharp
IntPtr parent = EasyWindow.Find("myapp");

IntPtr okButton = EasyElementFinder.FindElement(
    parent,
    className: "Button",
    windowText: "OK");

IntPtr controlById = EasyElementFinder.FindElement(parent, controlId: 1001);
var rect = EasyElementFinder.GetWindowRect(okButton);
```

## Keyboard

Keyboard helpers send input to the active foreground application.

```csharp
EasyWindow.FindAndFocus("notepad");
EasyKeyboard.TypeText("Hello world", delayBetweenChars: 20);
EasyKeyboard.SendCtrlC();
EasyKeyboard.SendCtrlV();
```

Use low-level key presses for shortcuts that are not wrapped directly:

```csharp
using static NGMemory.Enums;

EasyPressKey.PressKeys(KeyCode.LCtrl, KeyCode.A);
EasyPressKey.PressKeysWithDelay(50, KeyCode.LCtrl, KeyCode.C);
EasyPressKey.PressKeysAsync(KeyCode.Enter).Wait();
```

## Mouse

```csharp
EasyMouse.MoveTo(500, 300);
EasyMouse.Click(MouseButton.Left);
EasyMouse.ClickAt(600, 320);
EasyMouse.DoubleClick();
EasyMouse.DragAndDrop(100, 100, 400, 400);
EasyMouse.HumanClickAt(800, 500, doubleClick: false, button: MouseButton.Left);
```

## Wait And Retry

```csharp
bool found = EasyWait.Until(
    () => EasyWindow.Find("notepad") != IntPtr.Zero,
    timeout: 5000,
    checkInterval: 100);

EasyWait.ForDuration(250);

bool success = EasyWait.RetryUntilSuccess(
    action: () => EasyKeyboard.TypeText("retry"),
    successCheck: () => true,
    maxAttempts: 3,
    delayBetweenAttempts: 250);
```

## GUI Interop Helpers

```csharp
using System.Diagnostics;
using NGMemory.Easy;

var process = Process.GetProcessesByName("notepad")[0];
var handles = EasyGuiInterop.EnumerateProcessWindowHandles(process);

foreach (IntPtr handle in handles)
{
    Console.WriteLine(EasyGuiInterop.GetWindowTitle(handle));
    Console.WriteLine(EasyGuiInterop.GetClassName(handle));
}
```

## SysListView32

```csharp
IntPtr parent = EasyWindow.Find("someapp");
IntPtr listView = EasySysListView32.SearchSysListView32InWindow(parent);

int count = EasySysListView32.GetItemCount(listView);
int columns = EasySysListView32.GetColumnCount(listView);
var rows = EasySysListView32.GetAllRowsAsStrings(listView, autoColumnCount: true);

foreach (string[] row in rows)
{
    Console.WriteLine(string.Join("\t", row));
}

EasySysListView32.SelectItemByName(listView, "Example");
```

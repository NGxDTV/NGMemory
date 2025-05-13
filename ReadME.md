# NGMemory v1.0.5

NGMemory is a powerful, easy-to-use C# library that simplifies external **process-memory work** *and* rich **GUI automation**. Whether you are debugging, manipulating memory, or scripting complex UIs, NGMemory has you covered.

---

## 🚀 What’s new in 1.0.5

| Area                | Added                                                                                                                |
| ------------------- | -------------------------------------------------------------------------------------------------------------------- |
| **Menu automation** | `WinInteropTools.MenuStripHelper.ClickMenu(...)` – trigger any *nested* MenuStrip item; no SendKeys/RDP hassle.      |
| **Input combos**    | `WinInteropTools.InputHelper.PressKeys(...)` – press **any** key combination via `params ScanCode[]`, sync or async. |
| **Key enum**        | `WinInteropTools.ScanCode` – readable names for all common scan-codes (e.g. `ScanCode.LCtrl`, `ScanCode.C`).         |

> **Why PressKeys?**  It is a drop-in, RDP-friendly replacement for `SendKeys`, perfect for **unattended** automation where classic `SendKeys` fails.

### Quick 1.0.5 Examples

```csharp
// Click menu path 5 ➜ 7 ➜ 4 (non-blocking)
MenuStripHelper.ClickMenu(targetWnd, true, 5, 7, 4);

// Ctrl + C (blocking)
InputHelper.PressKeys(false, ScanCode.LCtrl, ScanCode.C);

// Alt + U (async)
InputHelper.PressKeys(true, ScanCode.LAlt, ScanCode.U);

// Only U (blocking)
InputHelper.PressKeys(false, ScanCode.U);
```

---

## Core Features (v1.0.x)

* **Memory Scanning** – locate byte patterns in target processes.
* **Module Base Address Lookup** – grab module bases fast.
* **Debugging** – attach, set HW breakpoints, read CPU registers.
* **Memory Reading & Writing** – safe external memory access.
* **GUI Interactions** – automate checkboxes, text boxes, combo boxes, list views, etc.

---

## Easy Helper Classes

NGMemory provides helpers under `NGMemory.Easy` to make UI automation painless.

### `EasyCheckBox`

* `IsChecked`, `SetChecked`, `ToggleState`, `ClickCheckBox`

### `EasyTextBox`

* `GetText`, `SetText`, `ClearText`

### `EasyComboBox`

* `GetSelectedItem`, `GetItems`, `SelectItemByString`, `SelectItemByIndex`

### `EasySysListView32`

* `GetItems`, `ReadItemText`, `GetColumnCount`, `InsertItem`, `RemoveItem`, `ClearAllItems`, `SetItemText`, etc.

### `EasyGuiInterop`

* Window text, titles, enumeration, z-order, focus…

### `EasyWindow`

* `GetMainWindow`, `GetAllChildWindows`, `FindAndFocus`, `FocusWindow`

### `EasyFormHelper`

* Batch set / read **TextBoxes**, **CheckBoxes**, **ComboBoxes** with dictionaries.

---

## Usage Examples

### 1. Setting Checkboxes

```csharp
NGMemory.Easy.EasyFormHelper.SetCheckBoxes(windowHandle, new Dictionary<int, bool>
{
    { 0x1001, true },
    { 0x1002, false }
});
```

### 2. Reading & Updating a TextBox

```csharp
string current = NGMemory.Easy.EasyTextBox.GetText(windowHandle, 0x4C);
NGMemory.Easy.EasyTextBox.SetText(windowHandle, 0x4C, "Updated Text");
```

### 3. ComboBox Selections

```csharp
string sel = NGMemory.Easy.EasyComboBox.GetSelectedItem(comboHandle);
NGMemory.Easy.EasyComboBox.SelectItemByString(comboHandle, "Option B");
```

### 4. ListView Automation

```csharp
int cols = NGMemory.Easy.EasySysListView32.GetColumnCount(listViewHandle);
var items = NGMemory.Easy.EasySysListView32.GetItems(listViewHandle, cols);

foreach (var it in items)
    Console.WriteLine(it.Text);

NGMemory.Easy.EasySysListView32.InsertItem(listViewHandle, 0, "NewItem");
```

### 5. Window Handling

```csharp
IntPtr h = NGMemory.Easy.EasyWindow.FindAndFocus("twe", "Einlagerungserfassung");
if (h != IntPtr.Zero)
{
    // ...
}
```

### 6. Filling a Form in One Go

```csharp
NGMemory.Easy.EasyFormHelper.SetTextFields(windowHandle, new Dictionary<int, string>
{
    { 0x101, "First" },
    { 0x102, "Second" },
    { 0x103, "Third" }
});

NGMemory.Easy.EasyFormHelper.SetCheckBoxes(windowHandle, new Dictionary<int, bool>
{
    { 0x201, true },
    { 0x202, false }
});

NGMemory.Easy.EasyFormHelper.SetComboBoxes(windowHandle, new Dictionary<int, string>
{
    { 0x301, "Choice A" },
    { 0x302, "Choice B" }
});
```

---

## Contributing
Feel free to fork this repository and contribute by submitting pull requests. Issues and feature requests are welcome!

## License
This library is licensed under the [MIT License](LICENSE).

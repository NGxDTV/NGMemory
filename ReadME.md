# NGMemory v1.0.4

NGMemory is a powerful, easy-to-use C# library designed to simplify working with external process memory and GUI interactions. Whether you're building debugging tools, exploring memory manipulation, or automating external applications, NGMemory has you covered.

## Core Features

- **Memory Scanning**: Locate byte patterns in target processes.
- **Module Base Address Lookup**: Quickly grab the base address of specific modules.
- **Debugging**: Attach to processes, set hardware breakpoints, and read CPU registers.
- **Memory Reading & Writing**: Read or modify external process memory safely.
- **GUI Interactions**: Automate and interact with controls like checkboxes, text boxes, combo boxes, list views, and more.

## Easy Helper Classes

To streamline GUI automation, NGMemory now includes several helper classes under the `NGMemory.Easy` namespace. These classes make it simple to interact with common controls in external applications:

### `EasyCheckBox`
- **IsChecked(IntPtr window, int controlId)**  
  Returns `true` if the checkbox is checked, otherwise `false`.

- **SetChecked(IntPtr window, int controlId, bool state)**  
  Sets the checkbox to either checked (`true`) or unchecked (`false`).

- **ToggleState(IntPtr window, int controlId)**  
  Flips the current checkbox state.

- **ClickCheckBox(IntPtr window, int controlId)**  
  Simulates a click on the checkbox control.

### `EasyTextBox`
- **GetText(IntPtr window, int controlId)**  
  Returns the current text in the specified text box.

- **SetText(IntPtr window, int controlId, string text)**  
  Updates the text in the specified text box.

- **ClearText(IntPtr window, int controlId)**  
  Clears the content of the specified text box.

### `EasyComboBox`
- **GetSelectedItem(IntPtr comboBoxHandle)**  
  Returns the currently selected item's text.

- **GetItems(IntPtr comboBoxHandle)**  
  Returns all items as a string array.

- **SelectItemByString(IntPtr comboBoxHandle, string itemText)**  
  Selects the combo box entry that matches `itemText`.

- **SelectItemByIndex(IntPtr comboBoxHandle, int index)**  
  Selects the combo box entry at the specified index.

### `EasySysListView32`
- **GetItems(IntPtr listViewHandle, int columnCount)**  
  Reads all items (rows) from the list view, returning a list of `ListViewItem` objects.

- **GetItems(IntPtr listViewHandle, bool autoColumnCount)**  
  Same as above, but automatically detects column count if `autoColumnCount` is true.

- **ReadItemText(IntPtr listViewHandle, int itemIndex, int subIndex)**  
  Reads the text from a specific row and column.

- **GetColumnCount(IntPtr listViewHandle)**  
  Retrieves the number of columns via the list view's header.

- **GetAllRowsAsStrings(...)**  
  Returns all rows as simple arrays of strings.

- **InsertItem(IntPtr listViewHandle, int index, string text)**  
  Inserts a new item (row) at the specified index with the given text in column 0.

- **RemoveItem(IntPtr listViewHandle, int index)**  
  Removes an item (row) at the specified index.

- **ClearAllItems(IntPtr listViewHandle)**  
  Removes all items from the list view.

- **SetItemText(IntPtr listViewHandle, int itemIndex, string newText)**  
  Updates the text of an item at the specified index (column 0).

### `EasyGuiInterop`
- Provides shortcuts for:
  - Getting/setting window text.
  - Fetching window titles.
  - Enumerating process windows.
  - Retrieving child handles and controlling their order.
  - Bringing windows into the foreground, etc.

### `EasyWindow`
- **GetMainWindow(processName, partialTitle)**  
  Returns the main window handle of a process (optionally checking if the window's title contains a specific substring).

- **GetAllChildWindows(parentHandle)**  
  Retrieves all child window handles for a given parent handle.

- **GetChildByTitle(parentHandle, partialTitle)**  
  Finds a child window by matching part of its title.

- **FocusWindow(windowHandle)**  
  Restores and brings the specified window to the foreground.

- **FindAndFocus(processName, partialTitle)**  
  Combines retrieval and focus in one step.

### `EasyFormHelper`
- **SetTextFields(...) / GetTextFields(...)**  
  Batch set or read multiple text boxes using a dictionary of `{controlId -> text}`.

- **SetCheckBoxes(...) / GetCheckBoxes(...)**  
  Batch set or read multiple checkboxes using a dictionary of `{controlId -> bool}`.

- **SetComboBoxes(...) / GetComboBoxes(...)**  
  Batch set or read multiple combo boxes using a dictionary of `{controlId -> string}`.

This makes common tasks (like populating a form with data or reading form fields at once) straightforward and saves you from writing many repetitive calls.

## Usage Examples

### 1. Setting Checkboxes
```csharp
// Example: Enable two checkboxes at once
NGMemory.Easy.EasyFormHelper.SetCheckBoxes(windowHandle, new Dictionary<int, bool>
{
    { 0x1001, true },
    { 0x1002, false }
});
```

### 2. Reading and Updating a TextBox
```csharp
// Read the current text
string currentText = NGMemory.Easy.EasyTextBox.GetText(windowHandle, 0x4C);

// Update the text
NGMemory.Easy.EasyTextBox.SetText(windowHandle, 0x4C, "Updated Text");
```

### 3. ComboBox Selections
```csharp
// Read selected item
string selectedItem = NGMemory.Easy.EasyComboBox.GetSelectedItem(comboHandle);

// Select an item by string
NGMemory.Easy.EasyComboBox.SelectItemByString(comboHandle, "Option B");
```

### 4. ListView Automation
```csharp
int columnCount = NGMemory.Easy.EasySysListView32.GetColumnCount(listViewHandle);
var items = NGMemory.Easy.EasySysListView32.GetItems(listViewHandle, columnCount);

foreach (var item in items)
{
    Console.WriteLine(item.Text);
}

// Inserting new item
NGMemory.Easy.EasySysListView32.InsertItem(listViewHandle, 0, "NewItem");

```

### 5. Window Handling
```csharp
// Finds a window of process "twe" whose title contains "Einlagerungserfassung"
IntPtr handle = NGMemory.Easy.EasyWindow.FindAndFocus("twe", "Einlagerungserfassung");

// Brings it to the foreground if found
if (handle != IntPtr.Zero)
{
    // ...
}

```

### Example: Writing Data in One Go
```csharp
// Suppose you have 5 text boxes, 3 checkboxes, and 2 combo boxes to set all at once:
NGMemory.Easy.EasyFormHelper.SetTextFields(windowHandle, new Dictionary<int, string>
{
    { 0x101, "First Value" },
    { 0x102, "Second Value" },
    { 0x103, "Third Value" },
    { 0x104, "Fourth Value" },
    { 0x105, "Fifth Value" }
});

NGMemory.Easy.EasyFormHelper.SetCheckBoxes(windowHandle, new Dictionary<int, bool>
{
    { 0x201, true },
    { 0x202, false },
    { 0x203, true }
});

NGMemory.Easy.EasyFormHelper.SetComboBoxes(windowHandle, new Dictionary<int, string>
{
    { 0x301, "Some Combo Option" },
    { 0x302, "Another Combo Option" }
});
```

With these helpers, repetitive interop calls (like SendMessage) are hidden behind user-friendly methods, allowing you to focus on the logic of your automation or debugging tasks.

## Contributing
Feel free to fork this repository and contribute by submitting pull requests. Issues and feature requests are welcome!

## License
This library is licensed under the [MIT License](LICENSE).

# NGMemory

NGMemory is a powerful yet easy-to-use C# library designed to simplify working with external process memory. Whether you're crafting debugging tools, exploring memory manipulation, or building advanced applications, NGMemory provides the tools you need to read, analyze, and modify memory in real-time effortlessly.

## Features
- **Memory Scanning**: Locate patterns in memory with byte-level precision.
- **Module Address Retrieval**: Easily find the base address of a specific module in a target process.
- **Debugging Utilities**: Manage hardware breakpoints, retrieve CPU register values, and control debugging sessions.
- **Memory Reading/Writing**: Perform efficient and safe read/write operations on external process memory.
- **GUI Interactions**: Work with GUI components like checkboxes and text boxes within external applications.

## New Features
- **Check Box Handling**: Check the state of checkboxes in external applications.
- **Text Manipulation in GUI**: Set and get text in GUI controls like dialogs and text boxes.
- **List View Manipulation**: Retrieve and manipulate list items in external applications.

## Installation
You can install NGMemory via NuGet:
```bash
dotnet add package NGMemory
```

## Usage

### Memory Scanning
```csharp
using NGMemory;
using System.Diagnostics;

Process targetProcess = Process.GetProcessesByName("SonsOfTheForest").FirstOrDefault();
Scanner scanner = new Scanner(targetProcess);

IntPtr? result = scanner.ScanMemory("F3 0F 10 70 ?? 33 D2 48 8B CF ??");
IntPtr? result2 = scanner.ScanMemory("F3 0F 10 70 10 33 D2 48 8B CF", (long)baseAddress, 0x100000000000);

MessageBox.Show(result.HasValue ? $"Pattern found at: {result.Value.ToString("X")}" : "Pattern not found.");
MessageBox.Show(result2.HasValue ? $"Pattern found at: {result2.Value.ToString("X")}" : "Pattern not found.");
```

#### Result:
```console
Pattern found at: 7FFB7F1ED715
Pattern found at: 7FFB7F1ED715
```
With (long)baseAddress, 0x100000000000, you can search within a range from (baseAddress - 0x100000000000) to (baseAddress + 0x100000000000).

### Retrieve Module Base Address
```csharp
using NGMemory;

Module module = new Module();
IntPtr baseAddress = module.getModuleBaseAddress("GameAssembly.dll", "SonsOfTheForest");
MessageBox.Show($"Base Address: {baseAddress.ToString("X")}");
```

#### Result:
```console
Base Address: 7FFB7BA60000
```

### Debugging Example
```csharp
using NGMemory;

ulong registerValue = DebugHook.WaitForRegister("SonsOfTheForest", new IntPtr(0x7FFB7F1ED715), Register.Rax);
MessageBox.Show($"Register RAX Value: {registerValue:X}");
```

#### Result:
```console
Register RAX Value: 0x1C741FA3310
```

### GET CheckBox Example
```csharp
using NGMemory.WinInteropTools;

bool isChecked = CheckBox.IsCheckBoxChecked(pointer, controlId);
MessageBox.Show(isChecked ? "Checked" : "Unchecked");
```

### GET TextBox Example
```csharp
using NGMemory.WinInteropTools;

string textBoxValue = TextBox.getTextBoxValue(pointer, controlId);
MessageBox.Show($"TextBox Value: {textBoxValue}");
```

### SET TextBox Example
```csharp
using NGMemory.WinInteropTools;

GuiInteropHandler.InteropSetText(pointer, 0x4C, "Test");
```

### GET List View Example
```csharp
using NGMemory.WinInteropTools;

List<ListViewItem> items = SysListView32.GetListViewItems(pointer, columnCount);
foreach (var item in items)
{
    Console.WriteLine(item.Text);
}
```

### GET List View Example .NET Forms
```csharp
var p = Process.GetProcessesByName("twe");
List<IntPtr> ChildList = getChildList(getWindowByContainsName(p, "Window Titel"));

IntPtr[] childLists = ChildList.ToArray();
IntPtr windowPointer = getWindowByContainsName(p, "Window Titel");

IntPtr hwndListView = NGMemory.Kernel32.GetDlgItem(childLists[0], 0x52C);
List<ListViewItem> items = GetListViewItems(hwndListView, 14);
foreach (var item in items)
{
    listview.Items.Add(item);
}
```

## NuGet Download
[https://www.nuget.org/packages/NGMemory/1.0.3](https://www.nuget.org/packages/NGMemory/1.0.3)

## Contributing
Feel free to fork this repository and contribute by submitting pull requests. Issues and feature requests are welcome!

## License
This library is licensed under the [MIT License](LICENSE).

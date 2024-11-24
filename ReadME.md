# NGMemory

NGMemory is a powerful C# library for external process memory manipulation and debugging. It allows developers to interact with other processes' memory regions, retrieve data, and manipulate memory in real-time.

## Features
- **Memory Scanning**: Locate patterns in memory with byte-level precision.
- **Module Address Retrieval**: Easily find the base address of a specific module in a target process.
- **Debugging Utilities**: Manage hardware breakpoints, retrieve CPU register values, and control debugging sessions.
- **Memory Reading/Writing**: Perform efficient and safe read/write operations on external process memory.

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
```
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
```Console
Base Address: 7FFB7BA60000
```

### Debugging Example
```csharp
using NGMemory;

ulong registerValue = DebugHook.WaitForRegister("SonsOfTheForest", new IntPtr(0x7FFB7F1ED715), Register.Rax);
MessageBox.Show($"Register RAX Value: {registerValue:X}");
```
#### Result:
```Console
    Register RAX Value: 0x1C741FA3310
```

## Contributing
Feel free to fork this repository and contribute by submitting pull requests. Issues and feature requests are welcome!

## License
This library is licensed under the [MIT License](LICENSE).

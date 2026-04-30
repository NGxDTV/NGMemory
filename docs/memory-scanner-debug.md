# Memory, Scanner, Debug

This area is for external process memory access, pattern scanning, and debug hooks.

Use responsibly. Many processes require elevated permissions. Make sure your app architecture matches the target process architecture.

```csharp
using NGMemory;
using NGMemory.Easy;
```

## VAMemory

`VAMemory` is a typed memory read/write wrapper.

### Connect To A Process

```csharp
using System;
using NGMemory;

var memory = new VAMemory("notepad");

if (!memory.CheckProcess())
{
    Console.WriteLine("Process not found or not accessible");
    return;
}

Console.WriteLine("Base address: 0x" + memory.getBaseAddress.ToString("X"));
```

### Read Values

```csharp
IntPtr address = new IntPtr(0x12345678);

byte b = memory.ReadByte(address);
short i16 = memory.ReadInt16(address);
int i32 = memory.ReadInt32(address);
long i64 = memory.ReadInt64(address);
float f = memory.ReadFloat(address);
double d = memory.ReadDouble(address);
bool flag = memory.ReadBoolean(address);
string ascii = memory.ReadStringASCII(address, 128);
string unicode = memory.ReadStringUnicode(address, 128);
byte[] bytes = memory.ReadByteArray(address, 16);
```

### Write Values

```csharp
IntPtr address = new IntPtr(0x12345678);

memory.WriteByte(address, 0x90);
memory.WriteInt32(address, 1234);
memory.WriteFloat(address, 1.5f);
memory.WriteDouble(address, 12.34);
memory.WriteBoolean(address, true);
memory.WriteStringASCII(address, "hello");
memory.WriteStringUnicode(address, "hello");
memory.WriteByteArray(address, "90 90 90");
memory.WriteByteArray(address, new byte[] { 0x90, 0x90, 0x90 });
```

## Pattern Scanning With Scanner

```csharp
using System;
using System.Diagnostics;
using NGMemory;

Process process = Process.GetProcessesByName("notepad")[0];
var scanner = new Scanner(process);

IntPtr? result = scanner.ScanMemory("48 8B ?? ?? 89");

if (result.HasValue)
{
    Console.WriteLine("Found at 0x" + result.Value.ToInt64().ToString("X"));
}
```

Pattern format:

```text
90 90 ?? 48 8B ?
```

`??` and `?` are wildcards.

## EasyMemory Pattern Search

```csharp
IntPtr found = EasyMemory.FindPattern(
    processName: "notepad",
    pattern: "90 90 ?? 90");

if (found != IntPtr.Zero)
{
    Console.WriteLine("Found: " + found);
}
```

## EasyMemory Read String

```csharp
using System;
using System.Text;
using NGMemory.Easy;

IntPtr address = new IntPtr(0x12345678);
string value = EasyMemory.ReadString("notepad", address, 256, Encoding.UTF8);
Console.WriteLine(value);
```

## Module Base Address

```csharp
using NGMemory;

var moduleHelper = new Module();
IntPtr baseAddress = moduleHelper.getModuleBaseAddress("notepad.exe", "notepad");
Console.WriteLine("Module base: 0x" + baseAddress.ToInt64().ToString("X"));
```

## DebugHook

`DebugHook` can attach as debugger and wait for register values at a target address.

```csharp
using System;
using NGMemory;
using static NGMemory.Enums;

ulong rax = DebugHook.WaitForRegister(
    processName: "targetprocess",
    targetAddress: new IntPtr(0x12345678),
    register: Register.Rax);

Console.WriteLine("RAX: 0x" + rax.ToString("X"));
```

Easy wrapper:

```csharp
ulong rip = EasyDebugHook.WaitForRegister(
    "targetprocess",
    new IntPtr(0x12345678),
    Enums.Register.Rip);
```

Notes:

- Debug APIs can pause or alter target behavior.
- Elevated permissions may be required.
- Anti-debugging protections can break this.
- Always test on your own processes first.

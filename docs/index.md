# NGMemory Docs

NGMemory is a Windows-only .NET Framework helper library for process memory access, Win32 GUI automation, input simulation, screen analysis, overlays, and screen-capture protection.

## Requirements

- Windows desktop
- .NET Framework 4.7.2
- A WinForms/WPF/console app that can reference `NGMemory.dll`
- Matching bitness for many memory/debug scenarios: x64 app for x64 targets, x86 app for x86 targets
- Sufficient permissions for memory/debug APIs; some targets require running as administrator

## Main Namespaces

```csharp
using NGMemory;
using NGMemory.Easy;
using NGMemory.Overlay;
using NGMemory.CaptureProtection;
using NGMemory.WinInteropTools;
```

## Documentation Map

- [Quick Start](quick-start.md): install/reference and first tests
- [Easy API](easy-api.md): windows, controls, keyboard, mouse, waits, screen helpers
- [Capture Protection](capture-protection.md): `SetWindowDisplayAffinity`, protected areas, designer control
- [Overlays](overlays.md): attach overlay windows to target windows
- [Memory, Scanner, Debug](memory-scanner-debug.md): memory read/write, pattern scanning, debug hooks
- [WinInteropTools](wininterop-tools.md): lower-level Win32 wrappers
- [Screen Analysis](screen-analysis.md): screenshots, color/image matching
- [Troubleshooting](troubleshooting.md): common errors, permissions, DPI, capture caveats
- [API Reference](api-reference.md): compact method/class map
- [Release 1.0.8](release-1.0.8.md): release notes for the capture-protection update

## Minimal Sanity Test

Create a console app or WinForms app, reference `NGMemory.dll`, then run:

```csharp
using System;
using NGMemory.Easy;

class Program
{
    static void Main()
    {
        IntPtr hwnd = EasyWindow.Find("notepad");
        Console.WriteLine(hwnd == IntPtr.Zero ? "Notepad not found" : "Notepad HWND: " + hwnd);
    }
}
```

Start Notepad before running the test. If you see a non-zero handle, the reference and basic Win32 access work.

# Quick Start

## Add NGMemory To A Project

### From NuGet

```powershell
Install-Package NGMemory -Version 1.0.7
```

### From Source

```powershell
git clone <your-repo-url>
cd NGMemory
dotnet build NGMemory.sln
```

Reference the generated DLL from:

```text
NGMemory\bin\Debug\NGMemory.dll
NGMemory\bin\Release\NGMemory.dll
```

## Recommended Test Project

For most examples, use a .NET Framework 4.7.2 Console App or WinForms App:

```csharp
using System;
using NGMemory;
using NGMemory.Easy;

class Program
{
    static void Main()
    {
        Console.WriteLine("NGMemory test started");
    }
}
```

## Test 1: Find A Window

Start Notepad, then run:

```csharp
using System;
using NGMemory.Easy;

class Program
{
    static void Main()
    {
        IntPtr hwnd = EasyWindow.FindAndFocus("notepad");
        Console.WriteLine("HWND: 0x" + hwnd.ToInt64().ToString("X"));
    }
}
```

Expected result:

- Notepad is brought to the foreground.
- A non-zero HWND is printed.

## Test 2: Type Text

Start Notepad and focus the text area:

```csharp
using NGMemory.Easy;

class Program
{
    static void Main()
    {
        EasyWindow.FindAndFocus("notepad");
        EasyKeyboard.TypeText("Hello from NGMemory", 20);
    }
}
```

## Test 3: Capture A Screenshot

```csharp
using System.Drawing.Imaging;
using NGMemory.Easy;

class Program
{
    static void Main()
    {
        using (var bitmap = EasyScreen.CaptureScreen())
        {
            bitmap.Save("screen.png", ImageFormat.Png);
        }
    }
}
```

## Test 4: Create A Protected Area Control In WinForms

```csharp
using System;
using System.Windows.Forms;
using NGMemory.CaptureProtection;

public class MainForm : Form
{
    public MainForm()
    {
        var maskControl = new CaptureMaskControl
        {
            Dock = DockStyle.Right,
            Width = 380
        };

        Controls.Add(maskControl);
        maskControl.BindToOwner(this);
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new MainForm());
    }
}
```

Expected result:

- A designer-editable control appears.
- Click `Maske erstellen`.
- Use `Schwarz` mode for black capture masking.


# NGMemory v1.0.8 Release Notes

Hey there! Welcome to NGMemory v1.0.8.

This release adds a reusable Windows screen-capture protection toolkit on top of `SetWindowDisplayAffinity`, including a designer-editable Protected Area control, click-through masking, black capture masks, and full documentation with copyable test snippets.

## What's Included?

- Windows screen-capture protection helpers for top-level windows
- Reusable `CaptureMaskControl` for WinForms projects
- Programmatic Protected Area API for custom UI
- Black screenshot masking via `WDA_MONITOR`
- Click-through/no-activate protected areas
- Visible demo modes for placeholder text and simulated blur
- Full `docs` folder with examples for almost every public feature
- Version bump to `1.0.8`

## Getting Started

Build from source:

```powershell
dotnet build NGMemory.sln
```

Reference the generated DLL from `NGMemory\bin\Debug` or `NGMemory\bin\Release`.

Add the namespaces you need:

```csharp
using NGMemory;
using NGMemory.Easy;
using NGMemory.Overlay;
using NGMemory.CaptureProtection;
using NGMemory.WinInteropTools;
```

Full docs:

```text
docs/index.md
```

## What's New in 1.0.8?

### Screen-Capture Protection API

`EasyWindow` now exposes simple helpers around `SetWindowDisplayAffinity`:

```csharp
using NGMemory.Easy;

var result = EasyWindow.SetCaptureProtection(this.Handle, true);

Console.WriteLine(result.Affinity);
Console.WriteLine(result.ReturnValue);
Console.WriteLine(result.Win32Error);

EasyWindow.ClearCaptureProtection(this.Handle);
```

You can also choose the exact affinity:

```csharp
EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.None);
EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.Monitor);
EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.ExcludeFromCapture);
```

### Designer-Editable Protected Area Control

The new `CaptureMaskControl` is a WinForms `UserControl` with a `.Designer.cs`, so you can open and edit it in Visual Studio.

```csharp
using System.Windows.Forms;
using NGMemory.CaptureProtection;

public partial class MainForm : Form
{
    private readonly CaptureMaskControl maskControl = new CaptureMaskControl();

    public MainForm()
    {
        InitializeComponent();

        maskControl.Dock = DockStyle.Right;
        maskControl.Width = 380;
        Controls.Add(maskControl);

        maskControl.BindToOwner(this);
    }
}
```

The control can create a movable Protected Area and provides UI for:

- X/Y position
- width/height
- border thickness
- opacity, including 0
- topmost mode
- owner-bound or desktop positioning
- mouse interaction on/off
- effect selection
- affinity/debug status

### Black Capture Mask Mode

The `Schwarz` mode is the robust screenshot masking mode.

Recommended settings:

```text
Effect: Schwarz
Opacity: 0
Mausinteraktion aktiv: false
Maske vor Capture schuetzen: true
```

Behind the scenes, NGMemory keeps a tiny internal opacity when required so Windows still has a real window surface for capture replacement, while normal usage remains almost invisible.

### Click-Through Protected Areas

When mouse interaction is disabled, the protected area uses native extended window styles:

- `WS_EX_TRANSPARENT`
- `WS_EX_NOACTIVATE`

That lets the user interact with windows underneath as if the protected area was not there.

```csharp
var model = new CaptureMaskViewModel
{
    X = 200,
    Y = 150,
    Width = 480,
    Height = 260,
    Effect = CaptureMaskEffect.Black,
    ProtectFromCapture = true,
    OpacityPercent = 0,
    AllowInteraction = false,
    TopMost = true
};

var manager = new ProtectedAreaManager(this, model);
manager.Create();
```

### Programmatic Protected Area API

You can build your own UI and still reuse the core logic:

```csharp
using NGMemory.CaptureProtection;

CaptureMaskViewModel model = new CaptureMaskViewModel
{
    X = 100,
    Y = 100,
    Width = 400,
    Height = 220,
    Effect = CaptureMaskEffect.Black,
    ProtectFromCapture = true,
    OpacityPercent = 0,
    AllowInteraction = false
};

ProtectedAreaManager manager = new ProtectedAreaManager(this, model);
manager.Create();

// Move it later:
model.X = 300;
model.Y = 200;

// Remove it:
manager.Remove();
```

### Placeholder And Blur Demo Modes

`Placeholder-Text` and `Simulierter Blur` are included as visible demo modes.

Important: normal Win32 APIs cannot create screenshot-only text or real blur over arbitrary foreign windows while keeping it invisible to the user. These modes draw real visible pixels, so screenshot tools can capture them.

```csharp
model.Effect = CaptureMaskEffect.PlaceholderText;
model.ProtectFromCapture = false;
model.BorderOnlyForUser = false;
model.OpacityPercent = 90;
```

```csharp
model.Effect = CaptureMaskEffect.SimulatedBlurDemoOnly;
model.ProtectFromCapture = false;
model.BorderOnlyForUser = false;
model.OpacityPercent = 90;
```

### Full Documentation Folder

The new `docs` folder includes:

- `docs/index.md`
- `docs/quick-start.md`
- `docs/easy-api.md`
- `docs/capture-protection.md`
- `docs/overlays.md`
- `docs/memory-scanner-debug.md`
- `docs/wininterop-tools.md`
- `docs/screen-analysis.md`
- `docs/troubleshooting.md`
- `docs/api-reference.md`

## Example: Protect A Form From Capture

```csharp
using System;
using System.Windows.Forms;
using NGMemory.Easy;

public partial class MainForm : Form
{
    private void protectButton_Click(object sender, EventArgs e)
    {
        var result = EasyWindow.SetCaptureProtection(this.Handle, true);

        MessageBox.Show(
            "Return: " + result.ReturnValue +
            "\nWin32: " + result.Win32Error +
            "\nAffinity: " + result.Affinity);
    }

    private void unprotectButton_Click(object sender, EventArgs e)
    {
        EasyWindow.ClearCaptureProtection(this.Handle);
    }
}
```

## Example: Add The Protected Area UI To A Panel

```csharp
using NGMemory.CaptureProtection;

CaptureMaskControl captureMaskControl = new CaptureMaskControl
{
    Dock = DockStyle.Fill
};

captureMaskPanel.Controls.Add(captureMaskControl);
captureMaskControl.BindToOwner(this);
```

## Example: Create A Black Click-Through Mask

```csharp
using NGMemory.CaptureProtection;

var model = new CaptureMaskViewModel
{
    X = 250,
    Y = 250,
    Width = 500,
    Height = 240,
    Effect = CaptureMaskEffect.Black,
    ProtectFromCapture = true,
    OpacityPercent = 0,
    AllowInteraction = false,
    TopMost = true
};

var manager = new ProtectedAreaManager(this, model);
manager.Create();
```

## Notes And Limits

- `SetWindowDisplayAffinity` only works on top-level windows owned by the current process.
- It does not protect normal child controls individually.
- `WDA_EXCLUDEFROMCAPTURE` is correctly supported starting with Windows 10 version 2004.
- Protection does not stop camera recordings, special drivers, kernel capture, or privileged capture tools.
- Real blur of foreign windows is not reliably possible with normal Win32/WinForms APIs.

## Feedback & Support

Found a bug or have an idea? Open an issue and include:

- Windows version
- capture tool used
- affinity mode
- whether the window is owned by your process
- `ReturnValue` and `Win32Error`

Thanks for using NGMemory. Happy coding.

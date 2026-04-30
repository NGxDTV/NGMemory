# Capture Protection

NGMemory exposes two levels of capture protection:

- `NGMemory.Easy.EasyWindow`: direct `SetWindowDisplayAffinity` helpers for top-level windows
- `NGMemory.CaptureProtection`: reusable protected-area UI and logic

## Important Windows Rules

`SetWindowDisplayAffinity` is a Win32 API from `user32.dll`.

It can:

- apply an affinity mode to a top-level window owned by the current process
- make many screenshot/stream/capture tools show a black region or exclude a window

It cannot:

- protect normal child controls individually
- blur foreign windows underneath your app
- protect against physical camera recording
- protect against special drivers, kernel capture, or malicious privileged tooling

`WDA_EXCLUDEFROMCAPTURE` is correctly supported starting with Windows 10 version 2004.

## Direct Window Protection

```csharp
using System;
using NGMemory.Easy;

IntPtr hwnd = this.Handle; // inside a WinForms Form

var result = EasyWindow.SetCaptureProtection(hwnd, true);
Console.WriteLine(result.ReturnValue);
Console.WriteLine(result.Win32Error);

EasyWindow.ClearCaptureProtection(hwnd);
```

## Choose A Specific Affinity

```csharp
using NGMemory.Easy;

EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.None);
EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.Monitor);
EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.ExcludeFromCapture);
```

## Designer-Editable Protected Area Control

Use this when you want an end-user configurable mask/area UI.

```csharp
using System;
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

The control provides:

- create/remove mask
- X/Y/width/height
- border thickness
- preview opacity, including 0
- topmost
- mouse interaction on/off
- bind to owner form or desktop coordinates
- effect mode
- debug information

## Protected Area Modes

### Black

Black is the practical screenshot-only mode.

Behavior:

- uses a real filled top-level window
- applies `WDA_MONITOR`
- allows opacity 0 in the UI
- internally uses 1% opacity when needed so the capture pipeline still sees a real window surface
- mouse interaction can be disabled so clicks pass through to windows underneath

Recommended settings:

```text
Effect: Schwarz
Opacity: 0
Mausinteraktion aktiv: false
Maske vor Capture schuetzen: true
```

### Placeholder Text

This is a visible demo mode.

It draws real pixels:

```text
CAPTURE PLACEHOLDER
```

Because screenshots can only capture real pixels, this mode is visible to the user too. It is not a screenshot-only effect.

### Simulated Blur

This is also a visible demo mode. It does not blur arbitrary foreign windows. It draws a blur-like placeholder pattern for content your app controls.

## Programmatic Protected Area

You can skip the UI and manage a protected area yourself:

```csharp
using System.Drawing;
using System.Windows.Forms;
using NGMemory.CaptureProtection;

public class MainForm : Form
{
    private CaptureMaskViewModel model;
    private ProtectedAreaManager manager;

    protected override void OnLoad(System.EventArgs e)
    {
        base.OnLoad(e);

        model = new CaptureMaskViewModel
        {
            X = 200,
            Y = 200,
            Width = 480,
            Height = 260,
            Effect = CaptureMaskEffect.Black,
            ProtectFromCapture = true,
            OpacityPercent = 0,
            AllowInteraction = false,
            TopMost = true
        };

        manager = new ProtectedAreaManager(this, model);
        manager.Create();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        manager?.Dispose();
        base.OnFormClosing(e);
    }
}
```

## Debug Result

Every affinity call returns `WindowCaptureProtectionResult`:

```csharp
var result = model.LastProtectionResult;
Console.WriteLine(result.WindowHandle);
Console.WriteLine(result.Affinity);
Console.WriteLine(result.ReturnValue);
Console.WriteLine(result.Win32Error);
```

Common error reasons:

- HWND is zero or invalid
- window is not owned by current process
- unsupported Windows version
- capture method ignores display affinity


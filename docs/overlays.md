# Overlays

The overlay system creates lightweight WinForms overlay windows attached to target windows.

```csharp
using NGMemory.Easy;
using NGMemory.Overlay;
```

## Create One Overlay For A Window

```csharp
using System;
using System.Drawing;
using NGMemory.Easy;
using NGMemory.Overlay;

IntPtr hwnd = EasyWindow.GetMainWindow("notepad");

if (hwnd != IntPtr.Zero)
{
    var manager = new OverlayManager();

    manager.CreateOverlay(hwnd, overlay =>
    {
        overlay.Configure()
            .WithSize(220, 80)
            .WithPosition(OverlayPosition.BottomRight)
            .WithOffset(12, 12)
            .WithBackgroundColor(Color.Transparent)
            .WithLabel("Overlay active", 10, 10, Color.LimeGreen)
            .WithButton("Click", 10, 35, 90, 28, (s, e) =>
            {
                Console.WriteLine("Overlay button clicked");
            });
    });
}
```

## Create Overlay By Process And Title

```csharp
var manager = new OverlayManager();

manager.CreateOverlayForWindow(
    processName: "notepad",
    windowTitleFilter: "Untitled",
    configure: overlay =>
    {
        overlay.Configure()
            .WithSize(160, 40)
            .WithPosition(OverlayPosition.TopRight)
            .WithLabel("Found", 8, 8, Color.White);
    });
```

## Scan For Matching Windows

```csharp
var manager = new OverlayManager();

int count = manager.CreateOverlaysForAllMatching(
    processName: "myapp",
    windowTitleFilter: "",
    configure: overlay =>
    {
        overlay.Configure()
            .WithSize(120, 40)
            .WithPosition(OverlayPosition.TopLeft)
            .WithLabel("Overlay", 8, 8, Color.Yellow);
    },
    windowType: TargetWindowType.All);

Console.WriteLine("Created overlays: " + count);
```

## Auto-Scan With Timer

```csharp
var manager = new OverlayManager();

Timer timer = manager.StartWindowScan(
    processName: "myapp",
    windowTitleFilter: "Dialog",
    interval: 1000,
    configure: overlay =>
    {
        overlay.Configure()
            .WithSize(180, 60)
            .WithPosition(OverlayPosition.Center)
            .WithLabel("Auto overlay", 10, 10, Color.Cyan);
    },
    windowType: TargetWindowType.DialogWindows);
```

Stop scanning:

```csharp
timer.Stop();
timer.Dispose();
manager.RemoveAllOverlays();
```

## Remove Overlays

```csharp
manager.RemoveOverlay(hwnd);
manager.RemoveAllOverlays();
```

## Alt-Tab Styling

```csharp
using NGMemory.Overlay;

OverlayStyleHelper.HideFromAltTab(overlay.Handle);
OverlayStyleHelper.RestoreAltTab(overlay.Handle);
OverlayStyleHelper.ForceShowInAltTab(overlay.Handle);
```

## Notes

- Overlays require a WinForms message loop.
- If the target window moves, the overlay must be synchronized by the overlay implementation/timer.
- Some games or elevated apps may block regular overlay behavior.
- For capture masking, prefer `NGMemory.CaptureProtection`.


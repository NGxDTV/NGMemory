# Troubleshooting

## `IntPtr.Zero` When Finding A Window

Checklist:

- Process name should not include `.exe`.
- The process must already be running.
- Some apps have no `MainWindowHandle`; enumerate windows with `EasyGuiInterop.EnumerateProcessWindowHandles`.
- Try partial title search:

```csharp
IntPtr hwnd = EasyWindow.Find("notepad", "Untitled");
```

## Control ID Does Not Work

Classic helpers use Win32 control IDs. They do not work with every modern UI framework.

Try:

```csharp
var children = EasyWindow.GetAllChildWindows(parent);
foreach (IntPtr child in children)
{
    Console.WriteLine(EasyGuiInterop.GetClassName(child));
    Console.WriteLine(EasyGuiInterop.GetWindowTitle(child));
}
```

If controls are custom-drawn or browser-based, message-based automation may not work.

## Memory Read/Write Fails

Checklist:

- Run as administrator.
- Match x86/x64 architecture.
- Make sure the address is valid for the current process instance.
- Use `Scanner` or module base + offsets instead of hard-coded absolute addresses where possible.
- Antivirus/anti-cheat/anti-debug protections may block access.

## Pattern Scan Finds Nothing

Try:

- Verify the process name.
- Verify the pattern bytes.
- Use wildcards for relocatable bytes: `??`.
- Search a broader range.
- Confirm the module is loaded.

## `SetWindowDisplayAffinity` Returns False

Common reasons:

- HWND is not a top-level window.
- HWND belongs to another process.
- Windows version does not support the requested mode.
- The window has not created its handle yet.

Test:

```csharp
var result = EasyWindow.SetDisplayAffinity(this.Handle, WindowDisplayAffinity.Monitor);
Console.WriteLine(result.ReturnValue);
Console.WriteLine(result.Win32Error);
```

## Capture Mask Does Not Show Black

For `CaptureMaskControl` black mode:

- Use effect `Schwarz`.
- Keep `Maske vor Capture schuetzen` enabled.
- Set opacity to 0 if you want it nearly invisible.
- Do not use border-only for black mode; the control disables it because a transparent interior has no surface for capture to replace.
- Some capture methods ignore display affinity.

## Placeholder Or Blur Is Visible To The User

That is expected.

Placeholder and blur are demo modes. A screenshot can only capture pixels that are actually drawn. Normal Win32 APIs cannot create screenshot-only text/blur over arbitrary foreign windows while remaining invisible to the user.

## Mouse Still Hits The Mask

In `CaptureMaskControl`, disable:

```text
Mausinteraktion aktiv
```

This applies native click-through/no-activate styles:

- `WS_EX_TRANSPARENT`
- `WS_EX_NOACTIVATE`

Then move/resize by editing X/Y/width/height in the control.

## DPI Position Looks Wrong

Enable DPI awareness in your app before showing forms:

```csharp
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new MainForm());
```

For best results, add PerMonitorV2 DPI awareness in app config or manifest. WinForms coordinates can otherwise be scaled by Windows.

## Overlay Does Not Follow Target

Overlays need a message loop and synchronization. If the target moves/resizes, use `OverlayManager.StartWindowScan` or your own timer to update/recreate overlays.


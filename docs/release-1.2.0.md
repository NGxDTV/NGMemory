# NGMemory v1.2.0 Release Notes

Version 1.2.0 adds `NGMemory.Remote`, a set of building blocks for driving foreign Win32/MFC windows reliably from background threads.

## Added

- `RemoteWindows` to find top-level windows, captioned children (MDI), controls by ID, and to read titles, classes, bounds, and bitness
- `RemoteText` to read and set control text with parent notification and verification
- `RemoteButton` for non-blocking clicks and checkbox state
- `RemoteCombo` to read and select combo box entries
- `RemoteTab` to read tab labels and switch tabs (property sheets and plain tab controls)
- `RemoteListView` and `RemoteListContent` to read a full report-view list and select rows
- `RemoteKeys` to post key presses to a window

Every call uses `SendMessageTimeout` with a 4 second limit or `PostMessage`, never shows a message box, and returns `false`, an empty value, `-1`, or `IntPtr.Zero` for dead or invalid windows instead of throwing.

## Compatibility

Existing classes, signatures, and behavior are unchanged. The project continues to target .NET Framework 4.7.2.

A 32-bit target works from x86 and x64 processes, a 64-bit target from x64 processes. See [Remote Control](remote.md) for details.

## Documentation

- [Remote Control](remote.md)

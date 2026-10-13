# Remote Control

`NGMemory.Remote` drives the windows and standard controls of another process. It is meant for automation that runs on background threads and must not hang or crash when the target is busy, shows a modal dialog, or closes.

```csharp
using NGMemory.Remote;
```

## Guarantees

- Synchronous, stateless, and safe to call from any thread
- Messages are sent with `SendMessageTimeout` (`SMTO_ABORTIFHUNG`, 4000 ms) or posted; nothing waits without a limit
- No message boxes and no exceptions for dead or invalid windows: the result is `false`, `""`, an empty array, `-1`, or `IntPtr.Zero`
- No scrolling and no clipboard access

## Bitness

Structures placed in the target process follow the pointer size of the target, not of the caller.

| Caller | Target | Result |
| --- | --- | --- |
| x86 | 32-bit | works |
| x64 | 32-bit | works |
| x64 | 64-bit | works |
| x86 | 64-bit | methods that need memory in the target fail cleanly |

Methods that need memory in the target: `RemoteTab.Labels`, `RemoteTab.IndexOf`, the click fallback of `RemoteTab.Select`, `RemoteListView.Read`, and `RemoteListView.Select`. All other methods only send messages and also work from x86 against a 64-bit target.

```csharp
bool is32Bit = RemoteWindows.Is32Bit(window);
```

## RemoteWindows

```csharp
IntPtr order = RemoteWindows.Find("twe", null, "Auftrag");
IntPtr field = RemoteWindows.Control(order, 1001);
```

- `TopLevel(processName)`: visible top-level windows of all processes with that name (without `.exe`)
- `Captioned(processName)`: top-level windows plus visible descendants with `WS_CAPTION`, for example MDI children
- `Find(processName, className, titleContains)` / `FindAll(...)`: search in `Captioned`; `null` matches anything, class is compared exactly, title by substring, both ignoring case
- `IsRunning(processName)`
- `Title(window)`, `ClassName(window)`, `IsAlive(window)`, `IsEnabled(window)`, `Owner(window)`
- `Descendants(parent)`: all child windows at any depth
- `Control(parent, controlId)`: direct child with that ID, otherwise the first visible descendant with that ID
- `ControlId(control)`, `Is32Bit(window)`
- `Close(window)`: posts `WM_CLOSE`
- `Bounds(window)`: screen coordinates; `BoundsIn(control, ancestor)`: relative to the client area of `ancestor`

## RemoteText

```csharp
bool stored = RemoteText.Set(field, "205/55 R16");
string current = RemoteText.Get(field);
```

`Set` sends `WM_SETTEXT`, notifies the parent with `EN_CHANGE` and `EN_KILLFOCUS`, and then waits up to `verifyMs` until the control reports exactly that text. With `verifyMs` 0 it returns `true` as soon as the text was delivered.

## RemoteButton

```csharp
if (RemoteButton.Click(RemoteWindows.Control(order, 1)))
{
    IntPtr dialog = RemoteWindows.Find("twe", "#32770", "Speichern");
}

RemoteButton.SetChecked(RemoteWindows.Control(order, 1020), true);
```

- `Click(button)`: `false` if the button is missing or disabled; otherwise posts `BM_CLICK` and returns at once, even if the click opens a modal dialog
- `IsChecked(button)`
- `SetChecked(button, value, verifyMs)`: does nothing if the state already matches; otherwise clicks and waits; if that has no effect it sets the state with `BM_SETCHECK` and notifies the parent with `BN_CLICKED`

## RemoteCombo

```csharp
string[] entries = RemoteCombo.Items(combo);
bool chosen = RemoteCombo.Select(combo, "Sommerreifen");
string active = RemoteCombo.Selected(combo);
```

`Select` matches the trimmed text exactly, ignoring case, and returns `false` if there is no such entry. It sets the selection and notifies the parent with `CBN_SELENDOK` and `CBN_SELCHANGE`. An entry that is already selected is left alone.

## RemoteTab

```csharp
int index = RemoteTab.IndexOf(tabs, "2-Positionen");
bool opened = RemoteTab.Select(tabs, index);
```

- `Count(tabControl)`, `Current(tabControl)`
- `Labels(tabControl)`: labels without `&`
- `IndexOf(tabControl, label)`: exact match first, then prefix, then substring; ignores `&` and case
- `Select(tabControl, index, verifyMs)`: sends `PSM_SETCURSEL` to the parent, which switches pages of a property sheet. If the tab is still not active, it posts a left click on the tab so the control itself raises `TCN_SELCHANGING` and `TCN_SELCHANGE`.

Windows does not deliver `WM_NOTIFY` across process boundaries, so the tab change cannot be announced to the parent from outside. The click fallback needs the tab to be inside the visible part of the tab strip.

## RemoteListView

```csharp
RemoteListContent list = RemoteListView.Read(listView);
int column = list.ColumnIndex("Auftrag");

for (int row = 0; row < list.Rows.Count; row++)
{
    if (list.Cell(row, column) == "4711") RemoteListView.Select(listView, row);
}
```

- `Count(listView)`
- `Read(listView, maxRows)`: headers and all cells of a report view through one block of memory in the target; a failed read returns empty content
- `Selected(listView)`: first selected row or `-1`
- `Select(listView, index)`: clears the selection, selects and focuses the row, scrolls it into view, and checks the result

`RemoteListContent` exposes `Columns`, `Rows`, `ColumnIndex(header)` (exact match, then substring, ignoring case; the first one wins for duplicate headers) and `Cell(row, column)` (`""` outside the content).

## RemoteKeys

```csharp
RemoteKeys.Press(order, 0x74);
```

`Press` posts `WM_KEYDOWN` and `WM_KEYUP` with the matching scan code. `0x74` is F5.

# API Reference

This page is a compact map of the public NGMemory surface. See the topic pages for runnable examples.

## `NGMemory.Easy`

### `EasyWindow`

- `GetMainWindow(string processName, string partialTitle = null)`
- `GetAllChildWindows(IntPtr parentHandle)`
- `GetChildByTitle(IntPtr parentHandle, string partialTitle)`
- `FocusWindow(IntPtr windowHandle)`
- `FindAndFocus(string processName, string partialTitle = null)`
- `Find(string processName, string partialTitle = null)`
- `SetCaptureProtection(IntPtr windowHandle, bool protect)`
- `SetDisplayAffinity(IntPtr windowHandle, WindowDisplayAffinity affinity)`
- `ClearCaptureProtection(IntPtr windowHandle)`

### `EasyButton`

- `Click(IntPtr window, int controlId)`
- `ClickAsync(IntPtr window, int controlId)`

### `EasyTextBox`

- `GetText(IntPtr window, int controlId)`
- `SetText(IntPtr window, int controlId, string text)`
- `ClearText(IntPtr window, int controlId)`

### `EasyCheckBox`

- `IsChecked(IntPtr window, int controlId)`
- `SetChecked(IntPtr window, int controlId, bool state)`
- `ToggleState(IntPtr window, int controlId)`
- `ClickCheckBox(IntPtr window, int controlId)`

### `EasyComboBox`

- `GetSelectedItem(IntPtr comboBoxHandle)`
- `GetItems(IntPtr comboBoxHandle)`
- `SelectItemByString(IntPtr comboBoxHandle, string itemText)`
- `SelectItemByIndex(IntPtr comboBoxHandle, int index)`

### `EasyElementFinder`

- `FindElement(IntPtr parentWindow, string className = null, string windowText = null, int controlId = -1)`
- `GetWindowRect(IntPtr hwnd)`

### `EasyGuiInterop`

- `SetText(IntPtr dialogHandle, int controlId, string text)`
- `GetWindowTitle(IntPtr hWnd)`
- `GetWindowByClassName(IEnumerable<IntPtr> windows, string className)`
- `GetWindowByClassName(IntPtr window, string className)`
- `EnumerateProcessWindowHandles(Process process)`
- `GetControlHandle(IntPtr hWnd, int controlId)`
- `GetWindowByContainsName(IEnumerable<IntPtr> handles, string titlePart)`
- `GetAllWindows(IEnumerable<IntPtr> handles)`
- `ShowProcessWindow(Process[] processes)`
- `GetHandleByIndex(int index, List<IntPtr> handleList)`
- `GetChildWindows(IntPtr parent)`
- `GetWindowByContainsName(Process[] processList, string windowName)`
- `GetClassName(IntPtr hWnd)`

### `EasyKeyboard` And `EasyPressKey`

- `EasyKeyboard.TypeText(string text, int delayBetweenChars = 10)`
- `EasyKeyboard.TypeTextAsync(string text, int delayBetweenChars = 10)`
- `EasyKeyboard.SendCtrlC()`
- `EasyKeyboard.SendCtrlV()`
- `EasyKeyboard.CharToKeyCode(char c)`
- `EasyPressKey.PressKeys(bool async, params KeyCode[] scanCodes)`
- `EasyPressKey.PressKeys(params KeyCode[] scanCodes)`
- `EasyPressKey.PressKeysAsync(params KeyCode[] scanCodes)`
- `EasyPressKey.PressKeysWithDelay(int delayBetweenKeysMs, params KeyCode[] scanCodes)`

### `EasyMouse`

- `MoveTo(int x, int y)`
- `MoveWithHumanMotion(int targetX, int targetY, int duration = 500)`
- `Click(MouseButton button = MouseButton.Left)`
- `ClickAt(int x, int y, MouseButton button = MouseButton.Left)`
- `HumanClickAt(int x, int y, bool doubleClick = false, MouseButton button = MouseButton.Left, int moveTime = 500)`
- `DoubleClick(MouseButton button = MouseButton.Left)`
- `DragAndDrop(int fromX, int fromY, int toX, int toY)`

### `EasyScreen` And `EasyScreenAnalysis`

- `EasyScreen.CaptureScreen()`
- `EasyScreen.CaptureRegion(int x, int y, int width, int height)`
- `EasyScreen.CaptureWindow(IntPtr hWnd)`
- `EasyScreen.FindColor(Color targetColor, Rectangle searchArea, int tolerance = 0)`
- `EasyScreenAnalysis.FindAllColorMatches(Color targetColor, Rectangle searchArea, int tolerance = 0)`
- `EasyScreenAnalysis.CompareImages(Bitmap image1, Bitmap image2, int samplingRate = 10)`
- `EasyScreenAnalysis.FindImageOnScreen(Bitmap templateImage, Rectangle searchArea, double minSimilarity = 90)`

### `EasyMemory`, `EasyDebugHook`, `EasyWait`

- `EasyMemory.FindPattern(string processName, string pattern, IntPtr startAddress = default, IntPtr endAddress = default)`
- `EasyMemory.ReadString(string processName, IntPtr address, int maxLength = 1024, Encoding encoding = null)`
- `EasyDebugHook.WaitForRegister(string processName, IntPtr targetAddress, Enums.Register register)`
- `EasyDebugHook.WaitForRegister(int processID, IntPtr targetAddress, Enums.Register register)`
- `EasyWait.Until(Func<bool> condition, int timeout = 10000, int checkInterval = 100)`
- `EasyWait.ForDuration(int milliseconds)`
- `EasyWait.RetryUntilSuccess(Action action, Func<bool> successCheck, int maxAttempts = 3, int delayBetweenAttempts = 500)`

### `EasySysListView32`

- `GetColumnCount(IntPtr listViewHandle)`
- `GetItems(IntPtr listViewHandle, int columnCount)`
- `GetItems(IntPtr listViewHandle, bool autoColumnCount)`
- `ReadItemText(IntPtr listViewHandle, int itemIndex, int subIndex)`
- `GetItemCount(IntPtr listViewHandle)`
- `GetAllRowsAsStrings(IntPtr listViewHandle, int columnCount)`
- `GetAllRowsAsStrings(IntPtr listViewHandle, bool autoColumnCount)`
- `InsertItem(IntPtr listViewHandle, int index, string text)`
- `RemoveItem(IntPtr listViewHandle, int index)`
- `ClearAllItems(IntPtr listViewHandle)`
- `SearchSysListView32InWindow(IntPtr listViewWindowHandle)`
- `GetListViewItemByName(IntPtr listViewHandle, string ContainsName)`
- `SetItemText(IntPtr listViewHandle, int itemIndex, string newText)`
- `SelectItemByIndex(IntPtr listViewHandle, int index)`
- `CopyAllRowsTSV(IntPtr hWnd, bool withHeader = false)`
- `SelectItemByName(IntPtr listViewHandle, string ContainsName)`

## `NGMemory.CaptureProtection`

- `CaptureMaskControl`: designer-editable WinForms control
- `CaptureMaskViewModel`: reusable state object
- `ProtectedAreaManager`: programmatic create/remove/sync
- `ProtectedAreaWindow`: top-level draggable protected window
- `CaptureMaskProtectionHelper.Apply(IntPtr hwnd, bool protect, CaptureMaskEffect effect)`
- `CaptureMaskEffect.Black`
- `CaptureMaskEffect.PlaceholderText`
- `CaptureMaskEffect.SimulatedBlurDemoOnly`

## `NGMemory.Overlay`

- `OverlayManager.CreateOverlay(IntPtr targetWindow, Action<EasyOverlay> configure = null)`
- `OverlayManager.CreateOverlayForWindow(string processName, string windowTitleFilter, Action<EasyOverlay> configure = null)`
- `OverlayManager.CreateOverlaysForAllMatching(...)`
- `OverlayManager.RemoveOverlay(IntPtr targetWindow)`
- `OverlayManager.RemoveAllOverlays()`
- `OverlayManager.StartWindowScan(...)`
- `OverlayConfiguration.WithSize(int width, int height)`
- `OverlayConfiguration.WithPosition(OverlayPosition position)`
- `OverlayConfiguration.WithOffset(int x, int y)`
- `OverlayConfiguration.WithBackgroundColor(Color color)`
- `OverlayConfiguration.WithLabel(...)`
- `OverlayConfiguration.WithButton(...)`
- `OverlayConfiguration.WithAutoPosition(bool enabled, int interval = 300)`
- `OverlayConfiguration.WithControl(Control control)`
- `OverlayConfiguration.WithCustomization(Action<EasyOverlay> customize)`

## Core

- `VAMemory`: typed read/write memory helper
- `Scanner`: pattern scan process memory
- `Module`: resolve module base address
- `DebugHook`: wait for debug/register values
- `User32`, `Kernel32`, `Constants`, `Enums`, `Structures`, `MessageHelper`: interop definitions


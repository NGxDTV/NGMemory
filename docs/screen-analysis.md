# Screen Analysis

`NGMemory.Easy` provides simple screenshot and image analysis helpers.

```csharp
using System.Drawing;
using System.Drawing.Imaging;
using NGMemory.Easy;
```

## Capture Entire Screen

```csharp
using (Bitmap screen = EasyScreen.CaptureScreen())
{
    screen.Save("screen.png", ImageFormat.Png);
}
```

## Capture Region

```csharp
using (Bitmap region = EasyScreen.CaptureRegion(100, 100, 400, 300))
{
    region.Save("region.png", ImageFormat.Png);
}
```

## Capture Window

```csharp
IntPtr hwnd = EasyWindow.Find("notepad");

using (Bitmap window = EasyScreen.CaptureWindow(hwnd))
{
    window.Save("notepad.png", ImageFormat.Png);
}
```

## Find One Color

```csharp
Rectangle area = new Rectangle(0, 0, 1920, 1080);
Point? point = EasyScreen.FindColor(Color.Red, area, tolerance: 10);

if (point.HasValue)
{
    Console.WriteLine("Found red near " + point.Value);
}
```

## Find All Color Matches

```csharp
Rectangle area = new Rectangle(0, 0, 1920, 1080);
var points = EasyScreenAnalysis.FindAllColorMatches(Color.LimeGreen, area, tolerance: 15);

Console.WriteLine("Matches: " + points.Count);
```

## Compare Images

```csharp
using (Bitmap a = new Bitmap("before.png"))
using (Bitmap b = new Bitmap("after.png"))
{
    double similarity = EasyScreenAnalysis.CompareImages(a, b, samplingRate: 10);
    Console.WriteLine("Similarity: " + similarity);
}
```

## Find Template On Screen

```csharp
using (Bitmap template = new Bitmap("button-template.png"))
{
    Rectangle searchArea = new Rectangle(0, 0, 1920, 1080);
    Point? match = EasyScreenAnalysis.FindImageOnScreen(template, searchArea, minSimilarity: 90);

    if (match.HasValue)
    {
        EasyMouse.ClickAt(match.Value.X, match.Value.Y);
    }
}
```

## Notes

- `Graphics.CopyFromScreen` captures what the desktop compositor provides to the process.
- Protected windows may appear black or absent depending on display affinity and capture method.
- Color matching is simple RGB tolerance matching. It is not OCR or computer vision.
- Template matching can be slow on large areas; keep search regions small.


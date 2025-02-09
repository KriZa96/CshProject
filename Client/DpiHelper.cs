using System.Runtime.InteropServices;

namespace Client;

public static class DpiHelper
{
    // Radi samo za windows 10 na dalje
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiFlag);

    private static readonly IntPtr DpiAwarenessContextPerMonitorAwareV2 = new(-4);

    public static void SetDpiAwareness()
    {
        try
        {
            SetProcessDpiAwarenessContext(DpiAwarenessContextPerMonitorAwareV2);
            Console.WriteLine("Successfully set the process dpi awareness context.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set DPI awareness: {ex.Message}");
        }
    }
}
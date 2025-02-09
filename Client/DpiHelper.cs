using System.Runtime.InteropServices;

namespace Client;

public static class DpiHelper
{
    // Radi samo za windows 10 na dalje
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

    private const int DpiAwarenessContextPerMonitorAwareV2 = 0x00000002;

    public static void SetDpiAwareness()
    {
        try
        {
            SetProcessDpiAwarenessContext(DpiAwarenessContextPerMonitorAwareV2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set DPI awareness: {ex.Message}");
        }
    }
}
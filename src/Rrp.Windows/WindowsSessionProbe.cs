using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Rrp.Windows;

public sealed record ActiveWindowInfo(nint Handle, int ProcessId, string ProcessName, string Title);

public sealed partial class WindowsSessionProbe
{
    public ActiveWindowInfo? GetActiveWindow()
    {
        var handle = GetForegroundWindow();
        if (handle == nint.Zero) return null;
        _ = GetWindowThreadProcessId(handle, out var processId);
        var title = new StringBuilder(1024);
        _ = GetWindowText(handle, title, title.Capacity);
        string processName;
        try { processName = Process.GetProcessById((int)processId).ProcessName; }
        catch { processName = "unknown"; }
        return new(handle, (int)processId, processName, title.ToString());
    }

    [LibraryImport("user32.dll")]
    private static partial nint GetForegroundWindow();
    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(nint hWnd, out uint processId);
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GetWindowText(nint hWnd, StringBuilder text, int count);
}

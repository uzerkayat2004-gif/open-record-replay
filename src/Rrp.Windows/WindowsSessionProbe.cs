using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Rrp.Windows;

public sealed record ActiveWindowInfo(nint Handle, int ProcessId, string ProcessName, string Title);

public sealed class WindowsSessionProbe
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

    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);
}

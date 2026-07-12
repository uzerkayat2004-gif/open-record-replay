using System.Diagnostics;using System.Runtime.InteropServices;using System.Text;
namespace Rrp.Windows;
public sealed record ActiveWindowInfo(nint Handle,int ProcessId,string ProcessName,string Title);
public sealed partial class WindowsSessionProbe{public ActiveWindowInfo? GetActiveWindow(){var h=GetForegroundWindow();if(h==nint.Zero)return null;_ = GetWindowThreadProcessId(h,out var pid);var title=new StringBuilder(1024);_ = GetWindowText(h,title,title.Capacity);string name;try{name=Process.GetProcessById((int)pid).ProcessName;}catch{name="unknown";}return new(h,(int)pid,name,title.ToString());}
[LibraryImport("user32.dll")]static partial nint GetForegroundWindow();[LibraryImport("user32.dll")]static partial uint GetWindowThreadProcessId(nint hWnd,out uint processId);[LibraryImport("user32.dll",StringMarshalling=StringMarshalling.Utf16)]static partial int GetWindowText(nint hWnd,StringBuilder text,int count);}

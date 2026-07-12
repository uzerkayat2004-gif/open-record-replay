using System.Runtime.InteropServices;

namespace Rrp.Windows;

internal static class NativeMethods
{
    internal const int WH_KEYBOARD_LL=13, WH_MOUSE_LL=14, WM_QUIT=0x0012;
    internal const int WM_KEYDOWN=0x0100, WM_KEYUP=0x0101, WM_SYSKEYDOWN=0x0104, WM_SYSKEYUP=0x0105;
    internal const int WM_MOUSEMOVE=0x0200, WM_LBUTTONDOWN=0x0201, WM_LBUTTONUP=0x0202, WM_RBUTTONDOWN=0x0204, WM_RBUTTONUP=0x0205, WM_MOUSEWHEEL=0x020A;
    internal const uint INPUT_MOUSE=0, INPUT_KEYBOARD=1, KEYEVENTF_KEYUP=0x0002, KEYEVENTF_UNICODE=0x0004;
    internal const uint MOUSEEVENTF_MOVE=0x0001, MOUSEEVENTF_LEFTDOWN=0x0002, MOUSEEVENTF_LEFTUP=0x0004, MOUSEEVENTF_RIGHTDOWN=0x0008, MOUSEEVENTF_RIGHTUP=0x0010, MOUSEEVENTF_WHEEL=0x0800, MOUSEEVENTF_ABSOLUTE=0x8000, MOUSEEVENTF_VIRTUALDESK=0x4000;
    internal const uint LLMHF_INJECTED=0x1, LLKHF_INJECTED=0x10;
    internal static readonly nuint RrpMarker=0x52525001;

    internal delegate nint HookProc(int nCode,nint wParam,nint lParam);
    [DllImport("user32.dll",SetLastError=true)] internal static extern nint SetWindowsHookEx(int idHook,HookProc proc,nint module,uint threadId);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")] internal static extern nint CallNextHookEx(nint hook,int code,nint wParam,nint lParam);
    [DllImport("user32.dll")] internal static extern int GetMessage(out MSG msg,nint hwnd,uint min,uint max);
    [DllImport("user32.dll")] internal static extern bool TranslateMessage(ref MSG msg);
    [DllImport("user32.dll")] internal static extern nint DispatchMessage(ref MSG msg);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool PostThreadMessage(uint id,uint msg,nuint wParam,nint lParam);
    [DllImport("kernel32.dll")] internal static extern uint GetCurrentThreadId();
    [DllImport("user32.dll",SetLastError=true)] internal static extern uint SendInput(uint count,INPUT[] inputs,int size);
    [DllImport("user32.dll")] internal static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(nint hwnd);
    [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern bool GetKeyboardState(byte[] state);
    [DllImport("user32.dll")] internal static extern uint MapVirtualKey(uint code,uint mapType);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int ToUnicode(uint virtualKey,uint scanCode,byte[] keyboardState,System.Text.StringBuilder buffer,int bufferSize,uint flags);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int GetWindowText(nint hwnd,System.Text.StringBuilder text,int count);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(nint hwnd,out uint processId);

    [StructLayout(LayoutKind.Sequential)] internal struct POINT { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] internal struct MSG { public nint hwnd; public uint message; public nuint wParam; public nint lParam; public uint time; public POINT pt; }
    [StructLayout(LayoutKind.Sequential)] internal struct MSLLHOOKSTRUCT { public POINT pt; public uint mouseData,flags,time; public nuint extraInfo; }
    [StructLayout(LayoutKind.Sequential)] internal struct KBDLLHOOKSTRUCT { public uint vkCode,scanCode,flags,time; public nuint extraInfo; }
    [StructLayout(LayoutKind.Sequential)] internal struct INPUT { public uint type; public InputUnion U; }
    [StructLayout(LayoutKind.Explicit)] internal struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] internal struct MOUSEINPUT { public int dx,dy; public uint mouseData,dwFlags,time; public nuint extraInfo; }
    [StructLayout(LayoutKind.Sequential)] internal struct KEYBDINPUT { public ushort wVk,wScan; public uint dwFlags,time; public nuint extraInfo; }
}

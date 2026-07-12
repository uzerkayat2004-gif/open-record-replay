using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Rrp.Core;

namespace Rrp.Windows;

public sealed class GlobalInputHook : IAsyncDisposable
{
    private readonly Channel<RawInputEvent> _events=Channel.CreateBounded<RawInputEvent>(new BoundedChannelOptions(4096){FullMode=BoundedChannelFullMode.DropOldest,SingleReader=false,SingleWriter=true});
    private readonly NativeMethods.HookProc _mouseProc,_keyboardProc;
    private Thread? _thread; private nint _mouseHook,_keyboardHook; private uint _threadId; private long _sequence;
    public ChannelReader<RawInputEvent> Events=>_events.Reader;
    public GlobalInputHook(){_mouseProc=MouseCallback;_keyboardProc=KeyboardCallback;}

    public Task StartAsync(CancellationToken ct=default)
    {
        if(_thread is not null) throw new InvalidOperationException("Hook already started.");
        var ready=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _thread=new Thread(()=>Run(ready)){IsBackground=true,Name="RRP Input Hook"}; _thread.Start();
        return ready.Task.WaitAsync(ct);
    }
    private void Run(TaskCompletionSource ready)
    {
        _threadId=NativeMethods.GetCurrentThreadId();
        _mouseHook=NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL,_mouseProc,nint.Zero,0);
        _keyboardHook=NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL,_keyboardProc,nint.Zero,0);
        if(_mouseHook==nint.Zero||_keyboardHook==nint.Zero){ready.SetException(new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));return;}
        ready.SetResult(); while(NativeMethods.GetMessage(out var msg,nint.Zero,0,0)>0){NativeMethods.TranslateMessage(ref msg);NativeMethods.DispatchMessage(ref msg);}
        if(_mouseHook!=nint.Zero)NativeMethods.UnhookWindowsHookEx(_mouseHook);if(_keyboardHook!=nint.Zero)NativeMethods.UnhookWindowsHookEx(_keyboardHook);_events.Writer.TryComplete();
    }
    private nint MouseCallback(int code,nint wParam,nint lParam)
    {
        if(code>=0){var s=Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);var kind=(int)wParam switch{NativeMethods.WM_MOUSEMOVE=>InputEventKind.MouseMove,NativeMethods.WM_LBUTTONDOWN or NativeMethods.WM_RBUTTONDOWN=>InputEventKind.MouseDown,NativeMethods.WM_LBUTTONUP or NativeMethods.WM_RBUTTONUP=>InputEventKind.MouseUp,NativeMethods.WM_MOUSEWHEEL=>InputEventKind.MouseWheel,_=>(InputEventKind)(-1)};if((int)kind>=0)_events.Writer.TryWrite(new(Interlocked.Increment(ref _sequence),DateTimeOffset.Now,Stopwatch.GetTimestamp(),kind,(int)wParam,(int)s.mouseData,new(s.pt.X,s.pt.Y),(s.flags&NativeMethods.LLMHF_INJECTED)!=0||s.extraInfo==NativeMethods.RrpMarker));}
        return NativeMethods.CallNextHookEx(_mouseHook,code,wParam,lParam);
    }
    private nint KeyboardCallback(int code,nint wParam,nint lParam)
    {
        if(code>=0){var s=Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);var kind=(int)wParam is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN?InputEventKind.KeyDown:InputEventKind.KeyUp;_events.Writer.TryWrite(new(Interlocked.Increment(ref _sequence),DateTimeOffset.Now,Stopwatch.GetTimestamp(),kind,(int)s.vkCode,(int)s.scanCode,new(0,0),(s.flags&NativeMethods.LLKHF_INJECTED)!=0||s.extraInfo==NativeMethods.RrpMarker));}
        return NativeMethods.CallNextHookEx(_keyboardHook,code,wParam,lParam);
    }
    public ValueTask DisposeAsync(){if(_thread is not null){NativeMethods.PostThreadMessage(_threadId,NativeMethods.WM_QUIT,0,0);_thread.Join(TimeSpan.FromSeconds(3));_thread=null;}return ValueTask.CompletedTask;}
}

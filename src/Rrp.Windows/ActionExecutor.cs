using System.Runtime.InteropServices;
using FlaUI.Core.AutomationElements;
using Rrp.Core;

namespace Rrp.Windows;

public sealed class ActionExecutor : IDisposable
{
    private readonly SelectorResolver _resolver = new();

    public Task ExecuteAsync(WorkflowStep step, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (step.Target is null) throw new InvalidOperationException("Desktop step has no target selector.");
        var resolved = _resolver.Resolve(step.Target);
        if (resolved.Ambiguous) throw new InvalidOperationException("target_ambiguous");
        if (resolved.Element is null) throw new InvalidOperationException("target_not_found");
        var element = resolved.Element;

        if (step.Action.Contains("click", StringComparison.OrdinalIgnoreCase) || step.Action.Contains("invoke", StringComparison.OrdinalIgnoreCase))
        {
            if (element.Patterns.Invoke.IsSupported) element.Patterns.Invoke.Pattern.Invoke();
            else Click(element.BoundingRectangle);
            return Task.CompletedTask;
        }
        if (step.Action.Contains("key", StringComparison.OrdinalIgnoreCase))
        {
            var key = step.Input is not null && step.Input.TryGetValue("virtualKey", out var raw) ? Convert.ToUInt16(raw) : throw new InvalidOperationException("virtualKey missing.");
            element.Focus();
            var inputs = new[] { VirtualKey(key, 0), VirtualKey(key, NativeMethods.KEYEVENTF_KEYUP) };
            if (NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>()) != (uint)inputs.Length) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            return Task.CompletedTask;
        }

        if (step.Action.Contains("type", StringComparison.OrdinalIgnoreCase) || step.Action.Contains("set", StringComparison.OrdinalIgnoreCase))
        {
            var value = step.Input is not null && step.Input.TryGetValue("text", out var text) ? Convert.ToString(text) : null;
            if (value is null) throw new InvalidOperationException("Text input missing.");
            if (element.Patterns.Value.IsSupported) element.Patterns.Value.Pattern.SetValue(value);
            else { element.Focus(); TypeUnicode(value); }
            return Task.CompletedTask;
        }
        throw new NotSupportedException($"Unsupported action {step.Action}");
    }

    private static void Click(System.Drawing.Rectangle b)
    {
        var x = b.X + b.Width / 2; var y = b.Y + b.Height / 2;
        var vx = NativeMethods.GetSystemMetrics(76); var vy = NativeMethods.GetSystemMetrics(77);
        var vw = NativeMethods.GetSystemMetrics(78); var vh = NativeMethods.GetSystemMetrics(79);
        var dx = (int)Math.Round((x-vx)*65535d/Math.Max(1,vw-1)); var dy = (int)Math.Round((y-vy)*65535d/Math.Max(1,vh-1));
        var inputs = new[] { Mouse(dx,dy,NativeMethods.MOUSEEVENTF_MOVE|NativeMethods.MOUSEEVENTF_ABSOLUTE|NativeMethods.MOUSEEVENTF_VIRTUALDESK), Mouse(0,0,NativeMethods.MOUSEEVENTF_LEFTDOWN), Mouse(0,0,NativeMethods.MOUSEEVENTF_LEFTUP) };
        if (NativeMethods.SendInput((uint)inputs.Length,inputs,Marshal.SizeOf<NativeMethods.INPUT>()) != (uint)inputs.Length) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
    private static NativeMethods.INPUT Mouse(int x,int y,uint flags) => new() { type=NativeMethods.INPUT_MOUSE,U=new(){mi=new(){dx=x,dy=y,dwFlags=flags,extraInfo=NativeMethods.RrpMarker}} };
    private static void TypeUnicode(string value)
    {
        var list=new List<NativeMethods.INPUT>(); foreach(var ch in value){list.Add(Key(ch,NativeMethods.KEYEVENTF_UNICODE));list.Add(Key(ch,NativeMethods.KEYEVENTF_UNICODE|NativeMethods.KEYEVENTF_KEYUP));}
        var inputs=list.ToArray(); if(NativeMethods.SendInput((uint)inputs.Length,inputs,Marshal.SizeOf<NativeMethods.INPUT>())!=(uint)inputs.Length)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
    private static NativeMethods.INPUT Key(char c,uint flags)=>new(){type=NativeMethods.INPUT_KEYBOARD,U=new(){ki=new(){wScan=c,dwFlags=flags,extraInfo=NativeMethods.RrpMarker}}};
    private static NativeMethods.INPUT VirtualKey(ushort key,uint flags)=>new(){type=NativeMethods.INPUT_KEYBOARD,U=new(){ki=new(){wVk=key,dwFlags=flags,extraInfo=NativeMethods.RrpMarker}}};
    public void Dispose() => _resolver.Dispose();
}

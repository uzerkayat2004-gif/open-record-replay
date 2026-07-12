using System.Diagnostics;
using Rrp.Core;

namespace Rrp.Windows;

public sealed class RecordingService : IAsyncDisposable
{
    private readonly IRrpStore _store; private GlobalInputHook? _hook; private CancellationTokenSource? _cts; private Task? _consumer; private RecordingDocument? _current; private readonly List<RecordedAction> _actions=[]; private readonly UiaInspector _inspector=new(); private long _start;
    public RecordingService(IRrpStore store)=>_store=store;
    public RecordingDocument? Current=>_current is null?null:_current with{Actions=_actions.ToArray()};
    public async Task<RecordingDocument> StartAsync(string name,CancellationToken ct)
    { if(_hook is not null)throw new InvalidOperationException("Recording already active.");_actions.Clear();var id=Guid.NewGuid().ToString("N");_current=new("1.0",id,name,DateTimeOffset.Now,null,RecordingState.Recording,[]);_start=Stopwatch.GetTimestamp();_cts=CancellationTokenSource.CreateLinkedTokenSource(ct);_hook=new();await _hook.StartAsync(ct);_consumer=ConsumeAsync(_hook,_cts.Token);return _current; }
    private async Task ConsumeAsync(GlobalInputHook hook,CancellationToken ct)
    { await foreach(var e in hook.Events.ReadAllAsync(ct)){if(e.Injected)continue;if(e.Kind==InputEventKind.MouseUp){var element=_inspector.InspectAt(e.Point);if(element?.IsPassword==true)continue;_actions.Add(new($"step-{_actions.Count+1}","desktop.click",Elapsed(e.TimestampTicks),element,new Dictionary<string,object?>{{"button",e.Code}}));}else if(e.Kind==InputEventKind.KeyDown)
            {
                var focused=_inspector.InspectFocused(); if(focused?.IsPassword==true) continue;
                var text=Translate(e.Code);
                if(text is not null) _actions.Add(new($"step-{_actions.Count+1}","desktop.type",Elapsed(e.TimestampTicks),focused,new Dictionary<string,object?>{{"text",text}}));
                else _actions.Add(new($"step-{_actions.Count+1}","desktop.key",Elapsed(e.TimestampTicks),focused,new Dictionary<string,object?>{{"virtualKey",e.Code}}));
            }} }
    long Elapsed(long ticks)=>(long)((ticks-_start)*1000d/Stopwatch.Frequency);
    static string? Translate(int virtualKey)
    {
        var state=new byte[256]; if(!NativeMethods.GetKeyboardState(state)) return null;
        var buffer=new System.Text.StringBuilder(8); var scan=NativeMethods.MapVirtualKey((uint)virtualKey,0);
        var count=NativeMethods.ToUnicode((uint)virtualKey,scan,state,buffer,buffer.Capacity,0);
        return count>0?buffer.ToString(0,count):null;
    }
    public async Task<RecordingDocument> StopAsync(bool compileWorkflow,CancellationToken ct)
    { if(_hook is null||_current is null)throw new InvalidOperationException("No active recording.");_cts!.Cancel();await _hook.DisposeAsync();try{if(_consumer is not null)await _consumer;}catch(OperationCanceledException){}var done=_current with{CompletedAt=DateTimeOffset.Now,State=RecordingState.Completed,Actions=_actions.ToArray()};await _store.SaveRecordingAsync(done,ct);if(compileWorkflow)await _store.SaveWorkflowAsync(new WorkflowCompiler().Compile(done),ct);_hook=null;_cts.Dispose();_cts=null;_consumer=null;_current=null;return done; }
    public async ValueTask DisposeAsync(){if(_hook is not null){_cts?.Cancel();await _hook.DisposeAsync();}_cts?.Dispose();}
}

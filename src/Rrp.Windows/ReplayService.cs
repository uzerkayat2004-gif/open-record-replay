using System.Collections.Concurrent;
using Rrp.Core;

namespace Rrp.Windows;

public sealed class ReplayService
{
    private readonly IRrpStore _store;private readonly ConcurrentDictionary<string,(ReplayRun Run,CancellationTokenSource Cts)> _runs=new();private readonly ActionExecutor _executor=new();
    public ReplayService(IRrpStore store)=>_store=store;
    public async Task<ReplayRun> StartAsync(string workflowId,CancellationToken ct)
    { var w=await _store.GetWorkflowAsync(workflowId,ct)??throw new KeyNotFoundException("Workflow not found.");var id=Guid.NewGuid().ToString("N");var cts=CancellationTokenSource.CreateLinkedTokenSource(ct);var run=new ReplayRun(id,workflowId,ReplayState.Queued,0,w.Steps.Count,DateTimeOffset.Now,null,null);_runs[id]=(run,cts);_ = Task.Run(()=>ExecuteAsync(id,w,cts.Token),CancellationToken.None);return run; }
    async Task ExecuteAsync(string id,RrpWorkflow w,CancellationToken ct)
    { Update(id,r=>r with{State=ReplayState.Running});try{for(var i=0;i<w.Steps.Count;i++){ct.ThrowIfCancellationRequested();Update(id,r=>r with{CurrentStep=i});var step=w.Steps[i];await _executor.ExecuteAsync(step,ct);}Update(id,r=>r with{State=ReplayState.Succeeded,CurrentStep=w.Steps.Count,CompletedAt=DateTimeOffset.Now});}catch(OperationCanceledException){Update(id,r=>r with{State=ReplayState.Cancelled,CompletedAt=DateTimeOffset.Now});}catch(Exception ex){Update(id,r=>r with{State=ReplayState.Failed,CompletedAt=DateTimeOffset.Now,Error=ex.Message});}}
    public ReplayRun? Get(string id)=>_runs.TryGetValue(id,out var x)?x.Run:null;
    public bool Cancel(string id){if(!_runs.TryGetValue(id,out var x))return false;x.Cts.Cancel();return true;}
    void Update(string id,Func<ReplayRun,ReplayRun> f)=>_runs.AddOrUpdate(id,_=>throw new InvalidOperationException(),(_,x)=>(f(x.Run),x.Cts));
}

namespace Rrp.Core;

public sealed class WorkflowCompiler
{
    public RrpWorkflow Compile(RecordingDocument recording)
    {
        if (recording.State != RecordingState.Completed) throw new InvalidOperationException("Recording must be completed.");
        var steps = recording.Actions.Select((a,i) => new WorkflowStep(
            string.IsNullOrWhiteSpace(a.Id) ? $"step-{i+1}" : a.Id,
            a.Action,
            BuildTarget(a.Element),
            a.Input,
            null,
            [new("action.completed")],
            Risk(a.Action),
            15000)).ToArray();
        return new("rrp.dev/v1","Workflow",new(recording.RecordingId,recording.Name,recording.CreatedAt),new("rrp/1.0",["replay.desktop"]),new Dictionary<string,WorkflowParameter>(),steps);
    }

    private static SelectorTarget? BuildTarget(ElementSnapshot? e)
    {
        if(e is null) return null;
        var c=new List<SelectorCandidate>();
        if(!string.IsNullOrWhiteSpace(e.AutomationId)) c.Add(new("uia.automationId",e.AutomationId,Name:e.Name,Weight:45));
        if(!string.IsNullOrWhiteSpace(e.Name)) c.Add(new("uia.name",e.Name,Name:e.Name,Weight:20));
        c.Add(new("uia.controlType",e.ControlType,Weight:15));
        return new(c,"one");
    }
    private static ApprovalRequirement? Risk(string action) => action.Contains("submit",StringComparison.OrdinalIgnoreCase) ? new("external-communication") : null;
}

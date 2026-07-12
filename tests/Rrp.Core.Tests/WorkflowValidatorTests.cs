using Rrp.Core;

namespace Rrp.Core.Tests;

public sealed class WorkflowValidatorTests
{
    [Fact]
    public void Valid_read_workflow_passes()
    {
        var workflow = MakeWorkflow(new WorkflowStep("inspect", "desktop.inspect", null, null, null,
            [new("window.visible")], null));
        Assert.True(new WorkflowValidator().Validate(workflow).Valid);
    }

    [Fact]
    public void Mutating_step_without_postcondition_fails()
    {
        var workflow = MakeWorkflow(new WorkflowStep("click", "desktop.click", null, null, null, null));
        var result = new WorkflowValidator().Validate(workflow);
        Assert.False(result.Valid);
        Assert.Contains(result.Issues, x => x.Code == "missing_postcondition");
    }

    [Fact]
    public void Planner_rejects_missing_capability()
    {
        var workflow = MakeWorkflow(new WorkflowStep("inspect", "desktop.inspect", null, null, null,
            [new("window.visible")], null), ["replay.desktop"]);
        var plan = new ReplayPlanner(new WorkflowValidator()).Plan(workflow,
            new("rrp/1.0", "test", new HashSet<string>()));
        Assert.False(plan.Runnable);
        Assert.Contains(plan.Issues, x => x.Code == "capability_mismatch");
    }

    private static RrpWorkflow MakeWorkflow(WorkflowStep step, IReadOnlyList<string>? requirements = null) =>
        new("rrp.dev/v1", "Workflow", new("test", "Test", DateTimeOffset.UtcNow),
            new("rrp/1.0", requirements ?? []), new Dictionary<string, WorkflowParameter>(), [step]);
}

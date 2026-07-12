namespace Rrp.Core;

public sealed class ReplayPlanner(WorkflowValidator validator)
{
    public ReplayPlan Plan(RrpWorkflow workflow, RuntimeCapabilities capabilities)
    {
        var validation = validator.Validate(workflow);
        var issues = validation.Issues.ToList();
        if (!string.Equals(workflow.Requires.Protocol, capabilities.Protocol, StringComparison.Ordinal))
            issues.Add(new("capability_mismatch", $"Workflow requires {workflow.Requires.Protocol}; runtime provides {capabilities.Protocol}."));
        foreach (var required in workflow.Requires.All.Where(x => !capabilities.Features.Contains(x)))
            issues.Add(new("capability_mismatch", $"Missing required capability '{required}'."));

        var planned = workflow.Steps.Select(s => new PlannedStep(
            s.Id, s.Action,
            s.Approval is { Class: not "read" },
            s.TimeoutMs)).ToArray();
        return new(issues.Count == 0, issues, planned);
    }
}

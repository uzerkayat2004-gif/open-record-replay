namespace Rrp.Core;

public sealed class WorkflowValidator
{
    private static readonly HashSet<string> ApprovalClasses = new(StringComparer.OrdinalIgnoreCase)
    { "read", "write", "destructive", "credential", "external-communication", "financial" };

    public ValidationResult Validate(RrpWorkflow workflow)
    {
        var issues = new List<ValidationIssue>();
        if (workflow.ApiVersion != "rrp.dev/v1") issues.Add(new("unsupported_version", "apiVersion must be rrp.dev/v1."));
        if (workflow.Kind != "Workflow") issues.Add(new("invalid_kind", "kind must be Workflow."));
        if (string.IsNullOrWhiteSpace(workflow.Metadata.Id)) issues.Add(new("missing_id", "metadata.id is required."));
        if (string.IsNullOrWhiteSpace(workflow.Metadata.Name)) issues.Add(new("missing_name", "metadata.name is required."));
        if (workflow.Steps.Count == 0) issues.Add(new("missing_steps", "At least one step is required."));

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var step in workflow.Steps)
        {
            if (!ids.Add(step.Id)) issues.Add(new("duplicate_step_id", $"Duplicate step id '{step.Id}'.", step.Id));
            if (string.IsNullOrWhiteSpace(step.Action)) issues.Add(new("missing_action", "Step action is required.", step.Id));
            if (step.TimeoutMs is < 1 or > 300000) issues.Add(new("invalid_timeout", "timeoutMs must be between 1 and 300000.", step.Id));
            if (step.Target is { Cardinality: not "one" and not "many" and not "optional" })
                issues.Add(new("invalid_cardinality", "Target cardinality must be one, many, or optional.", step.Id));
            if (step.Target is { Candidates.Count: 0 }) issues.Add(new("missing_selector", "A target must include selector candidates.", step.Id));
            if (step.Approval is not null && !ApprovalClasses.Contains(step.Approval.Class))
                issues.Add(new("invalid_approval_class", $"Unknown approval class '{step.Approval.Class}'.", step.Id));
            if (IsMutating(step.Action) && step.Postconditions is null)
                issues.Add(new("missing_postcondition", "Mutating steps require a postcondition.", step.Id));
        }
        return new(issues.Count == 0, issues);
    }

    private static bool IsMutating(string action) =>
        action.Contains("click", StringComparison.OrdinalIgnoreCase) ||
        action.Contains("set", StringComparison.OrdinalIgnoreCase) ||
        action.Contains("type", StringComparison.OrdinalIgnoreCase) ||
        action.Contains("upload", StringComparison.OrdinalIgnoreCase) ||
        action.Contains("submit", StringComparison.OrdinalIgnoreCase);
}

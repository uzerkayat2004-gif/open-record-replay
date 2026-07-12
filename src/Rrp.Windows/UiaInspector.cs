using System.Diagnostics;
using System.Drawing;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Rrp.Core;

namespace Rrp.Windows;

public sealed class UiaInspector : IDisposable
{
    private readonly UIA3Automation _automation = new();

    public ElementSnapshot? InspectFocused()
    {
        try { var element = _automation.FocusedElement(); return element is null ? null : Snapshot(element); } catch { return null; }
    }

    public ElementSnapshot? InspectAt(PointDto point)
    {
        try
        {
            var element = _automation.FromPoint(new Point(point.X, point.Y));
            if (element is null) return null;
            return Snapshot(element);
        }
        catch { return null; }
    }

    internal static ElementSnapshot Snapshot(AutomationElement element)
    {
        var processId = element.Properties.ProcessId.ValueOrDefault;
        var process = "unknown";
        try { process = Process.GetProcessById(processId).ProcessName; } catch { }
        var ancestors = new List<ElementAncestor>();
        var parent = element.Parent;
        for (var i = 0; i < 4 && parent is not null; i++, parent = parent.Parent)
            ancestors.Add(new(parent.AutomationId, parent.Name, parent.ControlType.ToString(), parent.ClassName));
        var b = element.BoundingRectangle;
        return new(processId, process, ancestors.FirstOrDefault()?.Name ?? "", element.AutomationId, element.Name,
            element.ControlType.ToString(), element.ClassName, element.FrameworkType.ToString(),
            new(b.X, b.Y, b.Width, b.Height), element.IsEnabled, element.IsOffscreen,
            element.Properties.IsPassword.ValueOrDefault, ancestors);
    }

    public void Dispose() => _automation.Dispose();
}

public sealed class SelectorResolver : IDisposable
{
    private readonly UIA3Automation _automation = new();

    public (AutomationElement? Element, int Score, bool Ambiguous) Resolve(SelectorTarget target)
    {
        var desktop = _automation.GetDesktop();
        var scored = new List<(AutomationElement Element, int Score)>();
        AutomationElement[] elements;
        try { elements = desktop.FindAllDescendants(); } catch { return (null, 0, false); }
        foreach (var element in elements.Take(5000))
        {
            try
            {
                var score = 0;
                foreach (var candidate in target.Candidates)
                {
                    var match = candidate.Strategy switch
                    {
                        "uia.automationId" => string.Equals(element.AutomationId, candidate.Value, StringComparison.Ordinal),
                        "uia.name" => string.Equals(element.Name, candidate.Value, StringComparison.OrdinalIgnoreCase),
                        "uia.controlType" => string.Equals(element.ControlType.ToString(), candidate.Value, StringComparison.OrdinalIgnoreCase),
                        "uia.className" => string.Equals(element.ClassName, candidate.Value, StringComparison.Ordinal),
                        _ => false
                    };
                    if (match) score += Math.Max(1, candidate.Weight);
                }
                if (!element.IsEnabled) score -= 15;
                if (element.IsOffscreen) score -= 10;
                if (score > 0) scored.Add((element, score));
            }
            catch { }
        }
        var ordered = scored.OrderByDescending(x => x.Score).ToArray();
        if (ordered.Length == 0 || ordered[0].Score < 45) return (null, ordered.FirstOrDefault().Score, false);
        return (ordered[0].Element, ordered[0].Score, ordered.Length > 1 && ordered[0].Score - ordered[1].Score < 10);
    }

    public void Dispose() => _automation.Dispose();
}

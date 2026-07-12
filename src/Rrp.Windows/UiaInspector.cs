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
        try { var element = _automation.FocusedElement(); return element is null ? null : Snapshot(element); } catch (Exception ex) { Console.Error.WriteLine($"[UiaInspector] InspectFocused error: {ex}"); return null; }
    }

    public ElementSnapshot? InspectAt(PointDto point)
    {
        try
        {
            var element = _automation.FromPoint(new Point(point.X, point.Y));
            if (element is null) return null;
            return Snapshot(element);
        }
        catch (Exception ex) { Console.Error.WriteLine($"[UiaInspector] InspectAt error: {ex}"); return null; }
    }

    internal static ElementSnapshot Snapshot(AutomationElement element)
    {
        var processId = 0;
        try { processId = element.Properties.ProcessId.ValueOrDefault; } catch { }
        var process = "unknown";
        try { if (processId > 0) process = Process.GetProcessById(processId).ProcessName; } catch { }
        
        var ancestors = new List<ElementAncestor>();
        var parent = SafeParent(element);
        for (var i = 0; i < 4 && parent is not null; i++, parent = SafeParent(parent))
        {
            ancestors.Add(new(
                SafeString(parent.Properties.AutomationId), 
                SafeString(parent.Properties.Name), 
                SafeControlType(parent), 
                SafeString(parent.Properties.ClassName)
            ));
        }

        return new(
            processId, 
            process, 
            ancestors.FirstOrDefault()?.Name ?? "", 
            SafeString(element.Properties.AutomationId), 
            SafeString(element.Properties.Name),
            SafeControlType(element), 
            SafeString(element.Properties.ClassName), 
            SafeString(element.Properties.FrameworkId) ?? "Unknown",
            SafeBounds(element), 
            SafeBool(element.Properties.IsEnabled), 
            SafeBool(element.Properties.IsOffscreen),
            SafeBool(element.Properties.IsPassword), 
            ancestors
        );
    }

    private static string? SafeString(FlaUI.Core.AutomationProperty<string> prop)
    {
        try { return prop.ValueOrDefault; } catch { return null; }
    }

    private static string SafeControlType(AutomationElement el)
    {
        try { return el.Properties.ControlType.ValueOrDefault.ToString(); } catch { return "Unknown"; }
    }

    private static AutomationElement? SafeParent(AutomationElement el)
    {
        try { return el.Parent; } catch { return null; }
    }

    private static RectDto SafeBounds(AutomationElement el)
    {
        try 
        { 
            var b = el.Properties.BoundingRectangle.ValueOrDefault; 
            return new RectDto(b.X, b.Y, b.Width, b.Height);
        } 
        catch 
        { 
            return new RectDto(0, 0, 0, 0); 
        }
    }

    private static bool SafeBool(FlaUI.Core.AutomationProperty<bool> prop, bool defaultValue = false)
    {
        try { return prop.ValueOrDefault; } catch { return defaultValue; }
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

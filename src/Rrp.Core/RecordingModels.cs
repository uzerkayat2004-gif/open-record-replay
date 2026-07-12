namespace Rrp.Core;

public enum RecordingState { Idle, Recording, Completed, Cancelled, Failed }
public enum InputEventKind { MouseMove, MouseDown, MouseUp, MouseWheel, KeyDown, KeyUp }
public enum ReplayState { Queued, Running, Paused, Succeeded, Failed, Cancelled }

public sealed record PointDto(int X, int Y);
public sealed record RectDto(double X, double Y, double Width, double Height);
public sealed record RawInputEvent(long Sequence, DateTimeOffset OccurredAt, long TimestampTicks, InputEventKind Kind, int Code, int Data, PointDto Point, bool Injected);
public sealed record ElementSnapshot(int ProcessId, string ProcessName, string WindowTitle, string? AutomationId, string? Name, string ControlType, string? ClassName, string? FrameworkId, RectDto Bounds, bool IsEnabled, bool IsOffscreen, bool IsPassword, IReadOnlyList<ElementAncestor> Ancestors);
public sealed record ElementAncestor(string? AutomationId, string? Name, string ControlType, string? ClassName);
public sealed record RecordedAction(string Id, string Action, long OffsetMs, ElementSnapshot? Element, IReadOnlyDictionary<string, object?> Input, bool Sensitive = false);
public sealed record RecordingDocument(string SchemaVersion, string RecordingId, string Name, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, RecordingState State, IReadOnlyList<RecordedAction> Actions);
public sealed record RecordingSummary(string RecordingId, string Name, RecordingState State, DateTimeOffset CreatedAt, int ActionCount);
public sealed record ReplayRun(string ReplayId, string WorkflowId, ReplayState State, int CurrentStep, int TotalSteps, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, string? Error);

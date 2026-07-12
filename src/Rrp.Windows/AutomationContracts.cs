using Rrp.Core;

namespace Rrp.Windows;

public interface IDesktopObserver
{
    Task<ActiveWindowInfo?> GetActiveWindowAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SelectorCandidate>> FindCandidatesAsync(SelectorTarget target, CancellationToken cancellationToken);
}

public interface IDesktopActuator
{
    Task ExecuteAsync(WorkflowStep step, CancellationToken cancellationToken);
}

public interface IEmergencyStop
{
    bool IsStopRequested { get; }
    void RequestStop();
    void Reset();
}

public sealed class EmergencyStop : IEmergencyStop
{
    private int _requested;
    public bool IsStopRequested => Volatile.Read(ref _requested) == 1;
    public void RequestStop() => Interlocked.Exchange(ref _requested, 1);
    public void Reset() => Interlocked.Exchange(ref _requested, 0);
}

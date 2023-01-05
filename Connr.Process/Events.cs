namespace Connr.Process;

public sealed class Events
{
    private readonly Statistics _statistics;

    public Events(Statistics statistics)
    {
        _statistics = statistics;
    }
    
    public event Action<IProcessContainer>? ProcessStarting;

    public event Action<IProcessContainer>? ProcessRunning;

    public event Action<IProcessContainer>? ProcessStopping;

    public event Action<IProcessContainer>? ProcessKilling;

    public event Action<IProcessContainer>? ProcessStopped;

    public event Action<Line>? StandardOutputEmitted;

    public event Action<Line>? ErrorOutputEmitted;

    internal void RaiseProcessStarting(IProcessContainer processContainer)
    {
        ProcessStarting?.Invoke(processContainer);
    }

    internal void RaiseProcessRunning(IProcessContainer processContainer)
    {
        ProcessRunning?.Invoke(processContainer);
    }

    internal void RaiseProcessStopping(IProcessContainer processContainer)
    {
        ProcessStopping?.Invoke(processContainer);
    }

    internal void RaiseProcessKilling(IProcessContainer processContainer)
    {
        ProcessKilling?.Invoke(processContainer);
    }

    internal void RaiseProcessStopped(IProcessContainer processContainer)
    {
        ProcessStopped?.Invoke(processContainer);
    }

    internal void RaiseStandardOutputEmitted(string output)
    {
        var line = _statistics.AddToRecentLines(output, true);
        StandardOutputEmitted?.Invoke(line);
    }

    internal void RaiseErrorOutputEmitted(string error)
    {
        var line = _statistics.AddToRecentLines(error, false);
        ErrorOutputEmitted?.Invoke(line);
    }
}
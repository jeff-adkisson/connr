namespace Connr.Process;

public sealed class Events
{
    public event Action<IProcessContainer>? ProcessStarting;

    public event Action<IProcessContainer>? ProcessRunning;

    public event Action<IProcessContainer>? ProcessStopping;
    
    public event Action<IProcessContainer>? ProcessKilling;

    public event Action<IProcessContainer>? ProcessStopped;

    public event Action<string>? StandardOutputEmitted;

    public event Action<string>? ErrorOutputEmitted;

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
        StandardOutputEmitted?.Invoke(output);
    }

    internal void RaiseErrorOutputEmitted(string error)
    {
        ErrorOutputEmitted?.Invoke(error);
    }
}
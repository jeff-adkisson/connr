namespace Connr.Process;

public interface IProcessService
{
    int RunningProcessingCount { get; }
    void Start(IProcessContainer process);

    void StopAll(bool waitUntilAllStopped = true, int maxStopTimeSeconds = 15, bool killIfNotStopped = true);

    void KillAll(bool waitUntilAllKilled = true, int maxStopTimeSeconds = 15);

    IReadOnlyList<IProcessContainer> GetRunningProcesses();

    bool TryGetProcesses(Parameters parametersToMatch, out List<IProcessContainer> runningProcesses);

    event Action<IProcessContainer> ProcessStarted;

    event Action<IProcessContainer> ProcessEnded;

    event Action<int> RunningProcessCountChanged;
}
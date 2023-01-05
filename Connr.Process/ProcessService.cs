using Microsoft.Extensions.Logging;

namespace Connr.Process;

public class ProcessService : IProcessService
{
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(ILogger<ProcessService> logger)
    {
        _logger = logger;
        Processes = new Processes(_logger);
        Processes.ProcessAdded += ProcessAdded;
        Processes.ProcessRemoved += ProcessRemoved;
    }

    private Processes Processes { get; }

    public void Start(IProcessContainer process)
    {
        Processes.Start(process);
    }

    public void StopAll(bool waitUntilAllStopped = true, int maxStopTimeSeconds = 15, bool killIfNotStopped = true)
    {
        Processes.StopAll(waitUntilAllStopped, maxStopTimeSeconds, killIfNotStopped);
    }

    public void KillAll(bool waitUntilAllKilled = true, int maxStopTimeSeconds = 15)
    {
        Processes.KillAll(waitUntilAllKilled, maxStopTimeSeconds);
    }

    public IReadOnlyList<IProcessContainer> GetRunningProcesses()
    {
        return Processes.GetRunningProcesses();
    }

    /// <summary>
    /// Gets a list of processes that match an instance of <see cref="Parameters"/>.
    /// Returns false if no matches.
    /// </summary>
    /// <param name="parametersToMatch"></param>
    /// <param name="runningProcesses"></param>
    /// <returns></returns>
    public bool TryGetProcesses(Parameters parametersToMatch, out List<IProcessContainer> runningProcesses)
    {
        runningProcesses = new List<IProcessContainer>();
        var currentProcesses = GetRunningProcesses();
        foreach (var runningProcess in currentProcesses)
        {
            if (runningProcess.Parameters.Equals(parametersToMatch))
            {
                runningProcesses.Add(runningProcess);
            }
        }
        return runningProcesses.Any();
    }

    public int RunningProcessingCount => Processes.RunningProcessCount;

    public event Action<IProcessContainer>? ProcessStarted;

    public event Action<IProcessContainer>? ProcessEnded;

    public event Action<int>? RunningProcessCountChanged;

    private void ProcessRemoved(IProcessContainer process)
    {
        ProcessEnded?.Invoke(process);
        RunningProcessCountChanged?.Invoke(RunningProcessingCount);
    }

    private void ProcessAdded(IProcessContainer process)
    {
        ProcessStarted?.Invoke(process);
        RunningProcessCountChanged?.Invoke(RunningProcessingCount);
    }
}
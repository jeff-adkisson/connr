using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Connr.Process;

public class Processes
{
    private readonly ILogger _logger;

    public Processes(ILogger logger)
    {
        _logger = logger;
    }

    private ConcurrentDictionary<string, IProcessContainer> RunningProcesses { get; } = new();

    public int RunningProcessCount => RunningProcesses.Count;

    public void Start(IProcessContainer processContainer)
    {
        processContainer.Events.ProcessRunning += AddToRunningDictionary;
        processContainer.Events.ProcessStopped += RemoveFromRunningDictionary;
        processContainer.Start();
    }

    public void StopAll(bool waitUntilAllStopped = true, int maxStopTimeSeconds = 15, bool killIfNotStopped = true)
    {
        if (RunningProcesses.IsEmpty) return;
        CallStopAllProcesses(false);
        var stopTryingAfter = DateTime.Now.AddSeconds(maxStopTimeSeconds);
        if (waitUntilAllStopped)
            _logger.LogInformation($"Waiting up to {maxStopTimeSeconds} seconds for all processes to stop");
        while (waitUntilAllStopped && DateTime.Now < stopTryingAfter && !RunningProcesses.IsEmpty)
            Task.Delay(250).Wait();
        if (waitUntilAllStopped && killIfNotStopped && !RunningProcesses.IsEmpty)
            KillAll(waitUntilAllStopped, maxStopTimeSeconds);
    }

    public void KillAll(bool waitUntilAllKilled = true, int maxStopTimeSeconds = 15)
    {
        if (RunningProcesses.IsEmpty) return;
        CallStopAllProcesses(true);
        var stopTryingAfter = DateTime.Now.AddSeconds(maxStopTimeSeconds);
        if (waitUntilAllKilled)
            _logger.LogInformation($"Waiting up to {maxStopTimeSeconds} seconds for all processes to forceably stop");
        while (waitUntilAllKilled && DateTime.Now < stopTryingAfter && !RunningProcesses.IsEmpty)
            Task.Delay(250).Wait();
    }

    private void CallStopAllProcesses(bool kill)
    {
        var keys = RunningProcesses.Keys;
        foreach (var key in keys)
        {
            if (!RunningProcesses.TryGetValue(key, out var process)) continue;
            if (kill)
                process.Kill();
            else
                process.Stop();
        }
    }

    private void AddToRunningDictionary(IProcessContainer processContainer)
    {
        if (!RunningProcesses.TryAdd(processContainer.Id, processContainer)) return;

        ProcessAdded?.Invoke(processContainer);
        processContainer.Events.ProcessRunning -= AddToRunningDictionary;
    }

    private void RemoveFromRunningDictionary(IProcessContainer processContainer)
    {
        if (!RunningProcesses.TryRemove(processContainer.Id, out var removedProcessContainer)) return;

        ProcessRemoved?.Invoke(removedProcessContainer);
        removedProcessContainer.Events.ProcessStopped -= RemoveFromRunningDictionary;
    }

    public event Action<IProcessContainer>? ProcessAdded;

    public event Action<IProcessContainer>? ProcessRemoved;

    public IReadOnlyList<IProcessContainer> GetRunningProcesses()
    {
        return RunningProcesses
            .Select(p => p.Value)
            .ToList()
            .AsReadOnly();
    }
}
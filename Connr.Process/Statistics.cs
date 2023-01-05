using System.Collections.Concurrent;

namespace Connr.Process;

public class Statistics
{
    public int ProcessId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset StoppedAt { get; set; }

    public long TotalLines => OutputLines + ErrorLines;

    public long ErrorLines { get; private set; }

    public long OutputLines { get; private set; }

    public DateTimeOffset LastErrorAt { get; private set; }

    public DateTimeOffset LastOutputAt { get; private set; }

    private readonly ConcurrentQueue<Line> _mostRecentLines = new();

    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    internal Line AddToRecentLines(string output, bool isStandardOutput)
    {
        try
        {
            _semaphore.Wait();
            if (isStandardOutput)
            {
                LastOutputAt = DateTimeOffset.Now;
                OutputLines++;
            }
            else
            {
                LastErrorAt = DateTimeOffset.Now;
                ErrorLines++;
            }
            var line = new Line() { Number = TotalLines, Output = output, IsStandardOutput = isStandardOutput };
            
            _mostRecentLines.Enqueue(line);
            if (_mostRecentLines.Count > 25) _mostRecentLines.TryDequeue(out _);
            
            return line;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public IReadOnlyList<Line> GetMostRecentLines()
    {
        try
        {
            _semaphore.Wait();
            return _mostRecentLines.ToList().AsReadOnly();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
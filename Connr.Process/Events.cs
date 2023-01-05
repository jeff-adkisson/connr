using System.Text;
using System.Timers;

namespace Connr.Process;

public sealed class Events : IDisposable
{
    private readonly Statistics _statistics;

    private System.Timers.Timer? _timer;

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
        StartTimer();
        try
        {
            _semaphore.Wait();
            _stdOutputAcc.Add(output);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    internal void RaiseErrorOutputEmitted(string error)
    {
        StartTimer();
        try
        {
            _semaphore.Wait();
            _errorOutputAcc.Add(error);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void StartTimer()
    {
        if (_timer != null) return;
        _timer = new(250) { AutoReset = true };
        _timer.Elapsed += FlushOutput;
        _timer.Start();
    }

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private void FlushOutput(object? sender, ElapsedEventArgs e)
    {
        string[] stdOutput;
        string[] errorOutput;
        try
        {
            _semaphore.Wait();
            stdOutput = _stdOutputAcc.ToArray();
            _stdOutputAcc.Clear();
            errorOutput = _errorOutputAcc.ToArray();
            _errorOutputAcc.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
        
        foreach (var line in GetLines(stdOutput))
        {
            var newLine = _statistics.AddToRecentLines(line, true);
            StandardOutputEmitted?.Invoke(newLine);
        }
        
        foreach (var line in GetLines(errorOutput))
        {
            var newLine = _statistics.AddToRecentLines(line, false);
            ErrorOutputEmitted?.Invoke(newLine);
        }
    }

    private string[] GetLines(string[] lines)
    {
        var lineCount = 1;
        var recombined = new List<string>();
        
        void FlushStringBuilder(StringBuilder stringBuilder, string nextLine)
        {
            recombined.Add(stringBuilder.ToString());
            stringBuilder.Clear();
            stringBuilder.AppendLine(nextLine);
        }
        
        var sb = new StringBuilder();
        sb.AppendLine(lines[0]);
        foreach (var currentLine in lines[1..]) {
            if (!string.IsNullOrWhiteSpace(currentLine) && 
                (currentLine.StartsWith("  ") || currentLine.StartsWith("\t"))) 
            {
                sb.AppendLine(currentLine);
                if (lineCount > 5)
                {
                    FlushStringBuilder(sb, currentLine);
                    lineCount = 0;
                }
            } else
            {
                FlushStringBuilder(sb, currentLine);
                lineCount = 0;
            }
            lineCount++;
        }
        if (sb.Length > 0) recombined.Add(sb.ToString());

        return recombined.ToArray();
    }

    private readonly List<string> _stdOutputAcc = new();
    private readonly List<string> _errorOutputAcc = new();

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
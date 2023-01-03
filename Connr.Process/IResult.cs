namespace Connr.Process;

public interface IResult
{
    /// <summary>
    /// Exit code set by the underlying process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// When the command started executing.
    /// </summary>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// When the command finished executing.
    /// </summary>
    public DateTimeOffset ExitTime { get; }

    /// <summary>
    /// Total duration of the command execution.
    /// </summary>
    public TimeSpan RunTime { get; }
    
    /// <summary>
    /// Error output of the command, if any.
    /// </summary>
    public string Error { get; }
    
    /// <summary>
    /// If true, process exited returning a 0 exit code.
    /// </summary>
    public bool Success { get; }
}
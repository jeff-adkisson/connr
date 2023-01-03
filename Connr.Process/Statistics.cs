namespace Connr.Process;

public class Statistics
{
    public int ProcessId { get; set; }
    
    public DateTimeOffset StartedAt { get; set; }
    
    public DateTimeOffset StoppedAt { get; set; }

    public long ErrorLines { get; set; }

    public long OutputLines { get; set; }
    
    public DateTimeOffset LastErrorAt { get; set; }

    public DateTimeOffset LastOutputAt { get; set; } 
}
namespace Connr.Process;

public record Line
{
    public string Id { get; } = System.Guid.NewGuid().ToString();
    
    public long Number { get; init; }

    public string Output { get; init; } = "";

    public bool IsStandardOutput { get; init; } = true;
}
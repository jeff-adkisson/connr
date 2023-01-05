using CliWrap;

namespace Connr.Process;

public sealed class Parameters : IEquatable<Parameters>
{
    public string Name { get; init; } = "Name";
    
    public string Command { get; init; } = "";

    public List<string> Arguments { get; init; } = new();

    public string WorkingDirectory { get; init; } = "";

    public Dictionary<string, string?> EnvironmentVariables { get; init; } = new();

    public Credentials Credentials { get; init; } = Credentials.Default;

    public int StopAfterSeconds { get; init; } = 0;

    public bool Equals(Parameters? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Command == other.Command && 
               Arguments.SequenceEqual(other.Arguments) &&
               WorkingDirectory == other.WorkingDirectory && 
               EnvironmentVariables.SequenceEqual(other.EnvironmentVariables) && 
               StopAfterSeconds == other.StopAfterSeconds;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is Parameters other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Command, 
            Arguments, 
            WorkingDirectory, 
            EnvironmentVariables, 
            StopAfterSeconds);
    }
}
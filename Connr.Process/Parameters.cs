using CliWrap;

namespace Connr.Process;

public sealed class Parameters
{
    public string Command { get; set; } = "";

    public List<string> Arguments { get; set; } = new();

    public string WorkingDirectory { get; set; } = "";

    public Dictionary<string, string?> EnvironmentVariables { get; set; } = new();

    public Credentials Credentials { get; set; } = Credentials.Default;

    public int StopAfterSeconds { get; set; } = 0;
}
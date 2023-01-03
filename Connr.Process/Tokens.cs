namespace Connr.Process;

public class Tokens
{
    public Tokens(Parameters parameters)
    {
        StopTokenSource = new CancellationTokenSource();
        if (parameters.StopAfterSeconds > 0)
            StopTokenSource.CancelAfter(TimeSpan.FromSeconds(parameters.StopAfterSeconds));
    }

    public CancellationTokenSource StopTokenSource { get; } = new();

    public CancellationTokenSource KillTokenSource { get; } = new();
}
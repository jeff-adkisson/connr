namespace Connr.Process;

public class EmptyProcess : IProcessContainer, IDisposable
{
    private EmptyProcess()
    {
        Result = Process.Result.Default;
        Statistics = new Statistics();
        Events = new Events(Statistics);
    }

    public string Id => "Empty";

    public void Start()
    {
        throw new NotSupportedException();
    }

    public void Stop()
    {
        throw new NotSupportedException();
    }

    public void Kill()
    {
        throw new NotSupportedException();
    }

    public Parameters Parameters { get; } = new();

    public Events Events { get; }

    private readonly IResult _result = new Result(1, new Statistics());

    public IResult Result
    {
        get => _result;
        set
        {
            //ignore
        }
    }

    public ProcessState State { get; } = ProcessState.NotStarted;

    public Tokens Tokens { get; } = new(new Parameters());

    public Statistics Statistics { get; } 

    public static EmptyProcess Instance { get; } = new();
    
    public void Dispose()
    {
        Events.Dispose();
        Tokens.Dispose();
    }
}
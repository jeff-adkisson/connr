namespace Connr.Process;

public class EmptyProcess : IProcessContainer
{
    private static EmptyProcess? _instance;

    internal EmptyProcess()
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

    public IResult Result { get; set; }

    public ProcessState State { get; } = ProcessState.NotStarted;

    public Tokens Tokens { get; } = new(new Parameters());

    public Statistics Statistics { get; } 

    public static EmptyProcess Instance()
    {
        return _instance ??= new EmptyProcess();
    }
}
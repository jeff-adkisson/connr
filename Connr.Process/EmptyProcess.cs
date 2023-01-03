namespace Connr.Process;

public class EmptyProcess : IProcessContainer
{
    private static EmptyProcess? _instance;

    internal EmptyProcess()
    {
        Result = Connr.Process.Result.Default;
    }
    
    public static EmptyProcess Instance() => _instance ??= new EmptyProcess();
    
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

    public Parameters Parameters { get; } = new Parameters();
    
    public Events Events { get; } = new Events();

    public IResult Result { get; set; }
    
    public ProcessState State { get; } = ProcessState.NotStarted;
    
    public Tokens Tokens { get; } = new Tokens(new Parameters());

    public Statistics Statistics { get; } = new();
}
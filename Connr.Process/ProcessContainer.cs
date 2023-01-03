using Stateless.Graph;

namespace Connr.Process;

using Stateless;

public sealed class ProcessContainer : IProcessContainer
{
    public ProcessContainer(Parameters parameters)
    {
        Id = Guid.NewGuid().ToString();
        Parameters = parameters;
        Tokens = new Tokens(parameters);
        StateMachine = new StateMachine(this);
        Result = Connr.Process.Result.Default;
    }

    public string Id { get; }
    public Parameters Parameters { get; }
    
    public Events Events { get; } = new();

    public IResult Result { get; set; }
    
    private StateMachine StateMachine { get; }

    public ProcessState State => StateMachine.CurrentState;

    public void Start() => StateMachine.TriggerStateChange(ProcessTrigger.Start);
    
    public void Stop() => StateMachine.TriggerStateChange(ProcessTrigger.Stop);
    
    public void Kill() => StateMachine.TriggerStateChange(ProcessTrigger.Kill);

    public Tokens Tokens { get; } 

    public Statistics Statistics { get; } = new();
}
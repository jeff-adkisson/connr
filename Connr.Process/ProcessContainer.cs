using System.Collections.Concurrent;

namespace Connr.Process;

public sealed class ProcessContainer : IProcessContainer, IDisposable
{
    public ProcessContainer(Parameters parameters)
    {
        Id = Guid.NewGuid().ToString();
        Parameters = parameters;
        Tokens = new Tokens(parameters);
        StateMachine = new StateMachine(this);
        Result = Process.Result.Default;
        Statistics = new Statistics();
        Events = new Events(Statistics);
    }

    private StateMachine StateMachine { get; }

    public string Id { get; }
    public Parameters Parameters { get; }

    public Events Events { get; }

    public IResult Result { get; set; }

    public ProcessState State => StateMachine.CurrentState;

    public void Start()
    {
        StateMachine.TriggerStateChange(ProcessTrigger.Start);
    }

    public void Stop()
    {
        StateMachine.TriggerStateChange(ProcessTrigger.Stop);
    }

    public void Kill()
    {
        StateMachine.TriggerStateChange(ProcessTrigger.Kill);
    }

    public Tokens Tokens { get; }

    public Statistics Statistics { get; }
    
    public void Dispose()
    {
        Events.Dispose();
        Tokens.Dispose();
    }
}
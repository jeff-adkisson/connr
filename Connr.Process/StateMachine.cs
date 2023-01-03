using System.Reactive.Linq;
using CliWrap;
using CliWrap.EventStream;
using Stateless;
using Stateless.Graph;

namespace Connr.Process;

public class StateMachine
{
    private const int SuccessCode = Connr.Process.Result.SuccessCode;
    private const int ErrorCode = Connr.Process.Result.ErrorCode;
    
    private readonly ProcessContainer _container;
    private ProcessState _currentState = ProcessState.NotStarted;

    public StateMachine(ProcessContainer container)
    {
        _container = container;
        State = new StateMachine<ProcessState, ProcessTrigger>(
            () => _currentState,
            newState => _currentState = newState);

        ConfigureMachine();
    }

    private Tokens Tokens => _container.Tokens;

    private Parameters Parameters => _container.Parameters;

    private Statistics Statistics => _container.Statistics;

    public ProcessState CurrentState
    {
        get => _currentState;
        private set => _currentState = value;
    }

    private Events Events => _container.Events;

    private IResult Result
    {
        get => _container.Result;
        set => _container.Result = value;
    }

    private StateMachine<ProcessState, ProcessTrigger> State { get; }

    private void ConfigureMachine()
    {
        State.Configure(ProcessState.NotStarted)
            .Permit(ProcessTrigger.Start, ProcessState.Starting);

        State.Configure(ProcessState.Starting)
            .Permit(ProcessTrigger.Run, ProcessState.Running)
            .Permit(ProcessTrigger.EndWithError, ProcessState.EndedError)
            .OnEntry(Start);

        State.Configure(ProcessState.Running)
            .Permit(ProcessTrigger.EndWithSuccess, ProcessState.EndedSuccess)
            .Permit(ProcessTrigger.EndWithError, ProcessState.EndedError)
            .Permit(ProcessTrigger.Stop, ProcessState.Stopping)
            .Permit(ProcessTrigger.Kill, ProcessState.Killing);

        State.Configure(ProcessState.Stopping)
            .Permit(ProcessTrigger.Kill, ProcessState.Killing)
            .Permit(ProcessTrigger.EndWithSuccess, ProcessState.EndedSuccess)
            .Permit(ProcessTrigger.EndWithError, ProcessState.EndedError)
            .OnEntry(Stop);

        State.Configure(ProcessState.Killing)
            .Permit(ProcessTrigger.EndWithSuccess, ProcessState.EndedSuccess)
            .Permit(ProcessTrigger.EndWithError, ProcessState.EndedError)
            .OnEntry(Kill);

        State.Configure(ProcessState.EndedSuccess)
            .SubstateOf(ProcessState.Ended);

        State.Configure(ProcessState.EndedError)
            .SubstateOf(ProcessState.Ended)
            .OnEntry(OnEndedError);

        //var viz = UmlDotGraph.Format(State.GetInfo());
    }

    private void Stop()
    {
        Events.RaiseProcessStopping(_container);
        Tokens.StopTokenSource.Cancel();
    }

    private void Kill()
    {
        Events.RaiseProcessKilling(_container);
        Tokens.KillTokenSource.Cancel();
    }

    private void Start()
    {
        Task.Run(async () =>
        {
            try
            {
                Events.RaiseProcessStarting(_container);
                var cmd = Cli.Wrap(Parameters.Command!)
                    .WithArguments(Parameters.Arguments)
                    .WithEnvironmentVariables(Parameters.EnvironmentVariables)
                    .WithWorkingDirectory(Parameters.WorkingDirectory)
                    .WithCredentials(Parameters.Credentials);

                await cmd.Observe(
                        Console.OutputEncoding,
                        Console.OutputEncoding,
                        Tokens.KillTokenSource.Token,
                        Tokens.StopTokenSource.Token)
                    .ForEachAsync(HandleProcessEvent);
            }
            catch (TaskCanceledException taskEx)
            {
                Statistics.StoppedAt = DateTime.Now;
                var code = Tokens.StopTokenSource.IsCancellationRequested && !Tokens.KillTokenSource.IsCancellationRequested 
                    ? SuccessCode 
                    : ErrorCode;
                var errorMsg = code == SuccessCode ? "" : taskEx.Message;
                Result = new Result(code, Statistics) { Error = errorMsg} ;
                HandleProcessEvent(new ExitedCommandEvent(code)); //does not get called otherwise when cancellation requested
                await State.FireAsync(code == SuccessCode ? ProcessTrigger.EndWithSuccess : ProcessTrigger.EndWithError);
            }
            catch (Exception ex)
            {
                Statistics.StoppedAt = DateTime.Now;
                Result = new Result(ErrorCode, Statistics) { Error = ex.Message };
                await State.FireAsync(ProcessTrigger.EndWithError);
            }
        }).ConfigureAwait(false);
    }
    
    private async void HandleProcessEvent(CommandEvent evt)
    {
        switch (evt)
        {
            
            case StartedCommandEvent startedCommandEvent:
                OnStarted(startedCommandEvent);
                break;

            case ExitedCommandEvent exitedCommandEvent:
                await OnExited(exitedCommandEvent);
                break;

            case StandardErrorCommandEvent standardErrorCommandEvent:
                OnStandardError(standardErrorCommandEvent);
                break;

            case StandardOutputCommandEvent standardOutputCommandEvent:
                OnStandardOutput(standardOutputCommandEvent);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(evt));
        }
    }

    private void OnStandardError(StandardErrorCommandEvent evt)
    {
        Statistics.ErrorLines++;
        Statistics.LastErrorAt = DateTimeOffset.Now;
        Events.RaiseErrorOutputEmitted(evt.Text);
    }

    private void OnStandardOutput(StandardOutputCommandEvent evt)
    {
        Statistics.OutputLines++;
        Statistics.LastOutputAt = DateTimeOffset.Now;
        Events.RaiseStandardOutputEmitted(evt.Text);
    }

    private void OnStarted(StartedCommandEvent startedCommandEvent)
    {
        Statistics.StartedAt = DateTime.Now;
        Statistics.ProcessId = startedCommandEvent.ProcessId;
        State.Fire(ProcessTrigger.Run);
        Events.RaiseProcessRunning(_container);
    }

    private async Task OnExited(ExitedCommandEvent exitedCommandEvent)
    {
        Result = new Result(exitedCommandEvent.ExitCode, Statistics);
        await State.FireAsync(
            Result.Success
                ? ProcessTrigger.EndWithSuccess
                : ProcessTrigger.EndWithError);
        OnEnded();
    }

    private void OnEndedError()
    {
        OnStandardError(new StandardErrorCommandEvent(Result.Error));
        OnEnded();
    }

    private void OnEnded()
    {
        Statistics.StoppedAt = DateTime.Now;
        Events.RaiseProcessStopped(_container);
    }
    

    internal void TriggerStateChange(ProcessTrigger trigger) => State.Fire(trigger);
}
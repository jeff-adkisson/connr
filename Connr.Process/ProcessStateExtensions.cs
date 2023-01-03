namespace Connr.Process;

public static class ProcessStateExtensions
{
    public static bool CanStart(this ProcessState state)
    {
        return state is ProcessState.NotStarted || state.IsEnded();
    }

    public static bool CanStop(this ProcessState state)
    {
        return state is ProcessState.Starting or ProcessState.Running;
    }

    public static bool CanKill(this ProcessState state)
    {
        return state.CanStop() || IsStopping(state);
    }

    public static bool IsNotStarted(this ProcessState state)
    {
        return state is ProcessState.NotStarted;
    }

    public static bool IsStarting(this ProcessState state)
    {
        return state is ProcessState.Starting;
    }

    public static bool IsRunning(this ProcessState state)
    {
        return state is ProcessState.Running or ProcessState.Stopping or ProcessState.Killing;
    }

    public static bool IsStopping(this ProcessState state)
    {
        return state is ProcessState.Stopping or ProcessState.Killing;
    }

    public static bool IsKilling(this ProcessState state)
    {
        return state is ProcessState.Killing;
    }

    public static bool IsEnded(this ProcessState state)
    {
        return state is ProcessState.Ended or ProcessState.EndedSuccess or ProcessState.EndedError;
    }
}
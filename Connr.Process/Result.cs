using CliWrap;

namespace Connr.Process;

public class Result : CommandResult, IResult
{
    public Result(
        int exitCode,
        Statistics statistics) :
        base(
            exitCode,
            statistics.StartedAt,
            statistics.StoppedAt)
    {
    }

    public static IResult Default { get; } = new Result(-1, new Statistics());

    public string Error { get; set; } = "";

    public bool Success => ExitCode == Codes.Success;
}
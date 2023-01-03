using CliWrap;

namespace Connr.Process;

public class Result : CommandResult, IResult
{
    public const int SuccessCode = 0;
    public const int ErrorCode = 1;

    private static IResult? _defaultInstance;

    public Result(
        int exitCode,
        Statistics statistics) :
        base(
            exitCode,
            statistics.StartedAt,
            statistics.StoppedAt)
    {
    }

    public static IResult Default => _defaultInstance ??= new Result(-1, new Statistics());

    public string Error { get; set; } = "";

    public bool Success => ExitCode == SuccessCode;
}
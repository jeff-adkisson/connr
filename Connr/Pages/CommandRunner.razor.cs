using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Timers;
using CliWrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace Connr.Pages;

public partial class CommandRunner : IDisposable
{
    [Inject] private IJSRuntime? JsRuntime { get; set; }
    
    [Parameter] public string Command { get; set; } = string.Empty;
    
    [Parameter] public string Arguments { get; set; } = string.Empty;
    
    [Parameter] public string WorkingDirectory { get; set; } = string.Empty;

    private const int MaxOutputLength = 1024 * 25;

    private readonly StringBuilder _stdOutBuffer = new();

    private readonly Timer _timer = new(1000) { AutoReset = true };

    private string _output = "";
    private bool _isRunning = false;
    private bool _isStopping = false;

    private CommandModel Model { get; } = new();

    private CancellationTokenSource? _tokenSource;

    public void Dispose()
    {
        if (_isRunning) Stop(false);
        _timer?.Dispose();
        _tokenSource?.Cancel();
    }

    protected override void OnInitialized()
    {
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        WriteOutput();
    }

    private void WriteOutput(bool clear = false)
    {
        if (clear) _output = "";
        
        var newOutput = _stdOutBuffer.ToString();
        _stdOutBuffer.Clear();
        var currentLen = _output.Length + newOutput.Length;
        var outputToCut = currentLen >= MaxOutputLength ? currentLen - MaxOutputLength : 0;
        _output = outputToCut > _output.Length ? newOutput : _output[outputToCut..] + newOutput;
        InvokeAsync(StateHasChanged);
        var _ = JsRuntime!.InvokeVoidAsync("scrollToBottom", "output");
    }

    private async Task Start()
    {
        var start = DateTimeOffset.Now;
        PrepareOutputWindow();

        _timer.Start();
        CommandResult result;
        try
        {
            using var cts = new CancellationTokenSource();
            _tokenSource = cts;
            _isRunning = true;
            result = await Cli.Wrap(Model.Command!)
                .WithArguments(Model.Arguments)
                .WithWorkingDirectory(Model.WorkingDir)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(_stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(_stdOutBuffer))
                .ExecuteAsync(cts.Token);
        }
        catch (Exception e)
        {
            _stdOutBuffer.AppendLine($"{Environment.NewLine}{e.Message}");
            result = new CommandResult(1, start, DateTimeOffset.Now);
        }

        _isRunning = false;
        var finalResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        _stdOutBuffer.AppendLine($"{Environment.NewLine}{finalResult}");
        WriteOutput();
    }

    private void PrepareOutputWindow()
    {
        _stdOutBuffer.Clear();
        _stdOutBuffer.AppendLine("Starting...");
        _stdOutBuffer.AppendLine($"- {Model.Command} {Model.Arguments}");
        _stdOutBuffer.AppendLine($"- {Model.WorkingDir}{Environment.NewLine}");
        WriteOutput(true);
    }

    private void Stop() => Stop(true);
    
    private void Stop(bool showOutput)
    {
        try
        {
            _isStopping = true;
            if (showOutput)
            {
                _stdOutBuffer.AppendLine("Stopping...");
                WriteOutput();
            }

            _timer.Stop();
            _tokenSource!.Cancel();
        }
        catch (Exception ex)
        {
            if (showOutput)
            {
                _stdOutBuffer.AppendLine($"Stop error: {ex.Message}");
                WriteOutput();
            }
        }
        finally
        {
            _isStopping = false;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Model.Command = Command;
        Model.Arguments = Arguments;
        Model.WorkingDir = WorkingDirectory;
    }

    public class CommandModel
    {
        [Required] 
        public string? Command { get; set; } = "dotnet";
        
        [Required]
        public string Arguments { get; set; } = "run";
        
        [Required]
        public string WorkingDir { get; set; } = @"D:\projects\compass\HighMatch.Compass.AppServer.SiloHost";
    }
}
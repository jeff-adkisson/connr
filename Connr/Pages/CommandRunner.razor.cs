using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Timers;
using CliWrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace Connr.Pages;

public partial class Index : IDisposable
{
    [Inject] private IJSRuntime? JsRuntime { get; set; }

    private const int MaxOutputLength = 1024 * 25;

    private readonly StringBuilder _stdOutBuffer = new();

    private readonly Timer _timer = new(1000) { AutoReset = true };

    private string _output = "";
    private bool _isRunning = false;

    private CommandModel Model { get; } = new();

    private CancellationTokenSource? _tokenSource;

    public void Dispose()
    {
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
            result = new CommandResult(1, DateTimeOffset.Now, DateTimeOffset.Now);
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

    private void Stop()
    {
        _stdOutBuffer.AppendLine("Stopping...");
        WriteOutput();
        
        _timer.Stop();
        _tokenSource!.Cancel();
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
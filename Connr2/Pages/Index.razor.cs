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
    [Inject] private IJSRuntime JsRuntime { get; set; }
    
    private const string WorkingDir = @"D:\projects\compass\HighMatch.Compass.AppServer.SiloHost";
    private const string Command = "dotnet";
    private const string Arguments = "run";

    private readonly StringBuilder _stdOutBuffer = new();

    private readonly Timer _timer = new(1000) { AutoReset = true };
    private readonly CancellationTokenSource _cts = new();

    private string _output = "";

    public void Dispose()
    {
        _timer?.Dispose();
        _cts.Cancel();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
        await Start();
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        WriteOutput();
    }

    private void WriteOutput()
    {
        var newOutput = _stdOutBuffer.ToString();
        _stdOutBuffer.Clear();
        var currentLen = _output.Length + newOutput.Length;
        var outputToCut = currentLen >= 25600 ? currentLen - 25600 : 0;
        
        _output = _output[outputToCut..] + newOutput;
        InvokeAsync(StateHasChanged);
    }

    private async Task Start()
    {
        var result = await Cli.Wrap(Command)
            .WithArguments(Arguments)
            .WithWorkingDirectory(WorkingDir)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(_stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(_stdOutBuffer))
            .ExecuteAsync(_cts.Token);

        var finalResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        _stdOutBuffer.AppendLine(finalResult);
        WriteOutput();
    }

    private void Stop()
    {
        _timer.Stop();
        _cts.Cancel();
    }
}
using System.ComponentModel.DataAnnotations;
using Connr.Process;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Connr.App.Components;

public partial class CommandRunner : IDisposable
{
    private const int MaxOutputLength = 1024 * 50;

    private string _output = "";

    [Inject] private IJSRuntime? JsRuntime { get; set; }

    [Inject] private IProcessService ProcessService { get; set; } = null!;

    [Parameter] public string Command { get; set; } = string.Empty;

    [Parameter] public string Arguments { get; set; } = string.Empty;

    [Parameter] public string WorkingDirectory { get; set; } = string.Empty;

    private CommandModel Model { get; } = new();

    private IProcessContainer CurrentProcess { get; set; } = EmptyProcess.Instance();

    public void Dispose()
    {
        CurrentProcess.Events.ProcessStarting -= OnProcessStarting;
        CurrentProcess.Events.ProcessRunning -= OnProcessRunning;
        CurrentProcess.Events.ProcessStopping -= OnProcessStopping;
        CurrentProcess.Events.ProcessKilling -= OnProcessKilling;
        CurrentProcess.Events.ProcessStopped -= OnProcessStopped;
        CurrentProcess.Events.StandardOutputEmitted -= WriteStandardOutput;
        CurrentProcess.Events.ErrorOutputEmitted -= WriteErrorOutput;
    }

    private void WriteOutput(string newOutput, bool clear = false)
    {
        if (clear) _output = "";

        var currentLen = _output.Length + newOutput.Length;
        var outputToCut = currentLen >= MaxOutputLength ? currentLen - MaxOutputLength : 0;
        _output = outputToCut > _output.Length
            ? $"{newOutput}{Environment.NewLine}"
            : $"{_output[outputToCut..]}{newOutput}{Environment.NewLine}";
        InvokeAsync(StateHasChanged);
        var _ = JsRuntime!.InvokeVoidAsync("scrollToBottom", "output");
    }

    private void Start()
    {
        PrepareOutputWindow();

        var args = new List<string>();
        if (!string.IsNullOrWhiteSpace(Model.Arguments)) args.Add(Model.Arguments);
        var parms = new Parameters
        {
            Command = Model.Command!,
            Arguments = args,
            WorkingDirectory = Model.WorkingDirectory
        };
        CurrentProcess = new ProcessContainer(parms);
        CurrentProcess.Events.ProcessStarting += OnProcessStarting;
        CurrentProcess.Events.ProcessRunning += OnProcessRunning;
        CurrentProcess.Events.ProcessStopping += OnProcessStopping;
        CurrentProcess.Events.ProcessKilling += OnProcessKilling;
        CurrentProcess.Events.ProcessStopped += OnProcessStopped;
        CurrentProcess.Events.StandardOutputEmitted += WriteStandardOutput;
        CurrentProcess.Events.ErrorOutputEmitted += WriteErrorOutput;
        ProcessService.Start(CurrentProcess);
    }

    private void OnProcessKilling(IProcessContainer _)
    {
        WriteOutput("Process killing (forced stop)...");
    }

    private void OnProcessStopping(IProcessContainer _)
    {
        WriteOutput("Process stopping...");
    }

    private void OnProcessStarting(IProcessContainer _)
    {
        WriteOutput("Process starting...");
    }

    private void OnProcessStopped(IProcessContainer process)
    {
        WriteOutput($"Process ID {process.Statistics.ProcessId} stopped");
        WriteOutput($"-- Final State: {process.State}");
        WriteOutput($"-- Run Time: {process.Statistics.StoppedAt - process.Statistics.StartedAt}");
    }

    private void OnProcessRunning(IProcessContainer process)
    {
        WriteOutput($"Process ID {process.Statistics.ProcessId} started");
    }

    private void WriteErrorOutput(string errOutput)
    {
        WriteOutput($"ERROR: {errOutput}");
    }

    private void WriteStandardOutput(string stdOutput)
    {
        WriteOutput(stdOutput);
    }

    private void PrepareOutputWindow()
    {
        WriteOutput("Starting...", true);
        WriteOutput($"- {Model.Command} {Model.Arguments}");
        WriteOutput($"- {Model.WorkingDirectory}{Environment.NewLine}");
    }

    private void Stop()
    {
        CurrentProcess!.Stop();
    }

    private void Kill()
    {
        CurrentProcess!.Kill();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Model.Command = Command;
        Model.Arguments = Arguments;
        Model.WorkingDirectory = WorkingDirectory;
    }

    public class CommandModel
    {
        [Required] public string? Command { get; set; } = "";

        public string Arguments { get; set; } = "";

        public string WorkingDirectory { get; set; } = @"";
    }
}
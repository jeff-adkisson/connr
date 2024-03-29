﻿using System.ComponentModel.DataAnnotations;
using Connr.App.Shared;
using Connr.Process;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Connr.App.Components;

public partial class CommandRunner : IDisposable
{
    private const int MaxOutputLength = 1024 * 50;

    private OutputWindow OutputWindowRef { get; set; } = null!;
    
    private RelatedPorts RelatedPortsRef { get; set; } = null!;
    
    private ProcessTree ProcessTreeRef { get; set; } = null!;

    [Inject] private IProcessService ProcessService { get; set; } = null!;
    
    [Parameter] public string Name { get; set; } = string.Empty;

    [Parameter] public string Command { get; set; } = string.Empty;

    [Parameter] public string Arguments { get; set; } = string.Empty;

    [Parameter] public string WorkingDirectory { get; set; } = string.Empty;

    [Parameter] public IProcessContainer? ProcessContainer { get; set; }

    private CommandModel Model { get; } = new();

    private IProcessContainer CurrentProcess { get; set; } = EmptyProcess.Instance;

    public void Dispose()
    {
        CurrentProcess.Events.ProcessStarting -= OnProcessStarting;
        CurrentProcess.Events.ProcessRunning -= OnProcessRunning;
        CurrentProcess.Events.ProcessStopping -= OnProcessStopping;
        CurrentProcess.Events.ProcessKilling -= OnProcessKilling;
        CurrentProcess.Events.ProcessStopped -= OnProcessStopped;
        CurrentProcess.Events.StandardOutputEmitted -= WriteLine;
        CurrentProcess.Events.ErrorOutputEmitted -= WriteLine;
    }

    private void WriteOutput(string output, bool clear = false)
    {
        var line = new Line { IsStandardOutput = true, Number = 0, Output = output };
        WriteOutput(line, clear, true);
    }

    private void WriteOutput(Line line, bool clear = false, bool doNotNumber = false)
    {
        if (clear) OutputWindowRef.Clear();
        
        OutputWindowRef.AppendOutput(line, doNotNumber);
        InvokeAsync(StateHasChanged);
    }

    private bool DisplayOutputWindow => CurrentProcess.State.IsRunning() || CurrentProcess.State.IsStopped();

    private void Start()
    {
        PrepareOutputWindow();

        var args = new List<string>();
        if (!string.IsNullOrWhiteSpace(Model.Arguments))
        {
            var split = Model.Arguments.Split(' ')
                .Where(arg => !string.IsNullOrWhiteSpace(arg));
            args.AddRange(split);
        }
        var parms = new Parameters
        {
            Command = Model.Command!,
            Arguments = args,
            WorkingDirectory = Model.WorkingDirectory,
            Name = Model.Name
        };
        CurrentProcess = new ProcessContainer(parms);
        ConnectProcessEvents();
        ProcessService.Start(CurrentProcess);
    }

    private void ConnectProcessEvents()
    {
        CurrentProcess.Events.ProcessStarting += OnProcessStarting;
        CurrentProcess.Events.ProcessRunning += OnProcessRunning;
        CurrentProcess.Events.ProcessStopping += OnProcessStopping;
        CurrentProcess.Events.ProcessKilling += OnProcessKilling;
        CurrentProcess.Events.ProcessStopped += OnProcessStopped;
        CurrentProcess.Events.StandardOutputEmitted += WriteLine;
        CurrentProcess.Events.ErrorOutputEmitted += WriteLine;
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
        var output =
            $"""
            Process ID {process.Statistics.ProcessId} stopped
               Final State: {process.State}
               Run Time: {process.Statistics.StoppedAt - process.Statistics.StartedAt}
            """;

        WriteOutput(output);
    }

    private void OnProcessRunning(IProcessContainer process)
    {
        WriteOutput($"Process ID {process.Statistics.ProcessId} started");
    }

    private void WriteLine(Line line)
    {
        WriteOutput(line);
    }

    private void PrepareOutputWindow()
    {
        var toWrite = 
            $"""
            Starting process...
               {Model.Command} {Model.Arguments}
               {Model.WorkingDirectory} 
            """;                
        WriteOutput(toWrite, true);
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
        Model.Name = Name;
        Model.Command = Command;
        Model.Arguments = Arguments;
        Model.WorkingDirectory = WorkingDirectory;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (!firstRender) return;
        
        //hook up already running process to UI
        if (ProcessContainer == null) return;
        StartRenderingOfRunningProcess();
    }

    private void StartRenderingOfRunningProcess()
    {
        CurrentProcess = ProcessContainer!;
        
        ConnectProcessEvents();
        
        var mostRecentLines = CurrentProcess.Statistics.GetMostRecentLines();
        foreach (var line in mostRecentLines) WriteLine(line);

        StateHasChanged();
    }

    public class CommandModel
    {
        public string Name { get; set; } = "";
        
        [Required] public string? Command { get; set; } = "";

        public string Arguments { get; set; } = "";

        public string WorkingDirectory { get; set; } = @"";
    }

    private void Clear()
    {
        CurrentProcess = EmptyProcess.Instance;
    }

    private void ShowProcessTree()
    {
        ProcessTreeRef.IsLoading = true;
        StateHasChanged();
        Task.Run(() =>
        {
            ProcessTreeRef.ShowProcessTree();
            InvokeAsync(StateHasChanged);
        }).ConfigureAwait(false);
    }
    
    private void ShowRelatedPorts()
    {
        RelatedPortsRef.IsLoading = true;
        StateHasChanged();
        Task.Run(() =>
        {
            RelatedPortsRef.ShowRelatedPorts();
            InvokeAsync(StateHasChanged);
        }).ConfigureAwait(false);
    }

    private void OnRelatedComponentClosed()
    {
        StateHasChanged();
    }
}
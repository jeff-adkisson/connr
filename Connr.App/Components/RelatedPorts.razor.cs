using System.Timers;
using Connr.Process;
using DotNetstat;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Processes = DotNetstat.Processes;
using Timer = System.Timers.Timer;

namespace Connr.App.Components;

public partial class RelatedPorts : IDisposable
{
    private Timer _refreshTimer = new() { Interval = 1000, AutoReset = false };

    private List<NetstatLine>? _relatedPorts;

    [Parameter] public int ProcessId { get; set; }

    [Parameter] public ProcessState ProcessState { get; set; }

    [Parameter] public string Class { get; set; } = "";
    
    [Parameter] public EventCallback OnClosed { get; set; }


    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    void IDisposable.Dispose()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (ProcessState.IsStarting()) _relatedPorts = null;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _refreshTimer.Elapsed += OnRefreshTimerElapsed;
    }

    private void OnRefreshTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    internal bool IsVisible => _relatedPorts != null;

    internal void ShowRelatedPorts()
    {
        if (ProcessId == 0)
        {
            _relatedPorts = null;
            return;
        }

        IsLoading = true;
        var netstat = Netstat.Call().Lines;

        _relatedPorts = netstat
            .ByProcessId(ProcessId)
            .Where(n => n.LocalPort > 0)
            .OrderBy(n => n.State == "LISTENING" ? 0 : 1)
            .ThenBy(n => n.LocalAddress)
            .ToList();
        IsLoading = false;
        InvokeAsync(StateHasChanged);
    }

    private void KillProcess(int relatedPortProcessId)
    {
        var process = Processes.Running().ByProcessId(relatedPortProcessId);
        if (process == null) return;
        try
        {
            process.Kill();
            Snackbar.Add(
                $"Killing process {relatedPortProcessId}.",
                Severity.Warning);
            _refreshTimer.Start();
        }
        catch (Exception e)
        {
            Snackbar.Add(
                $"Failed to kill process {relatedPortProcessId}. System returned {e.Message}",
                Severity.Error);
            throw;
        }
    }

    private void Hide()
    {
        _relatedPorts = null;
        IsLoading = false;
        OnClosed.InvokeAsync();
    }

    internal bool IsLoading { get; set; }
}
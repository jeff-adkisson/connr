using Connr.Process;
using Microsoft.AspNetCore.Components;

namespace Connr.App.Feature;

public partial class Run : IDisposable
{
    [Inject] private IProcessService ProcessService { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ProcessService.RunningProcessCountChanged += ProcessCountChanged;
    }

    private void ProcessCountChanged(int processCount)
    {
        InvokeAsync(StateHasChanged);
    }

    private void StopAll()
    {
        Task.Run(() => ProcessService.StopAll(false)).ConfigureAwait(false);
    }

    private void KillAll()
    {
        Task.Run(() => ProcessService.KillAll(false)).ConfigureAwait(false);
    }

    void IDisposable.Dispose()
    {
        ProcessService.RunningProcessCountChanged -= ProcessCountChanged;
    }

    private void StopServer()
    {
        Environment.Exit(0);
    }
}
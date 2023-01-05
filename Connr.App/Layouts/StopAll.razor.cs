using Connr.Process;
using Microsoft.AspNetCore.Components;

namespace Connr.App.Layouts;

public partial class StopAll : IDisposable
{
    [Inject] private IProcessService ProcessService { get; set; } = null!;

    private bool _isStoppingAll;
    private bool _isKillingAll;

    private void StopAllProcesses()
    {
        _isStoppingAll = true;
        Task.Run(() => ProcessService.StopAll(false)).ConfigureAwait(false);
    }

    private void KillAllProcesses()
    {
        _isKillingAll = true;
        Task.Run(() => ProcessService.KillAll(false)).ConfigureAwait(false);
    }

    private void ProcessCountChanged(int processCount)
    {
        if (processCount == 0)
        {
            _isStoppingAll = false;
            _isKillingAll = false;
        }
        InvokeAsync(StateHasChanged);

    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ProcessService.RunningProcessCountChanged += ProcessCountChanged;
    }
    
    void IDisposable.Dispose()
    {
        ProcessService.RunningProcessCountChanged -= ProcessCountChanged;
    }
}
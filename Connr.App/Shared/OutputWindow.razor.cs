using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Timers;
using Connr.Process;
using Timer = System.Timers.Timer;

namespace Connr.App.Shared;

public partial class OutputWindow : IDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Parameter] public string Class { get; set; } = "";

    private long _dictIndex = 0;
    private readonly ConcurrentDictionary<long, Line> _lines = new();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private Timer _timer = new(250) { AutoReset = true };
    private bool _awaitingRedraw;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _timer.Elapsed += OnDrawTable;
        _timer.Start();
    }

    private void OnDrawTable(object? sender, ElapsedEventArgs e)
    {
        if (!_awaitingRedraw) return;
        try
        {
            _semaphore.Wait();
            InvokeAsync(StateHasChanged).Wait();
            JsRuntime!.InvokeVoidAsync("scrollToBottom", Id);
            _awaitingRedraw = false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void AppendOutput(Line line, bool doNotNumber = false)
    {
        if (string.IsNullOrWhiteSpace(line.Output)) return;
        try
        {
            _semaphore.Wait();
            var idx = _dictIndex++;
            _lines.TryAdd(idx, line);
            if (_lines.Count > 1000)
            {
                var keys = _lines.Keys.ToArray();
                for (var keyIdx = 0; keyIdx < 50; keyIdx++)
                {
                    _lines.Remove(keys[keyIdx], out _);
                }
            }
            _awaitingRedraw = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Clear()
    {
        try
        {
            _semaphore.Wait();
            _lines.Clear();
            _awaitingRedraw = true;
        } 
        finally {
            _semaphore.Release();
        }
        
    }

    void IDisposable.Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
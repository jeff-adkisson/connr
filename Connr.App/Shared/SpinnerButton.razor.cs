using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Connr.App.Shared;

public partial class SpinnerButton
{
    [Parameter] public string Label { get; set; } = "Not Set";

    [Parameter] public bool IsDisabled { get; set; } = true;

    [Parameter] public Variant ButtonVariant { get; set; } = Variant.Filled;
    
    [Parameter] public Color ButtonColor { get; set; } = Color.Default;
    
    [Parameter] public EventCallback OnClick { get; set; }
    
    [Parameter] public Color SpinnerColor { get; set; } = Color.Default;
}
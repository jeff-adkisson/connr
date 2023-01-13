using Connr.Process;
using DotNetstat;
using DotNetstat.ProcessTree;
using Microsoft.AspNetCore.Components;
using Processes = DotNetstat.Processes;

namespace Connr.App.Components;

public partial class ProcessTree
{
    [Parameter] public int ProcessId { get; set; }

    [Parameter] public ProcessState ProcessState { get; set; }

    [Parameter] public string Class { get; set; } = "";

    [Parameter] public EventCallback OnClosed { get; set; }

    internal bool IsVisible => !IsLoading && TreeItems.Any();

    internal bool IsLoading { get; set; }

    private HashSet<TreeItemData> TreeItems { get; set; } = new();

    private void Hide()
    {
        IsLoading = false;
        TreeItems.Clear();
        OnClosed.InvokeAsync();
    }

    internal void ShowProcessTree()
    {
        if (ProcessId == 0) return;

        var process = Processes.GetProcessById(ProcessId);
        if (process == null) return;

        IsLoading = true;
        var tree = process.GetTree();
        TreeItems.Clear();
        AddChildren(tree, TreeItems, true);
        IsLoading = false;
        InvokeAsync(StateHasChanged);
    }

    private void AddChildren(DotNetstat.ProcessTree.Tree tree, HashSet<TreeItemData> treeItemData, bool expand)
    {
        var thisTreeItem = new TreeItemData { Title = $"{tree.Id}: {tree.ProcessName}", IsExpanded = expand };
        treeItemData.Add(thisTreeItem);
        foreach (var child in tree.ChildProcesses.OrderBy(p => p.Id)) AddChildren(child, thisTreeItem.TreeItems, false);
    }

    private sealed class TreeItemData
    {
        public string Title { get; init; } = "";

        public bool IsExpanded { get; set; }

        public HashSet<TreeItemData> TreeItems { get; } = new();
    }
}
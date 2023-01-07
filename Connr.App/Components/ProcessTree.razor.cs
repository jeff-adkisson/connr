﻿using Connr.Process;
using DotNetstat;
using Microsoft.AspNetCore.Components;
using Processes = DotNetstat.Processes;

namespace Connr.App.Components;

public partial class ProcessTree
{
    [Parameter] public int ProcessId { get; set; }

    [Parameter] public ProcessState ProcessState { get; set; }
    
    [Parameter] public string Class { get; set; } = "";

    internal bool IsVisible => !IsLoading && TreeItems.Any();
    
    internal bool IsLoading { get; set; }
    
    private void Hide()
    {
        IsLoading = false;
        TreeItems.Clear();
    }

    internal void ShowProcessTree()
    {
        if (ProcessId == 0) return;

        var process = Processes.ByProcessId(ProcessId);
        if (process == null) return;

        IsLoading = true;
        var tree = process.GetProcessTree();
        TreeItems.Clear();
        AddChildren(tree, TreeItems);
        IsLoading = false;
        InvokeAsync(StateHasChanged);
    }

    private void AddChildren(DotNetstat.ProcessTree tree,  HashSet<TreeItemData> treeItemData)
    {
        var thisTreeItem = new TreeItemData() { Title = $"{tree.Id}, {tree.ProcessName}" };
        treeItemData.Add(thisTreeItem);
        foreach (var child in tree.ChildProcesses)
        {
            AddChildren(child, thisTreeItem.TreeItems);
        }
    }
    
    internal class TreeItemData
    {
        public string Title { get; init; } = "";
        
        public bool IsExpanded { get; set; }

        public HashSet<TreeItemData> TreeItems { get; set; } = new();
    }

    private HashSet<TreeItemData> TreeItems { get; set;  } = new();
}
using Connr.Process;
using Microsoft.AspNetCore.Components;
using OperatingSystem = Connr.Process.OperatingSystem;

namespace Connr.App.Feature;

public partial class Run
{
    [Inject] private IProcessService ProcessService { get; set; } = null!;
    private List<(Parameters Parameters, IProcessContainer? ProcessContainer)>? RunList { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        RunList = GetRunList();
    }

    private List<(Parameters Parameters, IProcessContainer? ProcessContainer)> GetRunList()
    {
        var result = new List<(Parameters, IProcessContainer?)>();
        var defaultParms = GetDefaultParameterList();
        foreach (var defaultParm in defaultParms)
        {
            if (!ProcessService.TryGetProcesses(defaultParm, out var runningProcesses))
            {
                result.Add(new(defaultParm, null));
                continue;
            }
             
            foreach (var runningProcess in runningProcesses)
            {
                result.Add(new (runningProcess.Parameters, runningProcess));
            }
        }

        return result;
    }

    private static List<Parameters> GetDefaultParameterList()
    {
        return new List<Parameters>()
        {
            new()
            {
                Name = "Silo Host",
                Command = "dotnet",
                Arguments = new() { "run" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.AppServer.SiloHost"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.AppServer.SiloHost"
                        : @"/mnt/d/projects/compass/HighMatch.Compass.AppServer.SiloHost"
            },
            new()
            {
                Name = "Admin API",
                Command = "dotnet",
                Arguments = new() { "run" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.Api.Administration"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.Api.Administration"
                        : @"/mnt/d/projects/compass/HighMatch.Compass.Api.Administration"
            },
            new()
            {
                Name = "Participant API",
                Command = "dotnet",
                Arguments = new() { "run" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.Api.Participant"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.Api.Administration" 
                        : @"/mnt/d/projects/compass/HighMatch.Compass.Api.Participant"
            }
        };
    }
}
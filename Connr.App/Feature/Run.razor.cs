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
                result.Add(new ValueTuple<Parameters, IProcessContainer?>(defaultParm, null));
                continue;
            }

            foreach (var runningProcess in runningProcesses)
                result.Add(new ValueTuple<Parameters, IProcessContainer>(runningProcess.Parameters, runningProcess));
        }

        return result;
    }

    private static List<Parameters> GetDefaultParameterList()
    {
        return new List<Parameters>
        {
            new()
            {
                Name = "Silo Host",
                Command = "dotnet",
                Arguments = new List<string> { "run" },
                AutoStart = true,
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.AppServer.SiloHost"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.AppServer.SiloHost"
                        : @"/mnt/d/projects/compass/HighMatch.Compass.AppServer.SiloHost"
            },
            new()
            {
                Name = "Publisher",
                Command = "dotnet",
                Arguments = new List<string> { "run" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\publisher\HighMatch.Publisher.Site"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/publisher/HighMatch.Publisher.Site"
                        : @"/mnt/d/projects/compass/publisher/HighMatch.Publisher.Site"
            },
            new()
            {
                Name = "Admin API",
                Command = "dotnet",
                Arguments = new List<string> { "run" },
                AutoStart = true,
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.Api.Administration"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.Api.Administration"
                        : @"/mnt/d/projects/compass/HighMatch.Compass.Api.Administration"
            },
            new()
            {
                Name = "Admin UI",
                Command = "npm",
                Arguments = new List<string> { "start" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\compass.ui.admin"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass.ui.admin"
                        : @"/mnt/d/projects/compass/compass.ui.admin"
            },
            new()
            {
                Name = "Participant API",
                Command = "dotnet",
                Arguments = new List<string> { "run" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.Api.Participant"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.Api.Participant"
                        : @"/mnt/d/projects/compass/HighMatch.Compass.Api.Participant"
            },
            new()
            {
                Name = "Participant and Reports UI",
                Command = "nx",
                Arguments = new List<string> { "run", "report:serve:development" },
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\compass.ui"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass.ui"
                        : @"/mnt/d/projects/compass/compass.ui"
            },
            new()
            {
                Name = "Job Link",
                Command = "dotnet",
                Arguments = new List<string> { "run" },
                AutoStart = true,
                WorkingDirectory = OperatingSystem.IsWindows()
                    ? @"D:\projects\compass\HighMatch.Compass.JobLink"
                    : OperatingSystem.IsMacOS()
                        ? @"/Users/jeff/projects/compass/HighMatch.Compass.JobLink"
                        : @"/mnt/d/projects/compass/HighMatch.Compass.JobLink"
            }
        };
    }
}
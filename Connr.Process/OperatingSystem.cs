using System.Runtime.InteropServices;

namespace Connr.Process;

public static class OperatingSystem
{
    //from https://mariusschulz.com/blog/detecting-the-operating-system-in-net-core
    
    public static bool IsWindows() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}
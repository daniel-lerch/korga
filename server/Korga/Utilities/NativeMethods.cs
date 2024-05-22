using System;
using System.Runtime.InteropServices;

namespace Korga.Utilities;

public static class NativeMethods
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    public static bool IsRunningInProcessIIS()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            GetModuleHandle("aspnetcorev2_inprocess.dll") != IntPtr.Zero;
    }
}

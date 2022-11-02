using System.Diagnostics;

namespace FancyMouse.Helpers;

internal static class DpiAwarenessHelper
{

    public static NativeMethods.PROCESS_DPI_AWARENESS GetProcessDpiAwareness()
    {
        // get the current process's dpi awareness mode
        // see https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8
        var process = Process.GetCurrentProcess();
        var apiResult = NativeMethods.GetProcessDpiAwareness(
            process.Handle,
            out var awareness
        );
        if (apiResult != NativeMethods.S_OK)
        {
            throw new InvalidOperationException(
                $"{nameof(NativeMethods.GetProcessDpiAwareness)} returned {apiResult}"
            );
        }
        return awareness;
    }

    public static void SetProcessDpiAwareness(NativeMethods.PROCESS_DPI_AWARENESS awareness)
    {
        // set the current process's dpi awarenees mode.
        // see https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8
        var apiResult = NativeMethods.SetProcessDpiAwareness(awareness);
        switch (apiResult)
        {
            case NativeMethods.S_OK:
                break;
            default:
                throw new InvalidOperationException(
                    $"{nameof(NativeMethods.SetProcessDpiAwareness)} returned {apiResult}"
                );
        }
    }


}

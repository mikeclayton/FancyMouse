using System.Diagnostics;

using FancyMouse.Common.NativeMethods;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

/// <summary>
/// Based on WinForms Application.HighDpiMode
/// see https://github.com/dotnet/winforms/blob/1c324d074280ab5de6342d973069faa687f2c165/src/System.Windows.Forms/System/Windows/Forms/Application.cs#L18
///     https://github.com/dotnet/winforms/blob/1c324d074280ab5de6342d973069faa687f2c165/src/System.Windows.Forms.Primitives/src/System/Windows/Forms/Internals/ScaleHelper.cs#L14
/// </summary>
public static class DpiModeHelper
{
    /// <summary>
    /// See https://github.com/dotnet/winforms/blob/bd91bfb26ce90ac31e950c01dcb2b6e0776453a7/src/System.Private.Windows.Core/src/System/Private/Windows/OsVersion.cs#L9
    /// </summary>
    private static bool IsWindows10_1607OrGreater()
        => OperatingSystem.IsWindowsVersionAtLeast(major: 10, build: 14393);

    /// <summary>
    /// See https://github.com/dotnet/winforms/blob/bd91bfb26ce90ac31e950c01dcb2b6e0776453a7/src/System.Private.Windows.Core/src/System/Private/Windows/OsVersion.cs#L9
    /// </summary>
    private static bool IsWindows8_1OrGreater()
        => OperatingSystem.IsWindowsVersionAtLeast(major: 6, minor: 3);

    /// <summary>
    /// See https://github.com/dotnet/winforms/blob/1c324d074280ab5de6342d973069faa687f2c165/src/System.Windows.Forms.Primitives/src/Windows/Win32/UI/HiDpi/DPI_AWARENESS_CONTEXT.cs#L17
    /// </summary>
    internal static bool AreEquivalent(DPI_AWARENESS_CONTEXT dpiContextA, DPI_AWARENESS_CONTEXT dpiContextB) =>
        DpiModeHelper.IsWindows10_1607OrGreater() && User32.AreDpiAwarenessContextsEqual(dpiContextA, dpiContextB);

    /// <remarks>
    /// See https://github.com/dotnet/winforms/blob/bd91bfb26ce90ac31e950c01dcb2b6e0776453a7/src/System.Windows.Forms/System/Windows/Forms/Application.cs#L18
    /// </remarks>
    public static Models.Display.HighDpiMode HighDpiMode
        => DpiModeHelper.GetThreadHighDpiMode();

    /// <summary>
    /// Gets the DPI mode for the current thread.
    /// </summary>
    /// <remarks>
    /// See https://github.com/dotnet/winforms/blob/bd91bfb26ce90ac31e950c01dcb2b6e0776453a7/src/System.Windows.Forms.Primitives/src/System/Windows/Forms/Internals/ScaleHelper.cs#L368
    /// </remarks>
    private static Models.Display.HighDpiMode GetThreadHighDpiMode()
    {
        // For Windows 10 RS2 and above
        if (DpiModeHelper.IsWindows10_1607OrGreater())
        {
            var dpiAwareness = User32.GetThreadDpiAwarenessContext();

            if (DpiModeHelper.AreEquivalent(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE))
            {
                return Models.Display.HighDpiMode.SystemAware;
            }

            if (DpiModeHelper.AreEquivalent(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE))
            {
                return Models.Display.HighDpiMode.DpiUnaware;
            }

            if (DpiModeHelper.AreEquivalent(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
            {
                return Models.Display.HighDpiMode.PerMonitorV2;
            }

            if (DpiModeHelper.AreEquivalent(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE))
            {
                return Models.Display.HighDpiMode.PerMonitor;
            }

            if (DpiModeHelper.AreEquivalent(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED))
            {
                return Models.Display.HighDpiMode.DpiUnawareGdiScaled;
            }
        }
        else if (DpiModeHelper.IsWindows8_1OrGreater())
        {
            User32.GetProcessDpiAwareness(HANDLE.Null, out var processDpiAwareness);
            switch (processDpiAwareness)
            {
                case PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE:
                    return Models.Display.HighDpiMode.DpiUnaware;
                case PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE:
                    return Models.Display.HighDpiMode.SystemAware;
                case PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE:
                    return Models.Display.HighDpiMode.PerMonitor;
            }
        }
        else
        {
            // Available on Vista and higher.
            return User32.IsProcessDPIAware()
                ? Models.Display.HighDpiMode.SystemAware
                : Models.Display.HighDpiMode.DpiUnaware;
        }

        // We should never get here.
        Debug.Fail("Unexpected DPI state.");
        return Models.Display.HighDpiMode.DpiUnaware;
    }
}

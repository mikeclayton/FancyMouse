using System.Diagnostics;

using FancyMouse.WinUI3.Win32Gen;

using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.WinUI3.Internal.Helpers;

/// <summary>
/// Launches the standalone <c>FancyMouse.SettingsUI</c> app, pointed at this app's own
/// <see cref="ConfigHelper.AppSettingsPath"/> so it edits the file this app is actually reading
/// (rather than falling back to its own "next to itself" default). Any changes made to the file
/// by the settings app are automatically re-read by *this* app's settings file watcher.
/// </summary>
internal static class SettingsLauncher
{
    private const string SettingsExeName = "FancyMouse.SettingsUI.exe";

    /// <summary>
    /// Launches the external settings editor. If an instance is already running
    /// it brings its window to the front instead of starting another instance.
    /// </summary>
    public static void Launch()
    {
        var existingWindow = SettingsLauncher.FindExistingSettingsWindow();
        if (existingWindow is HWND hWnd)
        {
            SettingsLauncher.ActivateWindow(hWnd);
            return;
        }

        var exePath = SettingsLauncher.ResolveSettingsExePath()
            ?? throw new InvalidOperationException($"Could not find {SettingsLauncher.SettingsExeName}.");

        var appSettingsPath = ConfigHelper.AppSettingsPath;
        var startInfo = new ProcessStartInfo(exePath)
        {
            UseShellExecute = true,
        };
        if (appSettingsPath is not null)
        {
            startInfo.ArgumentList.Add(Path.GetFullPath(appSettingsPath));
        }

        Process.Start(startInfo);
    }

    /// <summary>
    /// Looks for a running <see cref="SettingsExeName"/> process with a main window - there's
    /// normally at most one (this is exactly what stops there being more), but if several somehow
    /// exist (e.g. from before this check existed), any one of them is as good as another to
    /// activate.
    /// </summary>
    private static HWND? FindExistingSettingsWindow()
    {
        var processName = Path.GetFileNameWithoutExtension(SettingsLauncher.SettingsExeName);
        foreach (var process in Process.GetProcessesByName(processName))
        {
            using (process)
            {
                var handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    return (HWND)handle;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// SW_RESTORE first, unconditionally - a no-op if the window's already in its normal state,
    /// but necessary if it's minimized, since <c>SetForegroundWindow</c> alone can't un-minimize
    /// a window (see its own remarks in <c>PreviewWindow.utils.cs</c>'s <c>SetAsForegroundWindow</c>
    /// for the conditions under which the foreground-activation call itself is allowed to succeed).
    /// </summary>
    private static void ActivateWindow(HWND hWnd)
    {
        _ = User32.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_RESTORE).IgnoreFailure();
        _ = User32.SetForegroundWindow(hWnd).IgnoreFailure();
    }

    /// <summary>
    /// Looks for the settings exe next to this one first (the real, installed layout - both
    /// modules' exes side by side, matching how PowerToys itself lays out its modules). Falls
    /// back to the sibling project's own build output directory, purely so this works
    /// out of the box for local dev/spike runs without needing to copy anything - production
    /// installs never need this second path since both would already live in the same directory.
    /// </summary>
    private static string? ResolveSettingsExePath()
    {
        // look inthe same directory as this assembly
        var sameDirectory = Path.Combine(AppContext.BaseDirectory, SettingsLauncher.SettingsExeName);
        if (File.Exists(sameDirectory))
        {
            return sameDirectory;
        }

        // look in the sibling visual studio project build folder
        var devDirectory = AppContext.BaseDirectory.Replace(
            $"FancyMouse.WinUI3{Path.DirectorySeparatorChar}",
            $"FancyMouse.SettingsUI{Path.DirectorySeparatorChar}",
            StringComparison.Ordinal);
        var devPath = Path.Combine(devDirectory, SettingsLauncher.SettingsExeName);
        return File.Exists(devPath) ? devPath : null;
    }
}

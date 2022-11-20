using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys.Win32Api;

internal static class Kernel32
{

    /// <summary>
    /// Retrieves a module handle for the specified module. 
    /// </summary>
    /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file). </param>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the specified module.
    /// If the function fails, the return value is NULL.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-getmodulehandlea
    /// </remarks>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(
        string lpModuleName
    );

}

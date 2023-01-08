using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys.Win32Api;

/// <summary>
///
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/api/windef/
///     https://github.com/MicrosoftDocs/sdk-api/tree/docs/sdk-api-src/content/windef
/// </remarks>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal static class Kernel32
{

    #region Learn / Windows / Apps / Win32 / Desktop Technologies / System Services / Dynamic-Link Libraries
    // see https://learn.microsoft.com/en-us/windows/win32/dlls/dynamic-link-libraries

    #region Dynamic-Link Library Reference / Dynamic-Link Library Functions
    // see https://learn.microsoft.com/en-us/windows/win32/dlls/dynamic-link-library-functions

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

    #endregion

    #endregion

    #region Learn / Windows / Apps / Win32 / Desktop Technologies / System Services / Dynamic-Link Libraries
    // see https://learn.microsoft.com/en-us/windows/win32/procthread/processes-and-threads

    #region Process and Thread Reference / Process and Thread Functions / Thread Functions
    // see https://learn.microsoft.com/en-us/windows/win32/procthread/process-and-thread-functions#thread-functions

    /// <summary>
    /// Retrieves the thread identifier of the calling thread.
    /// </summary>
    /// <returns>
    /// The return value is the thread identifier of the calling thread.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentthreadid
    /// </remarks>
    [DllImport("kernel32.dll")]
    public static extern int GetCurrentThreadId();

    #endregion

    #endregion

}

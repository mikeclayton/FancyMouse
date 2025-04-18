using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Kernel32
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
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/Kernel32/Interop.GetModuleHandle.cs
    /// </remarks>
    [LibraryImport(Libraries.Kernel32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial HMODULE GetModuleHandleW(
        LPCWSTR lpModuleName);
}

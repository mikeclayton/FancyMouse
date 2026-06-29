using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Drawing.Win32Api;

internal static class Win32ResultExtensions
{
    /// <summary>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </summary>
    public static Win32Result<BOOL> SuccessIsNonZero(this BOOL value, [CallerMemberName] string methodName = "")
        => new(methodName, value, isSuccess: value != 0, useLastError: false, Marshal.GetLastPInvokeError());

    /// <summary>
    /// If the function succeeds, the return value is the requested value.
    /// If the function fails, the return value is zero.
    /// </summary>
    public static Win32Result<int> SuccessIsNonZero(this int value, [CallerMemberName] string methodName = "")
        => new(methodName, value, isSuccess: value != 0, useLastError: false, Marshal.GetLastPInvokeError());

    /// <summary>
    /// If the function succeeds, the return value is a non-null handle.
    /// If the function fails, the return value is null.
    /// </summary>
    public static Win32Result<HDC> SuccessIsNonNull(this HDC value, [CallerMemberName] string methodName = "")
        => new(methodName, value, isSuccess: !value.IsNull, useLastError: false, Marshal.GetLastPInvokeError());

    /// <summary>
    /// The return value itself is the result the caller wants - there is no
    /// separate success/failure outcome to interpret (e.g. a "no selection
    /// made" return is a valid outcome, not a failure).
    /// </summary>
    public static Win32Result<TValue> AlwaysSucceeds<TValue>(this TValue value, [CallerMemberName] string methodName = "")
        => new(methodName, value, isSuccess: true, useLastError: false, Marshal.GetLastPInvokeError());
}

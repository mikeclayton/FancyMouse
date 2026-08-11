using System.Runtime.CompilerServices;

using Windows.Win32.UI.WindowsAndMessaging;

internal static partial class Win32ReturnCode
{
    // see the comment in Win32ReturnCode_HWND.cs - each CsWin32 handle
    // struct needs its own overload here.
    internal static Win32ReturnCode<HMENU> SuccessIsNotNull(
        this HMENU result,
        [CallerMemberName] string memberName = "")
        => new(result, !result.IsNull, lastError: null, memberName);
}

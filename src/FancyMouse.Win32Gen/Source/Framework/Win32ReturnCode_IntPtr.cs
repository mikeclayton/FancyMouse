using System.Runtime.CompilerServices;

internal static partial class Win32ReturnCode
{
    public static Win32ReturnCode<nint> SuccessIsNonZero(
        this nint result,
        [CallerMemberName] string memberName = "")
        => new(result, result != 0, lastError: null, memberName);
}

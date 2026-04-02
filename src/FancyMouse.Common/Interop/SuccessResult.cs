namespace FancyMouse.Common.Interop;

internal static class SuccessResult
{
    public delegate bool SuccessPredicate(int result);

    public static bool IfZero(int result)
        => result == 0;

    public static bool IfNotZero(int result)
        => result != 0;
}

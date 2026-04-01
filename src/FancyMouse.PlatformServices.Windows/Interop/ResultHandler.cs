using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FancyMouse.PlatformServices.Windows.Interop;

internal static class ResultHandler
{
    public static int GetLastError()
    {
        return Marshal.GetLastPInvokeError();
    }

    public static void ThrowIfZero(
        int result,
        bool getLastError = false,
        [CallerMemberName] string memberName = "")
    {
        ResultHandler.HandleResult(
            result,
            SuccessResult.IfNotZero(result),
            getLastError ? ResultHandler.GetLastError : null,
            memberName);
    }

    public static void ThrowIfNotZero(
        int result,
        bool getLastError = false,
        [CallerMemberName] string memberName = "")
    {
        ResultHandler.HandleResult(
            result,
            SuccessResult.IfZero(result),
            getLastError ? ResultHandler.GetLastError : null,
            memberName);
    }

    /*
    internal static void HandleResult(
        int result,
        SuccessResult.SuccessPredicate successPredicate,
        [CallerMemberName] string memberName = "")
    {
        ResultHandler.HandleResult(
            result,
            successPredicate(result),
            getLastError: null,
            memberName);
    }
    */

    /*
    internal static void HandleResult(
        int result,
        bool success,
        [CallerMemberName] string memberName = "")
    {
        ResultHandler.HandleResult(
            result,
            success,
            getLastError: null,
            memberName);
    }
    */

    /*
    internal static void HandleResult(
        int result,
        SuccessResult.SuccessPredicate successPredicate,
        Func<int>? getLastError,
        [CallerMemberName] string memberName = "")
    {
        ResultHandler.HandleResult(
            result,
            successPredicate(result),
            getLastError,
            memberName);
    }
    */

    internal static void HandleResult(
        int result,
        bool success,
        bool getLastError = false,
        [CallerMemberName] string memberName = "")
    {
        if (success)
        {
            return;
        }

        var lastError = getLastError ? (int?)ResultHandler.GetLastError() : null;
        ResultHandler.HandleFailure(result, lastError, memberName);
    }

    internal static void HandleResult(
        int result,
        bool success,
        Func<int>? getLastError,
        [CallerMemberName] string memberName = "")
    {
        if (success)
        {
            return;
        }

        var lastError = getLastError?.Invoke();
        ResultHandler.HandleFailure(result, lastError, memberName);
    }

    internal static void HandleResult(
        int result,
        bool success,
        int? lastError,
        [CallerMemberName] string memberName = "")
    {
        if (success)
        {
            return;
        }

        ResultHandler.HandleFailure(result, lastError, memberName);
    }

    internal static void HandleFailure(
        int result,
        bool getLastError = false,
        [CallerMemberName] string memberName = "")
    {
        var lastError = getLastError ? (int?)ResultHandler.GetLastError() : null;
        ResultHandler.HandleFailure(result, lastError, memberName);
    }

    internal static void HandleFailure(
        int result,
        int? lastError,
        [CallerMemberName] string memberName = "")
    {
        var lines = new List<string>
        {
            $"{memberName} failed with result {result}.",
        };

        if (lastError is not null)
        {
            lines.Add(
                $"last error was '{lastError}'");
        }

        var message = string.Join(Environment.NewLine, lines);
        throw new Win32Exception(message);
    }
}

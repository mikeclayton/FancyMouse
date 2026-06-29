using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FancyMouse.Drawing.Win32Api;

internal readonly struct Win32Result<TValue>
{
    internal Win32Result(string methodName, TValue value, bool isSuccess, bool useLastError, int capturedLastError)
    {
        this.MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        this.Value = value;
        this.IsSuccess = isSuccess;
        this.UseLastError = useLastError;
        this.CapturedLastError = capturedLastError;
    }

    public string MethodName
    {
        get;
    }

    public TValue Value
    {
        get;
    }

    public bool IsSuccess
    {
        get;
    }

    public bool UseLastError
    {
        get;
    }

    public int CapturedLastError
    {
        get;
    }

    public int? LastError
        => this.UseLastError ? this.CapturedLastError : null;

    public Win32Result<TValue> UsesLastError()
        => new(this.MethodName, this.Value, this.IsSuccess, useLastError: true, this.CapturedLastError);

    public Win32Result<TValue> OnSuccess(Action<TValue> action)
    {
        if (this.IsSuccess)
        {
            action(this.Value);
        }

        return this;
    }

    public Win32Result<TValue> OnFailure(Action<TValue> action)
    {
        if (!this.IsSuccess)
        {
            action(this.Value);
        }

        return this;
    }

    public Win32Result<TValue> OnFailure(Action<TValue, int?> action)
    {
        if (!this.UseLastError)
        {
            throw new InvalidOperationException(
                $"PInvoke method '{this.MethodName}' doesn't set LastError.");
        }

        if (!this.IsSuccess)
        {
            action(this.Value, this.LastError);
        }

        return this;
    }

    public Win32Result<TValue> ThrowIfFailed()
    {
        if (this.IsSuccess)
        {
            return this;
        }
        else if (this.LastError is null)
        {
            throw new Win32Exception(
                $"{this.MethodName} returned {this.Value}");
        }
        else
        {
            throw new Win32Exception(
                $"{this.MethodName} returned {this.Value}, {nameof(Marshal.GetLastPInvokeError)} returned {this.LastError}");
        }
    }

    public TValue GetValue()
        => this.Value;

    public Win32Result<TValue> Failed()
        => new(this.MethodName, this.Value, isSuccess: false, this.UseLastError, this.CapturedLastError);

    public Win32Result<TValue> Succeeded()
        => new(this.MethodName, this.Value, isSuccess: true, this.UseLastError, this.CapturedLastError);

    /// <summary>
    /// No-op - exists purely as an explicit alternative to <see cref="ThrowIfFailed"/> at the
    /// call site, so a reader can tell "failure was deliberately considered and ignored" apart
    /// from "failure handling was simply forgotten". Pair every call with a comment explaining
    /// why ignoring failure is safe here.
    /// </summary>
    public Win32Result<TValue> IgnoreFailure()
        => this;
}

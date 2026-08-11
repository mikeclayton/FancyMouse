using System;

/// <summary>
/// Marks a wrapper method's success rule as too specific to fit one of the
/// canned <see cref="SuccessIsNonZeroAttribute"/>/<see cref="SuccessIsNotNullAttribute"/>/
/// <see cref="AlwaysSucceedsAttribute"/> shapes - <see cref="MethodName"/>
/// names the method (declared alongside the wrapper, in the same partial
/// class) that actually decides success, so the custom logic stays a real,
/// reviewable, named artifact instead of untagged inline code. Examples of
/// what this is for: SetCursorPos's zero-result/zero-last-error quirk, or
/// GetMessage's -1-means-failure/0-means-WM_QUIT tri-state.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class SuccessDelegateAttribute : Attribute
{
    public SuccessDelegateAttribute(string methodName)
    {
        this.MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
    }

    public string MethodName
    {
        get;
    }
}

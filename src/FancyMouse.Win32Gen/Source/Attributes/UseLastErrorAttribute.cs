using System;

/// <summary>
/// Marks a wrapper method as capturing <see cref="System.Runtime.InteropServices.Marshal.GetLastPInvokeError"/>
/// on failure - the declarative counterpart to chaining
/// <c>.WithLastError()</c> in the method body. Should only be present when
/// the function's real, CsWin32-generated <c>DllImport</c> actually sets
/// <c>SetLastError = true</c> - pair it with one of
/// <see cref="SuccessIsNonZeroAttribute"/>/<see cref="SuccessIsNotNullAttribute"/>,
/// or with <see cref="SuccessDelegateAttribute"/> if the failure detection
/// itself is custom.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class UseLastErrorAttribute : Attribute
{
}

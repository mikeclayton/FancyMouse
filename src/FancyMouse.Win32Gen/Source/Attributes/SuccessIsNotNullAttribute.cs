using System;

/// <summary>
/// Marks a wrapper method's success rule as "the raw return value (a
/// handle) is not null" - the declarative counterpart to chaining
/// <c>.SuccessIsNotNull()</c> in the method body.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class SuccessIsNotNullAttribute : Attribute
{
}

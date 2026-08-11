using System;

/// <summary>
/// Marks a wrapper method's success rule as "the raw return value is
/// nonzero" - the declarative counterpart to chaining
/// <c>.SuccessIsNonZero()</c> in the method body. Exists so the rule a
/// human (or a generator) chose is queryable as real metadata by other
/// tooling without needing to parse the method body to recover it.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class SuccessIsNonZeroAttribute : Attribute
{
}

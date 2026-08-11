using System;

/// <summary>
/// Marks a wrapper method as having no reliable failure signal, so it's
/// always treated as successful - the declarative counterpart to chaining
/// <c>.AlwaysSucceeds()</c> in the method body.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class AlwaysSucceedsAttribute : Attribute
{
}

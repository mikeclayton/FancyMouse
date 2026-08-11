using System;

/// <summary>
/// Marks a wrapper method's success/last-error classification as having
/// been checked by a human against the function's real documentation and
/// its actual generated <c>DllImport</c> attribute, rather than left as an
/// unreviewed guess. Downstream tooling - an analyzer over a consuming
/// project, for instance - can use its absence to flag "this call goes
/// through unverified error-handling logic".
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class HumanVerifiedAttribute : Attribute
{
}

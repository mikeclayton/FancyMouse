namespace System.Runtime.CompilerServices;

// netstandard2.0 doesn't ship this marker type, but the compiler needs it
// to allow init-only setters / records - polyfilling it here is the usual
// workaround for generator projects, which are stuck targeting netstandard2.0.
internal static class IsExternalInit
{
}

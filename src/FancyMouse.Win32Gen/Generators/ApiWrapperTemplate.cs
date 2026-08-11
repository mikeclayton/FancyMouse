namespace FancyMouse.Win32Gen.Generators;

/// <summary>
/// The hand-authored source for one api's <c>User32</c>/<c>Kernel32</c>/etc
/// wrapper method, keyed by the api name as it appears in NativeMethods.txt.
/// </summary>
/// <param name="ClassName">
/// The static wrapper class this method belongs in - which class an api's
/// wrapper lands in stands in for "which DLL is this api defined in".
/// </param>
/// <param name="MethodSource">
/// The complete method declaration, dedented to column 0. Substituted
/// verbatim into the generated partial class.
/// </param>
internal sealed record ApiWrapperTemplate(string ClassName, string MethodSource);

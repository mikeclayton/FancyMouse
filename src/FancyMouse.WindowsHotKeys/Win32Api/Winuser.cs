using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys.Win32Api;

/// <summary>
///
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/
///     https://github.com/MicrosoftDocs/sdk-api/tree/docs/sdk-api-src/content/winuser
/// </remarks>
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal static class Winuser
{

    #region Constants

    public const int HWND_MESSAGE = -3;

    public const uint CW_USEDEFAULT = 0x80000000;

    #endregion

}

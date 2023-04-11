﻿using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Synthesizes keystrokes, mouse motions, and button clicks.
    /// </summary>
    /// <returns>
    /// The function returns the number of events that it successfully inserted into the keyboard or mouse input stream.
    /// If the function returns zero, the input was already blocked by another thread.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial UINT SendInput(
        UINT cInputs,
        LPINPUT pInputs,
        int cbSize);
}

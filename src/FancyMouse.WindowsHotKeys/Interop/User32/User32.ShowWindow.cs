﻿using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys.Interop;

internal static partial class User32
{
    /// <summary>
    /// Sets the specified window's show state.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="nCmdShow">Controls how the window is to be shown. </param>
    /// <returns>
    /// If the window was previously visible, the return value is nonzero.
    /// If the window was previously hidden, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    public static partial uint ShowWindow(
        IntPtr hWnd,
        ShowWindowCommands nCmdShow);
}

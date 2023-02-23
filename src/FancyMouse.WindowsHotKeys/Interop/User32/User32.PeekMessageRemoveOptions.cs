﻿namespace FancyMouse.WindowsHotKeys.Interop;

internal static partial class User32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-peekmessagea
    /// </remarks>
    public enum PeekMessageRemoveOptions : uint
    {
        /// <summary>
        /// Messages are not removed from the queue after processing by PeekMessage.
        /// </summary>
        PM_NOREMOVE = 0x0000,

        /// <summary>
        /// Messages are removed from the queue after processing by PeekMessage.
        /// </summary>
        PM_REMOVE = 0x0001,

        /// <summary>
        /// Prevents the system from releasing any thread that is waiting for the caller to go idle (see WaitForInputIdle).
        /// Combine this value with either PM_NOREMOVE or PM_REMOVE.
        /// </summary>
        PM_NOYIELD = 0x0002,
    }
}

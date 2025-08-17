﻿namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Identifies dots per inch (dpi) awareness values. DPI awareness indicates how much scaling work
    /// an application performs for DPI versus how much is done by the system.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/ne-shellscalingapi-process_dpi_awareness
    /// </remarks>
    internal enum PROCESS_DPI_AWARENESS
    {
        /// <summary>
        /// DPI unaware. This app does not scale for DPI changes and is always assumed to have a scale factor
        /// of 100% (96 DPI). It will be automatically scaled by the system on any other DPI setting.
        /// </summary>
        PROCESS_DPI_UNAWARE = 0,

        /// <summary>
        /// System DPI aware. This app does not scale for DPI changes. It will query for the DPI once and use
        /// that value for the lifetime of the app. If the DPI changes, the app will not adjust to the new DPI
        /// value. It will be automatically scaled up or down by the system when the DPI changes from the
        /// system value.
        /// </summary>
        PROCESS_SYSTEM_DPI_AWARE = 1,

        /// <summary>
        /// Per monitor DPI aware. This app checks for the DPI when it is created and adjusts the scale factor
        /// whenever the DPI changes. These applications are not automatically scaled by the system.
        /// </summary>
        PROCESS_PER_MONITOR_DPI_AWARE = 2,
    }
}

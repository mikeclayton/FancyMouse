namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Identifies the dots per inch (dpi) setting for a thread, process, or window.
    /// </summary>
    /// <remarks>
    /// https://learn.microsoft.com/en-us/windows/win32/api/windef/ne-windef-dpi_awareness
    /// </remarks>
    internal enum DPI_AWARENESS
    {
#pragma warning disable CA1712 // Do not prefix enum values with the name of the enum type 'DPI_AWARENESS'
        /// <summary>
        /// Invalid DPI awareness. This is an invalid DPI awareness value.
        /// </summary>
        DPI_AWARENESS_INVALID = -1,

        /// <summary>
        /// DPI unaware. This process does not scale for DPI changes and is always assumed to have a scale
        /// factor of 100% (96 DPI). It will be automatically scaled by the system on any other DPI setting.
        /// </summary>
        DPI_AWARENESS_UNAWARE = 0,

        /// <summary>
        /// System DPI aware. This process does not scale for DPI changes. It will query for the DPI once
        /// and use that value for the lifetime of the process. If the DPI changes, the process will not
        /// adjust to the new DPI value. It will be automatically scaled up or down by the system when the
        /// DPI changes from the system value.
        /// </summary>
        DPI_AWARENESS_SYSTEM_AWARE = 1,

        /// <summary>
        /// Per monitor DPI aware. This process checks for the DPI when it is created and adjusts the scale
        /// factor whenever the DPI changes. These processes are not automatically scaled by the system.
        /// </summary>
        DPI_AWARENESS_PER_MONITOR_AWARE = 2,
#pragma warning restore
    }
}

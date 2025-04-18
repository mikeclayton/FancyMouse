using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Identifies the awareness context for a window.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/hidpi/dpi-awareness-context
    /// </remarks>
    internal enum DPI_AWARENESS_CONTEXT
    {
#pragma warning disable CA1712 // Do not prefix enum values with type name
        /// <summary>
        /// DPI unaware. This window does not scale for DPI changes and is always assumed to have
        /// a scale factor of 100% (96 DPI). It will be automatically scaled by the system on any other DPI setting.
        /// </summary>
        DPI_AWARENESS_CONTEXT_UNAWARE = -1,

        /// <summary>
        /// System DPI aware. This window does not scale for DPI changes. It will query for the DPI
        /// once and use that value for the lifetime of the process. If the DPI changes, the process
        /// will not adjust to the new DPI value. It will be automatically scaled up or down by the
        /// system when the DPI changes from the system value.
        /// </summary>
        DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2,

        /// <summary>
        /// Per monitor DPI aware. This window checks for the DPI when it is created and adjusts
        /// the scale factor whenever the DPI changes. These processes are not automatically scaled
        /// by the system.
        /// </summary>
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,

        /// <summary>
        /// Also known as Per Monitor v2. An advancement over the original per-monitor DPI awareness
        /// mode, which enables applications to access new DPI-related scaling behaviors on a per
        /// top-level window basis.
        /// </summary>
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,

        /// <summary>
        /// DPI unaware with improved quality of GDI-based content. This mode behaves similarly to
        /// DPI_AWARENESS_CONTEXT_UNAWARE, but also enables the system to automatically improve the
        /// rendering quality of text and other GDI-based primitives when the window is displayed
        /// on a high-DPI monitor.
        /// </summary>
        DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = -5,
#pragma warning restore CA1712 // Do not prefix enum values with type name
    }
}

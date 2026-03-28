using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Shell32
{
    /// <summary>
    /// Sends a message to the taskbar's status area.
    /// </summary>
    /// <returns>
    /// Returns TRUE if successful, or FALSE otherwise.
    /// If dwMessage is set to NIM_SETVERSION, the function returns TRUE if the version
    /// was successfully changed, or FALSE if the requested version is not supported.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PNOTIFYICONDATAW
    {
        public static readonly PNOTIFYICONDATAW Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public PNOTIFYICONDATAW(IntPtr value)
        {
            this.Value = value;
        }

        public PNOTIFYICONDATAW(NOTIFYICONDATAW value)
        {
            this.Value = PNOTIFYICONDATAW.ToPtr(value);
        }

        private static IntPtr ToPtr(NOTIFYICONDATAW value)
        {
            var ptr = Marshal.AllocHGlobal(NOTIFYICONDATAW.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public NOTIFYICONDATAW ToStructure()
        {
            return Marshal.PtrToStructure<NOTIFYICONDATAW>(this.Value);
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(PNOTIFYICONDATAW value) => value.Value;

        public static implicit operator PNOTIFYICONDATAW(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}

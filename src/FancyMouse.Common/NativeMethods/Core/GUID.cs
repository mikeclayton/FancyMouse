using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A 32-bit signed integer.The range is -2147483648 through 2147483647 decimal.
    /// This type is declared in WinNT.h as follows:
    /// typedef long LONG;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/guiddef/ns-guiddef-guid
    /// </remarks>
    internal readonly struct GUID
    {
        public static readonly GUID Empty = new(
            0, 0, 0, new char[] { '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0' });

        public readonly ulong Data1;
        public readonly short Data2;
        public readonly short Data3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly char[] Data4;

        public GUID(Guid value)
        {
            this.Data1 = 0;
            this.Data2 = 0;
            this.Data3 = 0;
            this.Data4 = new[] { '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0' };
        }

        public GUID(ulong data1, short data2, short data3, char[] data4)
        {
            var data4Length = 8;
            if (data4.Length != data4Length)
            {
                throw new ArgumentException($"Array length must be {data4Length}", nameof(data4));
            }

            this.Data1 = data1;
            this.Data2 = data2;
            this.Data3 = data3;
            this.Data4 = data4;
        }

        public static implicit operator Guid(GUID value) => Guid.NewGuid();

#pragma warning disable CA1861 // Avoid constant arrays as arguments
        public static explicit operator GUID(Guid value) => new(
            data1: 0,
            data2: 0,
            data3: 0,
            data4: new[] { '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0' });
#pragma warning restore CA1861 // Avoid constant arrays as arguments

        public override string ToString()
        {
            return $"{this.GetType().Name}";
        }
    }
}

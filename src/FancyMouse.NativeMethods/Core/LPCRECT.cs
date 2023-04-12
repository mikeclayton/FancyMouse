﻿using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    internal readonly struct LPCRECT
    {
        public static readonly LPCRECT Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPCRECT(IntPtr value)
        {
            this.Value = value;
        }

        public LPCRECT(CRECT value)
        {
            this.Value = LPCRECT.ToPtr(value);
        }

        private static IntPtr ToPtr(CRECT value)
        {
            var ptr = Marshal.AllocHGlobal(CRECT.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(LPCRECT value) => value.Value;

        public static implicit operator LPCRECT(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}

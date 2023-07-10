﻿using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    internal readonly struct LPPOINT
    {
        public static readonly LPPOINT Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPPOINT(IntPtr value)
        {
            this.Value = value;
        }

        public LPPOINT(POINT value)
        {
            this.Value = LPPOINT.ToPtr(value);
        }

        public bool IsNull => this.Value == LPPOINT.Null.Value;

        private static IntPtr ToPtr(POINT value)
        {
            var ptr = Marshal.AllocHGlobal(POINT.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public POINT ToStructure()
        {
            return Marshal.PtrToStructure<POINT>(this.Value);
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(LPPOINT value) => value.Value;

        public static implicit operator LPPOINT(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}

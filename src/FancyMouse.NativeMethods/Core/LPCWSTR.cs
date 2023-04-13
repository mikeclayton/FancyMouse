﻿using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// A pointer to a constant null-terminated string of 16-bit Unicode characters.For more information, see Character Sets Used By Fonts.
    /// This type is declared in WinNT.h as follows:
    /// typedef CONST WCHAR* LPCWSTR;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct LPCWSTR
    {
        public static readonly LPCWSTR Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPCWSTR(IntPtr value)
        {
            this.Value = value;
        }

        public LPCWSTR(string value)
        {
            this.Value = Marshal.StringToHGlobalAuto(value);
        }

        public static implicit operator IntPtr(LPCWSTR value) => value.Value;

        public static implicit operator LPCWSTR(IntPtr value) => new(value);

        public static implicit operator string?(LPCWSTR value) => Marshal.PtrToStringUni(value.Value);

        public static implicit operator LPCWSTR(string value) => new(value);

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}

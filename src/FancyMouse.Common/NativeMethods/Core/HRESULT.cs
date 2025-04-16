﻿namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// The return codes used by COM interfaces.For more information, see Structure of the COM Error Codes.
    /// To test an HRESULT value, use the FAILED and SUCCEEDED macros.
    /// This type is declared in WinNT.h as follows:
    /// typedef LONG HRESULT;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct HRESULT
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly int Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HRESULT(int value)
        {
            this.Value = value;
        }

        public static implicit operator int(HRESULT value) => value.Value;

        public static explicit operator HRESULT(int value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}

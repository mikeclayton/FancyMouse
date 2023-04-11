﻿namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// An unsigned INT. The range is 0 through 4294967295 decimal.
    /// This type is declared in WinDef.h as follows:
    /// typedef unsigned int UINT;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct UINT
    {
        public readonly uint Value;

        public UINT(uint value)
        {
            this.Value = value;
        }

        public static implicit operator uint(UINT value) => value.Value;

        public static implicit operator UINT(uint value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}

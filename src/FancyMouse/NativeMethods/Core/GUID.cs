// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.NativeMethods;

internal static partial class Core
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
        public readonly ulong Data1;
        public readonly short Data2;
        public readonly short Data3;
        public readonly char[] Data4;

        public GUID(Guid value)
        {
            this.Data1 = 0;
            this.Data2 = 0;
            this.Data3 = 0;
            this.Data4 = new[] { (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0 };
        }

        public GUID(ulong data1, short data2, short data3, char[] data4)
        {
            this.Data1 = data1;
            this.Data2 = data2;
            this.Data3 = data3;
            this.Data4 = data4;
        }

        public static implicit operator Guid(GUID value) => Guid.NewGuid();

        public static implicit operator GUID(Guid value) => new(
            data1: 0,
            data2: 0,
            data3: 0,
            data4: new[] { (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0 });

        public override string ToString()
        {
            return $"{this.GetType().Name}";
        }
    }
}

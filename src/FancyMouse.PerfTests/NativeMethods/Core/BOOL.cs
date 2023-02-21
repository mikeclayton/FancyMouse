namespace FancyMouse.PerfTests.NativeMethods.Core
{

    public readonly struct BOOL
    {

        public readonly int Value;

        public BOOL(int value)
        {
            this.Value = value;
        }

        public BOOL(bool value)
        {
            this.Value = value ? 1 : 0;
        }

        public static implicit operator bool(BOOL value) => value.Value != 0;
        public static implicit operator BOOL(bool value) => new(value);

        public static implicit operator int(BOOL value) => value;
        public static implicit operator BOOL(int value) => new(value);

    }

}

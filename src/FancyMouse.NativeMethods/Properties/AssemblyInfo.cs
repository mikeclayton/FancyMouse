using System.Runtime.CompilerServices;

// resolve "CA1401 - P/Invoke method should not be visible"
// by making internals available to other selected assemblies
[assembly: InternalsVisibleTo("FancyMouse")]
[assembly: InternalsVisibleTo("FancyMouse.UnitTests")]
[assembly: InternalsVisibleTo("FancyMouse.WindowsHotKeys")]

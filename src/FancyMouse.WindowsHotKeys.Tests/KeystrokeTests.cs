using NUnit.Framework;

namespace FancyMouse.WindowsHotKeys.Tests;

public static class KeystrokeTests
{

    public static class ParseTests
    {

        [TestCase("CTRL", KeyModifiers.Control, Keys.None)]
        [TestCase("ALT", KeyModifiers.Alt, Keys.None)]
        [TestCase("SHIFT", KeyModifiers.Shift, Keys.None)]
        [TestCase("WIN", KeyModifiers.Windows, Keys.None)]
        [TestCase("F", KeyModifiers.None, Keys.F)]
        [TestCase("CTRL + ALT + SHIFT", KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift, Keys.None)]
        [TestCase("SHIFT + ALT + CTRL", KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift, Keys.None)]
        [TestCase("CTRL + ALT + SHIFT + F", KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift, Keys.F)]
        [TestCase("CTRL + ALT + SHIFT + WIN + F", KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift | KeyModifiers.Windows, Keys.F)]
        public static void ValidStringsShouldParse(string s, KeyModifiers modifier, Keys key)
        {
            var expected = new Keystroke(key, modifier);
            var actual = Keystroke.Parse(s);
            Assert.Multiple(() => {
                Assert.AreEqual(expected.Key, actual.Key);
                Assert.AreEqual(expected.Modifiers, actual.Modifiers);
            });
        }

    }

}

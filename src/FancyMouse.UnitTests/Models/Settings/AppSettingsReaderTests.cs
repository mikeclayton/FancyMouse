using System.Drawing;
using System.Text.Json;
using FancyMouse.Models.Settings;
using FancyMouse.Models.Styles;
using FancyMouse.WindowsHotKeys;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FancyMouse.UnitTests.Models.Settings;

[TestClass]
public sealed class AppSettingsReaderTests
{
    [TestClass]
    public sealed class ParseJsonTests
    {
        [TestMethod]
        public void NullJsonShouldReturnDefaultSettings()
        {
            var actual = AppSettingsReader.ParseJson(null);
            var expected = AppSettings.DefaultSettings;
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void EmptyJsonShouldReturnDefaultSettings()
        {
            var actual = AppSettingsReader.ParseJson(string.Empty);
            var expected = AppSettings.DefaultSettings;
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void InvalidJsonShouldReturnDefaultSettings()
        {
            var actual = AppSettingsReader.ParseJson("xxx");
            var expected = AppSettings.DefaultSettings;
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void MissingVersionShouldBeTreatedAsVersion1()
        {
            var actual = AppSettingsReader.ParseJson("{}");
            var expected = AppSettings.DefaultSettings;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        [TestMethod]
        public void EmptyVersion1ShouldParse()
        {
            var json = ParseJsonTests.SerializeAnonymousType(
                new
                {
                    version = 1,
                });
            var actual = AppSettingsReader.ParseJson(json);
            var expected = AppSettings.DefaultSettings;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        [TestMethod]
        public void Version1WithNullRootKeysShouldParse()
        {
            var json = ParseJsonTests.SerializeAnonymousType(
                new
                {
                    version = 1,
                    fancymouse = (object?)null,
                });
            var actual = AppSettingsReader.ParseJson(json);
            var expected = AppSettings.DefaultSettings;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        [TestMethod]
        public void Version1WithNullChildKeysShouldParse()
        {
            var json = ParseJsonTests.SerializeAnonymousType(
                new
                {
                    version = 1,
                    fancymouse = new
                    {
                        hotkey = (string?)null,
                    },
                });
            var actual = AppSettingsReader.ParseJson(json);
            var expected = AppSettings.DefaultSettings;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        [TestMethod]
        public void Version1WithAllValuesShouldParse()
        {
            var json = ParseJsonTests.SerializeAnonymousType(
                new
                {
                    version = 1,
                    fancymouse = new
                    {
                        hotkey = "CTRL + ALT + X",
                        preview = "800 x 600",
                    },
                });
            var actual = AppSettingsReader.ParseJson(json);
            var expected = new AppSettings(
                hotkey: new(
                    key: Keys.X,
                    modifiers: KeyModifiers.Control | KeyModifiers.Alt
                ),
                previewStyle: new(
                    canvasSize: new(
                        width: 800,
                        height: 600
                    ),
                    canvasStyle: AppSettings.DefaultSettings.PreviewStyle.CanvasStyle,
                    screenshotStyle: AppSettings.DefaultSettings.PreviewStyle.ScreenshotStyle
                ));
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        [TestMethod]
        public void Version2WithNullRootKeysShouldParse()
        {
            var json = ParseJsonTests.SerializeAnonymousType(
                new
                {
                    version = 2,
                    hotkey = (object?)null,
                    preview = (object?)null,
                });
            var actual = AppSettingsReader.ParseJson(json);
            var expected = AppSettings.DefaultSettings;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        [TestMethod]
        public void Version2WithAllValuesShouldParse()
        {
            var json = ParseJsonTests.SerializeAnonymousType(
                new
                {
                    version = 2,
                    hotkey = new
                    {
                        key = Keys.X.ToString(),
                        modifiers = (KeyModifiers.Control | KeyModifiers.Alt).ToString(),
                    },
                    preview = new
                    {
                        size = new
                        {
                            width = 800,
                            height = 600,
                        },
                        canvas = new
                        {
                            border = new
                            {
                                color = $"{nameof(SystemColors)}.{nameof(SystemColors.Control)}",
                                width = 5,
                                depth = 2,
                            },
                            padding = new
                            {
                                width = 2,
                            },
                            background = new
                            {
                                color1 = $"{nameof(Color)}.{nameof(Color.Green)}",
                                color2 = $"{nameof(Color)}.{nameof(Color.Blue)}",
                            },
                        },
                        screenshot = new
                        {
                            margin = new
                            {
                                width = 10,
                            },
                            border = new
                            {
                                color = $"{nameof(SystemColors)}.{nameof(SystemColors.Control)}",
                                width = 5,
                                depth = 2,
                            },
                            background = new
                            {
                                color1 = $"{nameof(Color)}.{nameof(Color.Yellow)}",
                                color2 = $"{nameof(Color)}.{nameof(Color.Pink)}",
                            },
                        },
                    },
                });
            var actual = AppSettingsReader.ParseJson(json);
            var expected = new AppSettings(
                hotkey: new(
                    key: Keys.X,
                    modifiers: KeyModifiers.Control | KeyModifiers.Alt
                ),
                previewStyle: new(
                    canvasSize: new(
                        width: 800,
                        height: 600
                    ),
                    canvasStyle: new(
                        marginStyle: MarginStyle.Empty,
                        borderStyle: new(
                            color: SystemColors.Control,
                            all: 5,
                            depth: 2
                        ),
                        paddingStyle: new(
                            all: 2
                        ),
                        backgroundStyle: new(
                            color1: Color.Green,
                            color2: Color.Blue
                        )
                    ),
                    screenshotStyle: new(
                        marginStyle: new(
                            all: 10
                        ),
                        borderStyle: new(
                            color: SystemColors.Control,
                            all: 5,
                            depth: 2
                        ),
                        paddingStyle: PaddingStyle.Empty,
                        backgroundStyle: new(
                            color1: Color.Yellow,
                            color2: Color.Pink
                        )
                    )
                ));
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }

        private static string SerializeAnonymousType<T>(T value)
        {
            var options = new JsonSerializerOptions
            {
                 WriteIndented = true,
            };
            return JsonSerializer.Serialize(value, options);
        }
    }
}

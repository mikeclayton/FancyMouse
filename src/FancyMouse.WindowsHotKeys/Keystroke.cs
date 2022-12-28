namespace FancyMouse.WindowsHotKeys;

public sealed class Keystroke
{

    #region Constructors

    public Keystroke(Keys key, KeyModifiers modifiers)
    {
        this.Key = key;
        this.Modifiers = modifiers;
    }

    #endregion

    #region Properties

    public Keys Key
    {
        get;
    }

    public KeyModifiers Modifiers
    {
        get;
    }

    #endregion

    #region Static Methods

    public static Keystroke Parse(string s)
    {

        // see https://github.com/microsoft/terminal/blob/14919073a12fc0ecb4a9805cc183fdd68d30c4b6/src/cascadia/TerminalSettingsModel/KeyChordSerialization.cpp#L124
        // for an alternate implementation

        // e.g. "CTRL + ALT + SHIFT + F"
        if (s == null)
        {
            throw new ArgumentNullException(nameof(s));
        }
        var parts = s
            .Replace(" ", string.Empty)
            .ToUpperInvariant()
            .Split('+');
        var keystroke = (Keys: Keys.None, Modifiers: KeyModifiers.None);
        foreach (var part in parts)
        {
            switch (part)
            {
                case "CTRL":
                    keystroke.Modifiers |= KeyModifiers.Control;
                    break;
                case "ALT":
                    keystroke.Modifiers |= KeyModifiers.Alt;
                    break;
                case "SHIFT":
                    keystroke.Modifiers |= KeyModifiers.Shift;
                    break;
                case "WIN":
                    keystroke.Modifiers |= KeyModifiers.Windows;
                    break;
                default:
                    keystroke.Keys = Enum.Parse<Keys>(part);
                    break;
            }
        };
        return new Keystroke(keystroke.Keys, keystroke.Modifiers);
    }

    #endregion

    #region Object Interface

    public override string ToString()
    {
        var parts = new List<string>();
        if (this.Modifiers.HasFlag(KeyModifiers.Control))
        {
            parts.Add("CTRL");
        }
        if (this.Modifiers.HasFlag(KeyModifiers.Alt))
        {
            parts.Add("ALT");
        }
        if (this.Modifiers.HasFlag(KeyModifiers.Shift))
        {
            parts.Add("SHIFT");
        }
        if (this.Modifiers.HasFlag(KeyModifiers.Windows))
        {
            parts.Add("WIN");
        }
        if (this.Key != Keys.None)
        {
            parts.Add(this.Key.ToString());
        }
        return string.Join(" + ", parts);
    }

    #endregion

}

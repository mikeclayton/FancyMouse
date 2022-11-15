namespace KeyboardWatcher.Extensions;

public static class KeyEventArgsExtensions
{

    public static string ToKeySequence(this KeyEventArgs args)
    {
        var parts = new List<string>();
        if (args.Control) { parts.Add("CTRL"); }
        if (args.Alt) { parts.Add("ALT"); }
        if (args.Shift) { parts.Add("SHIFT"); }
        parts.Add(args.KeyCode.ToString());
        return string.Join(" + ", parts);
    }

}

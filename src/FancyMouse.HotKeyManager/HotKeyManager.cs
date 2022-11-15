using FancyMouse.HotKeyManager.Win32Api;

namespace FancyMouse.HotKeyManager;

/// <summary>
///
/// </summary>
/// <remarks>
/// See https://stackoverflow.com/a/3654821/3156906
///     https://learn.microsoft.com/en-us/archive/msdn-magazine/2007/june/net-matters-handling-messages-in-console-apps
///     https://www.codeproject.com/Articles/5274425/Understanding-Windows-Message-Queues-for-the-Cshar
/// </remarks>
public sealed class HotKeyManager
{

    #region Fields

    internal volatile MessageWindow _wnd;
    internal volatile IntPtr _hwnd;
    internal ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);

    private int _id = 0;

    #endregion

    #region Events

    delegate bool RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
    delegate bool UnregisterHotKeyDelegate(IntPtr hwnd, int id);

    public event EventHandler<HotKeyEventArgs> HotKeyPressed;

    #endregion

    #region Constructors

    public HotKeyManager()
    {
        var messageLoop = new Thread(delegate () {
            Application.Run(new MessageWindow(this));
        }) {
            Name = "MessageLoopThread",
            IsBackground = true
        };
        messageLoop.Start();
    }

    #endregion

    #region Properties

    #endregion

    #region Instance Methods

    public int RegisterHotKey(Keys key, KeyModifiers modifiers)
    {
        _windowReadyEvent.WaitOne();
        int id = Interlocked.Increment(ref _id);
        _wnd.Invoke(
            new RegisterHotKeyDelegate(Winuser.RegisterHotKey), _hwnd, id, (uint)modifiers, (uint)key
        );
        return id;
    }

    public void UnregisterHotKey(int id)
    {
        _wnd.Invoke(
            new UnregisterHotKeyDelegate(Winuser.UnregisterHotKey), _hwnd, id
        );
    }

    internal void OnHotKeyPressed(HotKeyEventArgs e)
    {
        this.HotKeyPressed?.Invoke(null, e);
    }

    #endregion

    #region Static Methods

    public static (Keys Keys, KeyModifiers Modifiers) Parse(string s)
    {
        // e.g. "CTRL + ALT + SHIFT + F"
        if (s == null)
        {
            throw new ArgumentNullException(nameof(s));
        }
        var parts = s.Replace(" ", string.Empty).ToUpperInvariant().Split('+');
        var keys = (Keys: Keys.None, Modifiers: KeyModifiers.None);
        foreach (var part in parts)
        {
            switch (part)
            {
                case "CTRL":
                    keys.Modifiers |= KeyModifiers.Control;
                    break;
                case "ALT":
                    keys.Modifiers |= KeyModifiers.Alt;
                    break;
                case "SHIFT":
                    keys.Modifiers |= KeyModifiers.Shift;
                    break;
                default:
                    keys.Keys = Enum.Parse<Keys>(part);
                    break;
            }
        };
        return keys;
    }

    #endregion

}

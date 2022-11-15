namespace FancyMouse.HotKeyManager;

internal class MessageWindow : Form
{

    private const int WM_HOTKEY = 0x312;

    #region Constructors

    public MessageWindow(HotKeyManager owner)
    {
        this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        this.Owner._wnd = this;
        this.Owner._hwnd = this.Handle;
        this.Owner._windowReadyEvent.Set();
    }

    #endregion

    #region Properties

    public HotKeyManager Owner
    {
        get;
    }

    #endregion

    #region Form Overrides

    protected override void WndProc(ref Message m)
    {

        if (m.Msg == WM_HOTKEY)
        {

            uint param = (uint)m.LParam.ToInt64();
            var key = (Keys)((param & 0xffff0000) >> 16);
            var modifiers = (KeyModifiers)(param & 0x0000ffff);
            HotKeyEventArgs e = new HotKeyEventArgs(key, modifiers);
            this.Owner.OnHotKeyPressed(e);

        }

        base.WndProc(ref m);

    }

    protected override void SetVisibleCore(bool value)
    {
        // Ensure the window never becomes visible
        base.SetVisibleCore(false);
    }

    #endregion

    private void InitializeComponent()
    {
            this.SuspendLayout();
            //
            // MessageWindow
            //
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MessageWindow";
            this.ResumeLayout(false);

    }

}

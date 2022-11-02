namespace FancyMouse.UI;

public partial class FancyMouseNotify : Form
{

    #region Constructors

    public FancyMouseNotify()
    {
        InitializeComponent();
    }

    #endregion

    #region Form Events

    private void FancyMouseNotify_Load(object sender, EventArgs e)
    {
    }

    private void FancyMouseNotify_FormClosed(object sender, FormClosedEventArgs e)
    {
        notifyIcon1.Visible = false;
    }

    #endregion

}

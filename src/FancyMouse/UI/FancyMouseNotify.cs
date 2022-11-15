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

    private void FancyMouseNotify_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    #endregion

    #region Context Menu Methods

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        notifyIcon1.Visible = false;
        Application.DoEvents();
        Application.Exit();
    }

    #endregion

}

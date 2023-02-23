namespace FancyMouse.UI;

public partial class FancyMouseNotify : Form
{
    public FancyMouseNotify()
    {
        InitializeComponent();
    }

    private void FancyMouseNotify_Load(object sender, EventArgs e)
    {
    }

    private void FancyMouseNotify_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        notifyIcon1.Visible = false;
        Application.DoEvents();
        Application.Exit();
    }
}

namespace FancyMouse.UI;

partial class FancyMouseForm {

    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FancyMouseForm));
        panel1 = new Panel();
        Thumbnail = new PictureBox();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)Thumbnail).BeginInit();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.BackColor = SystemColors.Highlight;
        panel1.Controls.Add(Thumbnail);
        panel1.Dock = DockStyle.Fill;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Padding = new Padding(5);
        panel1.Size = new Size(800, 450);
        panel1.TabIndex = 1;
        // 
        // Thumbnail
        // 
        Thumbnail.BackColor = SystemColors.ControlDarkDark;
        Thumbnail.Dock = DockStyle.Fill;
        Thumbnail.Location = new Point(5, 5);
        Thumbnail.Name = "Thumbnail";
        Thumbnail.Size = new Size(790, 440);
        Thumbnail.SizeMode = PictureBoxSizeMode.StretchImage;
        Thumbnail.TabIndex = 1;
        Thumbnail.TabStop = false;
        Thumbnail.Click += Thumbnail_Click;
        // 
        // FancyMouseForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(panel1);
        FormBorderStyle = FormBorderStyle.None;
        Icon = (Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        Name = "FancyMouseForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Text = "FancyMouse";
        TopMost = true;
        Deactivate += FancyMouseForm_Deactivate;
        Load += FancyMouseForm_Load;
        KeyDown += FancyMouseForm_KeyDown;
        panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)Thumbnail).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel panel1;
    private PictureBox Thumbnail;

}

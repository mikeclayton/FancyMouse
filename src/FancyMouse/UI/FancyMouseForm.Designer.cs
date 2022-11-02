namespace FancyMouse;

partial class FancyMouseForm
{

    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FancyMouseForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.pbxPreview = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Highlight;
            this.panel1.Controls.Add(this.pbxPreview);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(800, 450);
            this.panel1.TabIndex = 1;
            // 
            // pbxPreview
            // 
            this.pbxPreview.BackColor = System.Drawing.SystemColors.Control;
            this.pbxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxPreview.Location = new System.Drawing.Point(5, 5);
            this.pbxPreview.Name = "pbxPreview";
            this.pbxPreview.Size = new System.Drawing.Size(790, 440);
            this.pbxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxPreview.TabIndex = 1;
            this.pbxPreview.TabStop = false;
            this.pbxPreview.Click += new System.EventHandler(this.pbxPreview_Click);
            // 
            // FancyMouseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "FancyMouseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FancyMouse";
            this.Deactivate += new System.EventHandler(this.FancyMouseForm_Deactivate);
            this.Load += new System.EventHandler(this.FancyMouseForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FancyMouseForm_KeyDown);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private Panel panel1;
    private PictureBox pbxPreview;

}

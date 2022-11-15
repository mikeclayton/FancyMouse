namespace FancyMouse.UI;

partial class FancyMouseSettings
{

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FancyMouseSettings));
            this.udnPreviewMaxWidth = new System.Windows.Forms.NumericUpDown();
            this.udnPreviewMaxHeight = new System.Windows.Forms.NumericUpDown();
            this.lblPreviewImageX = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.udnPreviewMaxWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udnPreviewMaxHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // udnPreviewMaxWidth
            // 
            this.udnPreviewMaxWidth.Location = new System.Drawing.Point(58, 22);
            this.udnPreviewMaxWidth.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.udnPreviewMaxWidth.Name = "udnPreviewMaxWidth";
            this.udnPreviewMaxWidth.Size = new System.Drawing.Size(79, 23);
            this.udnPreviewMaxWidth.TabIndex = 0;
            this.udnPreviewMaxWidth.Value = new decimal(new int[] {
            1600,
            0,
            0,
            0});
            // 
            // udnPreviewMaxHeight
            // 
            this.udnPreviewMaxHeight.Location = new System.Drawing.Point(187, 22);
            this.udnPreviewMaxHeight.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.udnPreviewMaxHeight.Name = "udnPreviewMaxHeight";
            this.udnPreviewMaxHeight.Size = new System.Drawing.Size(79, 23);
            this.udnPreviewMaxHeight.TabIndex = 1;
            this.udnPreviewMaxHeight.Value = new decimal(new int[] {
            1200,
            0,
            0,
            0});
            // 
            // lblPreviewImageX
            // 
            this.lblPreviewImageX.AutoSize = true;
            this.lblPreviewImageX.Location = new System.Drawing.Point(156, 24);
            this.lblPreviewImageX.Name = "lblPreviewImageX";
            this.lblPreviewImageX.Size = new System.Drawing.Size(13, 15);
            this.lblPreviewImageX.TabIndex = 2;
            this.lblPreviewImageX.Text = "x";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(143, 330);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(233, 330);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // FancyMouseSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 365);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblPreviewImageX);
            this.Controls.Add(this.udnPreviewMaxHeight);
            this.Controls.Add(this.udnPreviewMaxWidth);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FancyMouseSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FancyMouse Settings";
            this.Load += new System.EventHandler(this.FancyMouseOptions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.udnPreviewMaxWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udnPreviewMaxHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private NumericUpDown udnPreviewMaxWidth;
    private NumericUpDown udnPreviewMaxHeight;
    private Label lblPreviewImageX;
    private Button button1;
    private Button button2;
}
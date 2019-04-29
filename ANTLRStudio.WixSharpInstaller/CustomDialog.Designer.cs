partial class CustomDialog
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
            this.cancelBtn = new System.Windows.Forms.Button();
            this.nextBtn = new System.Windows.Forms.Button();
            this.backBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.WPFRadioButton = new System.Windows.Forms.RadioButton();
            this.WinFormsRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(463, 298);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 8;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // nextBtn
            // 
            this.nextBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nextBtn.Location = new System.Drawing.Point(367, 298);
            this.nextBtn.Name = "nextBtn";
            this.nextBtn.Size = new System.Drawing.Size(75, 23);
            this.nextBtn.TabIndex = 7;
            this.nextBtn.Text = "Next";
            this.nextBtn.UseVisualStyleBackColor = true;
            this.nextBtn.Click += new System.EventHandler(this.nextBtn_Click);
            // 
            // backBtn
            // 
            this.backBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.backBtn.Location = new System.Drawing.Point(286, 298);
            this.backBtn.Name = "backBtn";
            this.backBtn.Size = new System.Drawing.Size(75, 23);
            this.backBtn.TabIndex = 6;
            this.backBtn.Text = "Back";
            this.backBtn.UseVisualStyleBackColor = true;
            this.backBtn.Click += new System.EventHandler(this.backBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 20);
            this.label1.TabIndex = 9;
            // 
            // WPFRadioButton
            // 
            this.WPFRadioButton.AutoSize = true;
            this.WPFRadioButton.Checked = true;
            this.WPFRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.WPFRadioButton.Location = new System.Drawing.Point(4, 46);
            this.WPFRadioButton.Name = "WPFRadioButton";
            this.WPFRadioButton.Size = new System.Drawing.Size(241, 21);
            this.WPFRadioButton.TabIndex = 10;
            this.WPFRadioButton.TabStop = true;
            this.WPFRadioButton.Text = "Windows Presentation Foundation";
            this.WPFRadioButton.UseVisualStyleBackColor = true;
            this.WPFRadioButton.CheckedChanged += new System.EventHandler(this.WPFRadioButton_CheckedChanged);
            // 
            // WinFormsRadioButton
            // 
            this.WinFormsRadioButton.AutoSize = true;
            this.WinFormsRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.WinFormsRadioButton.Location = new System.Drawing.Point(4, 19);
            this.WinFormsRadioButton.Name = "WinFormsRadioButton";
            this.WinFormsRadioButton.Size = new System.Drawing.Size(250, 21);
            this.WinFormsRadioButton.TabIndex = 11;
            this.WinFormsRadioButton.Text = "Windows Forms (More rudimentary)";
            this.WinFormsRadioButton.UseVisualStyleBackColor = true;
            this.WinFormsRadioButton.CheckedChanged += new System.EventHandler(this.WinFormsRadioButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.WinFormsRadioButton);
            this.groupBox1.Controls.Add(this.WPFRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(536, 72);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Choose Graphics Backend";
            // 
            // CustomDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 333);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.nextBtn);
            this.Controls.Add(this.backBtn);
            this.Name = "CustomDialog";
            this.Text = "Graphics Backend Choice";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button cancelBtn;
    private System.Windows.Forms.Button nextBtn;
    private System.Windows.Forms.Button backBtn;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.RadioButton WPFRadioButton;
    private System.Windows.Forms.RadioButton WinFormsRadioButton;
    private System.Windows.Forms.GroupBox groupBox1;
}

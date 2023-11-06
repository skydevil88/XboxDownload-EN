namespace XboxDownload
{
    partial class FormAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            pictureBox1 = new PictureBox();
            label1 = new Label();
            linkLabel2 = new LinkLabel();
            lbVersion = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(40, 22);
            pictureBox1.Margin = new Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(108, 108);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(180, 22);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(274, 30);
            label1.TabIndex = 1;
            label1.Text = "Xbox Download By Devil";
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.Location = new Point(180, 94);
            linkLabel2.Margin = new Padding(4, 0, 4, 0);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(446, 24);
            linkLabel2.TabIndex = 3;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "https://github.com/skydevil88/XboxDownload-EN";
            linkLabel2.LinkClicked += LinkLabel_LinkClicked;
            // 
            // lbVersion
            // 
            lbVersion.AutoSize = true;
            lbVersion.Location = new Point(180, 62);
            lbVersion.Margin = new Padding(4, 0, 4, 0);
            lbVersion.Name = "lbVersion";
            lbVersion.Size = new Size(102, 24);
            lbVersion.TabIndex = 7;
            lbVersion.Text = "Version {0}";
            // 
            // FormAbout
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(644, 164);
            Controls.Add(lbVersion);
            Controls.Add(linkLabel2);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAbout";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private LinkLabel linkLabel2;
        private Label lbVersion;
    }
}
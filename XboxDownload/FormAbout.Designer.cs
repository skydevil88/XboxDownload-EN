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
            label2 = new Label();
            lbVersion = new Label();
            labBTC = new Label();
            labETH = new Label();
            linkCopyBTC = new LinkLabel();
            linkCopyETH = new LinkLabel();
            label5 = new Label();
            label6 = new Label();
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(40, 146);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(219, 30);
            label2.TabIndex = 4;
            label2.Text = "Support the project";
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
            // labBTC
            // 
            labBTC.AutoSize = true;
            labBTC.Location = new Point(89, 185);
            labBTC.Name = "labBTC";
            labBTC.Size = new Size(662, 24);
            labBTC.TabIndex = 8;
            labBTC.Text = "bc1p3ledqqnj582acr9q8md53d79frumnzkdldcax76ac0apyg0vyw3s2tjwm6";
            // 
            // labETH
            // 
            labETH.AutoSize = true;
            labETH.Location = new Point(89, 222);
            labETH.Name = "labETH";
            labETH.Size = new Size(462, 24);
            labETH.TabIndex = 9;
            labETH.Text = "0x3702c8f2d2d73c073ed294b180646b265b05c5c2";
            // 
            // linkCopyBTC
            // 
            linkCopyBTC.AutoSize = true;
            linkCopyBTC.Location = new Point(757, 185);
            linkCopyBTC.Name = "linkCopyBTC";
            linkCopyBTC.Size = new Size(52, 24);
            linkCopyBTC.TabIndex = 10;
            linkCopyBTC.TabStop = true;
            linkCopyBTC.Text = "copy";
            linkCopyBTC.LinkClicked += LinkCopyBTC_LinkClicked;
            // 
            // linkCopyETH
            // 
            linkCopyETH.AutoSize = true;
            linkCopyETH.Location = new Point(557, 222);
            linkCopyETH.Name = "linkCopyETH";
            linkCopyETH.Size = new Size(52, 24);
            linkCopyETH.TabIndex = 11;
            linkCopyETH.TabStop = true;
            linkCopyETH.Text = "copy";
            linkCopyETH.LinkClicked += LinkCopyETH_LinkClicked;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(40, 185);
            label5.Name = "label5";
            label5.Size = new Size(43, 24);
            label5.TabIndex = 12;
            label5.Text = "BTC";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(40, 222);
            label6.Name = "label6";
            label6.Size = new Size(44, 24);
            label6.TabIndex = 13;
            label6.Text = "ETH";
            // 
            // FormAbout
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(827, 269);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(linkCopyETH);
            Controls.Add(linkCopyBTC);
            Controls.Add(labETH);
            Controls.Add(labBTC);
            Controls.Add(lbVersion);
            Controls.Add(label2);
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
        private Label label2;
        private Label lbVersion;
        private Label labBTC;
        private Label labETH;
        private LinkLabel linkCopyBTC;
        private LinkLabel linkCopyETH;
        private Label label5;
        private Label label6;
    }
}
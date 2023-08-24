namespace XboxDownload
{
    partial class FormNSBH
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNSBH));
            label1 = new Label();
            tbNSHomepage = new TextBox();
            butSubmit = new Button();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 26);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(110, 24);
            label1.TabIndex = 0;
            label1.Text = "Home Page";
            // 
            // tbNSHomepage
            // 
            tbNSHomepage.Location = new Point(130, 20);
            tbNSHomepage.Margin = new Padding(4, 4, 4, 4);
            tbNSHomepage.Name = "tbNSHomepage";
            tbNSHomepage.Size = new Size(448, 30);
            tbNSHomepage.TabIndex = 1;
            // 
            // butSubmit
            // 
            butSubmit.Location = new Point(586, 17);
            butSubmit.Margin = new Padding(4, 4, 4, 4);
            butSubmit.Name = "butSubmit";
            butSubmit.Size = new Size(112, 36);
            butSubmit.TabIndex = 2;
            butSubmit.Text = "Save";
            butSubmit.UseVisualStyleBackColor = true;
            butSubmit.Click += ButSubmit_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 65);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(685, 288);
            label2.TabIndex = 3;
            label2.Text = resources.GetString("label2.Text");
            // 
            // FormNSBH
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(727, 370);
            Controls.Add(label2);
            Controls.Add(butSubmit);
            Controls.Add(tbNSHomepage);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormNSBH";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Set NS browser home page";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbNSHomepage;
        private Button butSubmit;
        private Label label2;
    }
}
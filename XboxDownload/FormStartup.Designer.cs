namespace XboxDownload
{
    partial class FormStartup
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
            cbStartup = new CheckBox();
            butSubmit = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // cbStartup
            // 
            cbStartup.AutoSize = true;
            cbStartup.Location = new Point(44, 22);
            cbStartup.Margin = new Padding(4);
            cbStartup.Name = "cbStartup";
            cbStartup.Size = new Size(147, 28);
            cbStartup.TabIndex = 0;
            cbStartup.Text = "Auto Startup";
            cbStartup.UseVisualStyleBackColor = true;
            // 
            // butSubmit
            // 
            butSubmit.Location = new Point(201, 18);
            butSubmit.Margin = new Padding(4);
            butSubmit.Name = "butSubmit";
            butSubmit.Size = new Size(100, 36);
            butSubmit.TabIndex = 1;
            butSubmit.Text = "Save";
            butSubmit.UseVisualStyleBackColor = true;
            butSubmit.Click += ButSubmit_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(44, 63);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(217, 48);
            label1.TabIndex = 2;
            label1.Text = "Start up Listen and \r\nMinimize to system tray";
            // 
            // FormStartup
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(356, 120);
            Controls.Add(label1);
            Controls.Add(butSubmit);
            Controls.Add(cbStartup);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormStartup";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Auto Startup";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox cbStartup;
        private Button butSubmit;
        private Label label1;
    }
}
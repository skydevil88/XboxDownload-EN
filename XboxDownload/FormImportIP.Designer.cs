namespace XboxDownload
{
    partial class FormImportIP
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
            panel2 = new Panel();
            linkLabel4 = new LinkLabel();
            button1 = new Button();
            textBox1 = new TextBox();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.Controls.Add(linkLabel4);
            panel2.Controls.Add(button1);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 542);
            panel2.Margin = new Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new Size(975, 50);
            panel2.TabIndex = 1;
            // 
            // linkLabel4
            // 
            linkLabel4.AutoSize = true;
            linkLabel4.Location = new Point(878, 13);
            linkLabel4.Margin = new Padding(4, 0, 4, 0);
            linkLabel4.Name = "linkLabel4";
            linkLabel4.Size = new Size(93, 24);
            linkLabel4.TabIndex = 2;
            linkLabel4.TabStop = true;
            linkLabel4.Text = "Open File";
            linkLabel4.LinkClicked += LinkLabel4_LinkClicked;
            // 
            // button1
            // 
            button1.Location = new Point(430, 7);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(112, 36);
            button1.TabIndex = 1;
            button1.Text = "Submit";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(0, 0);
            textBox1.Margin = new Padding(4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Akamai\r\n\r\nxxx.xxx.xxx.xxx\t (Location, optional)";
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(975, 542);
            textBox1.TabIndex = 2;
            // 
            // FormImportIP
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(975, 592);
            Controls.Add(textBox1);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormImportIP";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Import Akamai CND IP";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel panel2;
        private LinkLabel linkLabel4;
        private Button button1;
        private TextBox textBox1;
    }
}
﻿using System.Diagnostics;
using System.Text;

namespace XboxDownload
{
    public partial class FormStartup : Form
    {
        public FormStartup()
        {
            InitializeComponent();
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\Tasks\XboxDownload"))
            {
                cbStartup.Checked = true;
            }
        }

        private void ButSubmit_Click(object sender, EventArgs e)
        {
            butSubmit.Enabled = false;
            if (cbStartup.Checked)
            {
                string taskPath = Path.GetTempPath() + "XboxDownloadTask.xml";
                string xml = String.Format(Properties.Resource.Task, Application.ExecutablePath);
                File.WriteAllText(taskPath, xml, Encoding.GetEncoding("UTF-16"));
                string cmdPath = Path.GetTempPath() + "\\XboxDownloadTask.cmd";
                string cmd = "schtasks /create /xml \"" + taskPath + "\" /tn \"XboxDownload\" /f";
                File.WriteAllText(cmdPath, cmd);
                using (Process p = new())
                {
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Arguments = "/c \"" + cmdPath + "\"";
                    p.Start();
                    p.WaitForExit();
                }
                File.Delete(cmdPath);
                File.Delete(taskPath);
            }
            else
            {
                string cmdPath = Path.GetTempPath() + "XboxDownloadTask.cmd";
                string cmd = "schtasks /delete /tn \"XboxDownload\" /f\r\nschtasks /delete /tn \"XboxDownload\" /f";
                File.WriteAllText(cmdPath, cmd);
                using (Process p = new())
                {
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Arguments = "/c \"" + cmdPath + "\"";
                    p.Start();
                    p.WaitForExit();
                }
                File.Delete(cmdPath);
            }
            this.Close();
        }
    }
}

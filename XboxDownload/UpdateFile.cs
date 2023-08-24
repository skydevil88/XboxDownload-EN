using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XboxDownload
{
    internal class UpdateFile
    {
        public const string homePage = "https://xbox.skydevil.xyz";
        public const string updateUrl = "https://github.com/skydevil88/XboxDownload-EN/releases/";

        public static void Start(bool autoupdate, Form1 parentForm)
        {
            Properties.Settings.Default.NextUpdate = DateTime.Now.AddDays(7).Ticks;
            Properties.Settings.Default.Save();

            using HttpResponseMessage? response = ClassWeb.HttpResponseMessage(updateUrl + "latest", "HEAD");
            if (response != null && response.IsSuccessStatusCode)
            {
                string? url = response.RequestMessage?.RequestUri?.ToString();
                if (url != null)
                {
                    bool isUpdate = false;
                    Match result = Regex.Match(url, @"(?<version>\d+(\.\d+){2,3})$");
                    if (result.Success)
                    {
                        Version version1 = new(result.Groups["version"].Value);
                        Version version2 = new((Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version) ?? string.Empty);
                        if (version1 > version2 && version1.Major == 2)
                        {
                            parentForm.Invoke(new Action(() =>
                            {
                                isUpdate = MessageBox.Show("A new version has been detected, update now?", "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes;
                                if (!isUpdate) parentForm.tsmUpdate.Enabled = true;
                            }));
                            if (!isUpdate) return;
                        }
                        else
                        {
                            parentForm.Invoke(new Action(() =>
                            {
                                if (!autoupdate) MessageBox.Show("This software is already the latest version.", "Update", MessageBoxButtons.OK, MessageBoxIcon.None);
                                parentForm.tsmUpdate.Enabled = true;
                            }));
                            return;
                        }
                    }
                    if (isUpdate)
                    {
                        string download = (url.Replace("tag", "download") + "/XboxDownload.zip");
                        using HttpResponseMessage? response2 = ClassWeb.HttpResponseMessage(download, "GET", null, null, null, 180000);
                        if (response2 != null && response2.IsSuccessStatusCode)
                        {
                            if (!Directory.Exists(Form1.resourcePath))
                                Directory.CreateDirectory(Form1.resourcePath);
                            byte[] buffer = response2.Content.ReadAsByteArrayAsync().Result;
                            if (buffer.Length > 0)
                            {
                                using FileStream fs = new(Form1.resourcePath + "\\" + "XboxDownload.zip", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                                fs.Write(buffer, 0, buffer.Length);
                                fs.Flush();
                                fs.Close();
                                string tempDir = Form1.resourcePath + @"\Temp";
                                if (Directory.Exists(tempDir))
                                    Directory.Delete(tempDir, true);
                                ZipFile.ExtractToDirectory(Form1.resourcePath + @"\XboxDownload.zip", tempDir, true);
                                foreach (DirectoryInfo di in new DirectoryInfo(tempDir).GetDirectories())
                                {
                                    if (File.Exists(di.FullName + @"\XboxDownload.exe"))
                                    {
                                        parentForm.Invoke(new Action(() =>
                                        {
                                            if (Form1.bServiceFlag) parentForm.ButStart_Click(null, null);
                                            parentForm.notifyIcon1.Visible = false;
                                        }));
                                        string cmd = "choice /t 3 /d y /n >nul\r\nxcopy \"" + di.FullName + "\" \"" + Path.GetDirectoryName(Application.ExecutablePath) + "\" /s /e /y\r\ndel /a/f/q " + Form1.resourcePath + "\\XboxDownload.zip\r\n\"" + Application.ExecutablePath + "\"\r\nrd /s/q " + tempDir;
                                        File.WriteAllText(tempDir + "\\" + ".update.cmd", cmd);
                                        using (Process p = new())
                                        {
                                            p.StartInfo.FileName = "cmd.exe";
                                            p.StartInfo.UseShellExecute = false;
                                            p.StartInfo.CreateNoWindow = true;
                                            p.StartInfo.Arguments = "/c \"" + tempDir + "\\.update.cmd\"";
                                            p.Start();
                                        }
                                        Process.GetCurrentProcess().Kill();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                parentForm.Invoke(new Action(() =>
                {
                    if (!autoupdate) MessageBox.Show("Error on downloading file. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    parentForm.tsmUpdate.Enabled = true;
                }));
            }
            else if(!autoupdate)
            {
                parentForm.Invoke(new Action(() =>
                {
                    MessageBox.Show("Error on checking for updates. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    parentForm.tsmUpdate.Enabled = true;
                }));
            }
        }
    }
}

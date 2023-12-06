﻿using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XboxDownload
{
    internal class UpdateFile
    {
        public const string homePage = "https://xbox.skydevil.xyz";
        public const string project = "https://github.com/skydevil88/XboxDownload-EN";
        private static readonly string[,] proxys = {
            { "proxy", "https://py.skydevil.xyz/"},
            { "direct", "" }
        };

        public static void Start(bool autoupdate, Form1 parentForm)
        {
            Properties.Settings.Default.NextUpdate = DateTime.Now.AddDays(7).Ticks;
            Properties.Settings.Default.Save();

            string? releases = null;
            Task[] tasks = new Task[proxys.GetLongLength(0)];
            for (int i = 0; i <= tasks.Length - 1; i++)
            {
                string proxy = proxys[i, 0] switch
                {
                    "proxy" => proxys[i, 1],
                    _ => ""
                };
                tasks[i] = new Task(() =>
                {
                    using HttpResponseMessage? response = ClassWeb.HttpResponseMessage(proxy + UpdateFile.project + "/releases/latest", "HEAD", null, null, null, 6000);
                    if (response != null && response.IsSuccessStatusCode && string.IsNullOrEmpty(releases))
                        releases = response.RequestMessage?.RequestUri?.ToString();
                    else
                        Thread.Sleep(6000);
                });
            }
            Array.ForEach(tasks, x => x.Start());
            Task.WaitAny(tasks);
            if (!string.IsNullOrEmpty(releases))
            {
                Match result = Regex.Match(releases, @"(?<version>\d+(\.\d+){2,3})$");
                if (result.Success)
                {
                    Version version1 = new(result.Groups["version"].Value);
                    Version version2 = new((Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version) ?? string.Empty);
                    if (version1 > version2 && version1.Major == 2)
                    {
                        bool isUpdate = false;
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
                            if (!autoupdate) MessageBox.Show("This App is already the latest version.", "Update", MessageBoxButtons.OK, MessageBoxIcon.None);
                            parentForm.tsmUpdate.Enabled = true;
                        }));
                        return;
                    }
                    string download = releases.Replace("tag", "download") + "/XboxDownload.zip";
                    using (HttpResponseMessage? response = ClassWeb.HttpResponseMessage(download, "GET", null, null, null, 60000))
                    {
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            if (!Directory.Exists(Form1.resourcePath))
                                Directory.CreateDirectory(Form1.resourcePath);
                            byte[] buffer = response.Content.ReadAsByteArrayAsync().Result;
                            if (buffer.Length > 0)
                            {
                                using FileStream fs = new(Form1.resourcePath + "\\" + "XboxDownload.zip", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                                fs.Write(buffer, 0, buffer.Length);
                                fs.Flush();
                                fs.Close();
                                string tempDir = Form1.resourcePath + @"\.Temp";
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
                                        string cmd = "chcp 65001\r\nchoice /t 3 /d y /n >nul\r\nxcopy \"" + di.FullName + "\" \"" + Path.GetDirectoryName(Application.ExecutablePath) + "\" /s /e /y\r\ndel /a/f/q " + Form1.resourcePath + "\\XboxDownload.zip\r\n\"" + Application.ExecutablePath + "\"\r\nrd /s/q " + tempDir;
                                        File.WriteAllText(tempDir + "\\update.cmd", cmd);
                                        using (Process p = new())
                                        {
                                            p.StartInfo.FileName = "cmd.exe";
                                            p.StartInfo.UseShellExecute = false;
                                            p.StartInfo.CreateNoWindow = true;
                                            p.StartInfo.Arguments = "/c \"" + tempDir + "\\update.cmd\"";
                                            p.Start();
                                        }
                                        Process.GetCurrentProcess().Kill();
                                        break;
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
            }
            if (!autoupdate)
            {
                parentForm.Invoke(new Action(() =>
                {
                    MessageBox.Show("Error on checking for updates. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    parentForm.tsmUpdate.Enabled = true;
                }));
            }
        }

        public static async Task DownloadIP(FileInfo fi)
        {
            string? fileUrl = null;
            Task[] tasks = new Task[proxys.GetLongLength(0)];
            for (int i = 0; i <= tasks.Length - 1; i++)
            {
                string proxy = proxys[i, 0] switch
                {
                    "proxy" => proxys[i, 1],
                    _ => ""
                };
                tasks[i] = new Task(() =>
                {
                    string tmpUrl = proxy + UpdateFile.project.Replace("github.com", "raw.githubusercontent.com") + "/master/IP/" + fi.Name;
                    using HttpResponseMessage? response = ClassWeb.HttpResponseMessage(tmpUrl, "HEAD", null, null, null, 6000);
                    if (response != null && response.IsSuccessStatusCode && string.IsNullOrEmpty(fileUrl))
                        fileUrl = tmpUrl;
                    else
                        Thread.Sleep(6000);
                });
            }
            Array.ForEach(tasks, x => x.Start());
            await Task.WhenAny(tasks);
            if (!string.IsNullOrEmpty(fileUrl))
            {
                using HttpResponseMessage? response = ClassWeb.HttpResponseMessage(fileUrl, "GET", null, null, null, 60000);
                if (response != null && response.IsSuccessStatusCode)
                {
                    byte[] buffer = response.Content.ReadAsByteArrayAsync().Result;
                    if (buffer.Length > 0)
                    {
                        if (fi.DirectoryName != null && !Directory.Exists(fi.DirectoryName))
                            Directory.CreateDirectory(fi.DirectoryName);
                        using FileStream fs = new(fi.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Flush();
                        fs.Close();
                        fi.Refresh();
                    }
                }
            }
        }
    }
}

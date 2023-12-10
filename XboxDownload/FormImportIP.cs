using System.Data;
using System.Net;
using System.Text.RegularExpressions;

namespace XboxDownload
{
    public partial class FormImportIP : Form
    {
        public String host = string.Empty;
        public DataTable dt;

        public FormImportIP()
        {
            InitializeComponent();

            dt = new DataTable();
            dt.Columns.Add("IP", typeof(string));
            dt.Columns.Add("IpFilter", typeof(string));
            dt.Columns.Add("Location", typeof(string));
            dt.Columns.Add("IpLong", typeof(ulong));
            var col = dt.Columns["IpFilter"];
            if (col != null) dt.PrimaryKey = new DataColumn[] { col };
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string content = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            string[] array = content.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 1)
            {
                if (String.Equals(array[0].Trim(), "Akamai", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.host = "Akamai";
                }
                else
                {
                    foreach (string str in array)
                    {
                        string tmp = str.Trim();
                        if (Regex.IsMatch(tmp, @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$"))
                        {
                            this.host = tmp.ToLowerInvariant();
                            switch (this.host)
                            {
                                case "atum.hac.lp1.d4c.nintendo.net":
                                    this.host = "Akamai";
                                    break;
                                default:
                                    if (Regex.IsMatch(this.host, @"\.akamaihd\.net$"))
                                    {
                                        this.host = "Akamai";
                                    }
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(this.host))
            {
                MessageBox.Show("Format does not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Match result = Regex.Match(content, @"(?<IP>\d{0,3}\.\d{0,3}\.\d{0,3}\.\d{0,3})\s*\((?<Location>[^\)]*)\)|(?<IP>\d{0,3}\.\d{0,3}\.\d{0,3}\.\d{0,3})(?<Location>[^\)]*)\d+ms|^\s*(?<IP>\d{0,3}\.\d{0,3}\.\d{0,3}\.\d{0,3})\s*$", RegexOptions.Multiline);
            while (result.Success)
            {
                string ip = result.Groups["IP"].Value;
                UInt64 ipLong = IpToLong(ip);
                if (ipLong == 0) return;
                string IpFilter = Regex.Replace(ip, @"\d{0,3}$", "");
                DataRow? dr = dt.Rows.Find(IpFilter);
                if (dr == null)
                {
                    dr = dt.NewRow();
                    dr["IP"] = ip;
                    dr["IpFilter"] = IpFilter;
                    dr["Location"] = result.Groups["Location"].Value.Trim();
                    dr["IpLong"] = ipLong;
                    dt.Rows.Add(dr);
                }
                result = result.NextMatch();
            }
            this.Close();
        }

        private void LinkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "text file(*.txt)|*.txt",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new(openFileDialog.FileName);
                textBox1.Text = sr.ReadToEnd();
                sr.Close();
            }
        }

        private static ulong IpToLong(string ip)
        {
            ulong IntIp = 0;
            if (IPAddress.TryParse(ip, out IPAddress? ipaddress))
            {
                string[] ips = ipaddress.ToString().Split('.');
                IntIp = ulong.Parse(ips[0]) << 0x18 | ulong.Parse(ips[1]) << 0x10 | ulong.Parse(ips[2]) << 0x8 | ulong.Parse(ips[3]);
            }
            return IntIp;
        }
    }
}

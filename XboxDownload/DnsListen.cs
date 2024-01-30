using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Net.NetworkInformation;
using System.Data;

namespace XboxDownload
{
    internal class DnsListen
    {
        Socket? socket = null;
        public static string dohServer = string.Empty;
        private readonly Form1 parentForm;
        private readonly Regex reDoHFilter = new("google|youtube|facebook|twitter");
        public static Regex reHosts = new(@"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$");
        public static ConcurrentDictionary<String, List<ResouceRecord>> dicService = new(), dicHosts1 = new();
        public static ConcurrentDictionary<Regex, List<ResouceRecord>> dicHosts2 = new();
        public static ConcurrentDictionary<String, Dns> dicDns = new();

        public class Dns
        {
            public string IPv4 { get; set; } = "";

            public string IPv6 { get; set; } = "";
        }

        public DnsListen(Form1 parentForm)
        {
            this.parentForm = parentForm;
            dohServer = Thread.CurrentThread.CurrentCulture.Name != "zh-CN" ? "https://8.8.8.8" : "https://223.5.5.5";
        }

        public void Listen()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces().Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback && (x.NetworkInterfaceType == NetworkInterfaceType.Ethernet || x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && !x.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (Properties.Settings.Default.SetDns)
            {
                dicDns.Clear();
                using var key = Microsoft.Win32.Registry.LocalMachine;
                foreach (NetworkInterface adapter in adapters)
                {
                    var dns = new Dns();
                    var rk1 = key.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + adapter.Id);
                    if (rk1 != null)
                    {
                        string? ip = rk1.GetValue("NameServer", null) as string;
                        if (string.IsNullOrEmpty(ip) || ip == Properties.Settings.Default.LocalIP) ip = "";
                        dns.IPv4 = ip;
                        rk1.Close();
                    }
                    var rk2 = key.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters\Interfaces\" + adapter.Id);
                    if (rk2 != null)
                    {
                        string? ip = rk2.GetValue("NameServer", null) as string;
                        if (string.IsNullOrEmpty(ip) || ip == "::") ip = "";
                        dns.IPv6 = ip;
                        rk2.Close();
                    }
                    dicDns.TryAdd(adapter.Id, dns);
                }
            }
            int port = 53;
            IPEndPoint? iPEndPoint = null;
            if (string.IsNullOrEmpty(Properties.Settings.Default.DnsIP))
            {
                foreach (NetworkInterface adapter in adapters)
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    foreach (IPAddress dns in adapterProperties.DnsAddresses)
                    {
                        if (dns.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (dns.ToString() == Properties.Settings.Default.LocalIP || IPAddress.IsLoopback(dns))
                                continue;
                            iPEndPoint = new IPEndPoint(dns, port);
                            break;
                        }
                    }
                    if (iPEndPoint != null) break;
                }
                iPEndPoint ??= new IPEndPoint(IPAddress.Parse("8.8.8.8"), port);
                if (Form1.bServiceFlag)
                    parentForm.SetTextBox(parentForm.tbDnsIP, iPEndPoint.Address.ToString());
            }
            else
            {
                iPEndPoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.DnsIP), port);
            }
            if (!Form1.bServiceFlag) return;

            IPEndPoint ipe = new(Properties.Settings.Default.ListenIP == 0 ? IPAddress.Parse(Properties.Settings.Default.LocalIP) : IPAddress.Any, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(ipe);
            }
            catch (SocketException ex)
            {
                parentForm.Invoke(new Action(() =>
                {
                    parentForm.pictureBox1.Image = Properties.Resource.Xbox3;
                    MessageBox.Show($"Enabling DNS service Failure! \nmessage: {ex.Message}\n\nSolution: 1.Disable services that occupy the {port} port. 2.Listening IP selection (Any)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return;
            }

            Byte[] localIP = IPAddress.Parse(Properties.Settings.Default.LocalIP).GetAddressBytes();
            Byte[]? gameIP = null, appIP = null, psIP = null, nsIP = null, eaIP = null, battleIP = null;
            Task[] tasks = new Task[6];
            tasks[0] = new Task(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.GameIP))
                {
                    gameIP = IPAddress.Parse(Properties.Settings.Default.GameIP).GetAddressBytes();
                }
                else
                {
                    string? ip = Properties.Settings.Default.DoH ? ClassDNS.DoH("xvcf1.xboxlive.com") : ClassDNS.HostToIP("xvcf2.xboxlive.com", Properties.Settings.Default.DnsIP);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Form1.bServiceFlag) parentForm.SetTextBox(parentForm.tbGameIP, ip);
                        gameIP = IPAddress.Parse(ip).GetAddressBytes();
                    }
                }
            });
            tasks[1] = new Task(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.AppIP))
                {
                    appIP = IPAddress.Parse(Properties.Settings.Default.AppIP).GetAddressBytes();
                }
                else
                {
                    string? ip = Properties.Settings.Default.DoH ? ClassDNS.DoH("dl.delivery.mp.microsoft.com") : ClassDNS.HostToIP("dl.delivery.mp.microsoft.com", Properties.Settings.Default.DnsIP);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Form1.bServiceFlag) parentForm.SetTextBox(parentForm.tbAppIP, ip);
                        appIP = IPAddress.Parse(ip).GetAddressBytes();
                    }
                }
            });
            tasks[2] = new Task(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.PSIP))
                {
                    psIP = IPAddress.Parse(Properties.Settings.Default.PSIP).GetAddressBytes();
                }
                else
                {
                    string? ip = Properties.Settings.Default.DoH ? ClassDNS.DoH("gst.prod.dl.playstation.net") : ClassDNS.HostToIP("gst.prod.dl.playstation.net", Properties.Settings.Default.DnsIP);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Form1.bServiceFlag) parentForm.SetTextBox(parentForm.tbPSIP, ip);
                        psIP = IPAddress.Parse(ip).GetAddressBytes();
                    }
                }
            });
            tasks[3] = new Task(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.NSIP))
                {
                    nsIP = IPAddress.Parse(Properties.Settings.Default.NSIP).GetAddressBytes();
                }
                else
                {
                    string? ip = Properties.Settings.Default.DoH ? ClassDNS.DoH("atum.hac.lp1.d4c.nintendo.net") : ClassDNS.HostToIP("atum.hac.lp1.d4c.nintendo.net", Properties.Settings.Default.DnsIP);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Form1.bServiceFlag) parentForm.SetTextBox(parentForm.tbNSIP, ip);
                        nsIP = IPAddress.Parse(ip).GetAddressBytes();
                    }
                }
            });
            tasks[4] = new Task(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.EAIP))
                {
                    eaIP = IPAddress.Parse(Properties.Settings.Default.EAIP).GetAddressBytes();
                }
                else
                {
                    string? ip = Properties.Settings.Default.DoH ? ClassDNS.DoH("origin-a.akamaihd.net") : ClassDNS.HostToIP("origin-a.akamaihd.net", Properties.Settings.Default.DnsIP);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Form1.bServiceFlag) parentForm.SetTextBox(parentForm.tbEAIP, ip);
                        eaIP = IPAddress.Parse(ip).GetAddressBytes();
                    }
                }
            });
            tasks[5] = new Task(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.BattleIP))
                {
                    battleIP = IPAddress.Parse(Properties.Settings.Default.BattleIP).GetAddressBytes();
                }
                else
                {
                    string? ip = Properties.Settings.Default.DoH ? ClassDNS.DoH("blzddist1-a.akamaihd.net") : ClassDNS.HostToIP("blzddist1-a.akamaihd.net", Properties.Settings.Default.DnsIP);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Form1.bServiceFlag) parentForm.SetTextBox(parentForm.tbBattleIP, ip);
                        battleIP = IPAddress.Parse(ip).GetAddressBytes();
                    }
                }
            });
            Array.ForEach(tasks, x => x.Start());
            Task.WaitAll(tasks);
            if (!Form1.bServiceFlag) return;

            dicService.Clear();
            List<ResouceRecord> lsLocalIP = new() { new ResouceRecord { Datas = localIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
            if (Properties.Settings.Default.GameLink)
            {
                dicService.TryAdd("xvcf1.xboxlive.com", lsLocalIP);
                dicService.TryAdd("assets1.xboxlive.com", lsLocalIP);
                dicService.TryAdd("d1.xboxlive.com", lsLocalIP);
                dicService.TryAdd("dlassets.xboxlive.com", lsLocalIP);
                dicService.TryAdd("assets1.xboxlive.cn", lsLocalIP);
                dicService.TryAdd("d1.xboxlive.cn", lsLocalIP);
                dicService.TryAdd("dlassets.xboxlive.cn", lsLocalIP);
                if (gameIP != null)
                {
                    List<ResouceRecord> lsGameIP = new() { new ResouceRecord { Datas = gameIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                    dicService.TryAdd("xvcf2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("assets2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("d2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("dlassets2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("assets2.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("d2.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("dlassets2.xboxlive.cn", lsGameIP);
                }
                if (appIP != null)
                {
                    List<ResouceRecord> lsAppIP = new() { new ResouceRecord { Datas = appIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                    dicService.TryAdd("dl.delivery.mp.microsoft.com", lsAppIP);
                    dicService.TryAdd("tlu.dl.delivery.mp.microsoft.com", lsLocalIP);
                    dicService.TryAdd("2.tlu.dl.delivery.mp.microsoft.com", lsAppIP);
                }
            }
            else
            {
                if (gameIP != null)
                {
                    List<ResouceRecord> lsGameIP = new() { new ResouceRecord { Datas = gameIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                    dicService.TryAdd("xvcf1.xboxlive.com", lsGameIP);
                    dicService.TryAdd("xvcf2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("assets1.xboxlive.com", lsGameIP);
                    dicService.TryAdd("assets2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("d1.xboxlive.com", lsGameIP);
                    dicService.TryAdd("d2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("dlassets.xboxlive.com", lsGameIP);
                    dicService.TryAdd("dlassets2.xboxlive.com", lsGameIP);
                    dicService.TryAdd("assets1.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("assets2.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("d1.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("d2.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("dlassets.xboxlive.cn", lsGameIP);
                    dicService.TryAdd("dlassets2.xboxlive.cn", lsGameIP);
                }
                if (appIP != null)
                {
                    List<ResouceRecord> lsAppIP = new() { new ResouceRecord { Datas = appIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                    dicService.TryAdd("dl.delivery.mp.microsoft.com", lsAppIP);
                    dicService.TryAdd("tlu.dl.delivery.mp.microsoft.com", lsAppIP);
                    dicService.TryAdd("2.tlu.dl.delivery.mp.microsoft.com", lsAppIP);
                }
            }
            if (psIP != null)
            {
                List<ResouceRecord> lsPsIP = new() { new ResouceRecord { Datas = psIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                dicService.TryAdd("gst.prod.dl.playstation.net", lsPsIP);
                dicService.TryAdd("gs2.ww.prod.dl.playstation.net", lsPsIP);
                dicService.TryAdd("zeus.dl.playstation.net", lsPsIP);
                dicService.TryAdd("ares.dl.playstation.net", lsPsIP);
            }
            if (nsIP != null)
            {
                List<ResouceRecord> lsNsIP = new() { new ResouceRecord { Datas = nsIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                dicService.TryAdd("atum.hac.lp1.d4c.nintendo.net", lsNsIP);
                dicService.TryAdd("bugyo.hac.lp1.eshop.nintendo.net", lsNsIP);
                dicService.TryAdd("ctest-dl-lp1.cdn.nintendo.net", lsNsIP);
                dicService.TryAdd("ctest-ul-lp1.cdn.nintendo.net", lsNsIP);
            }
            dicService.TryAdd("atum-eda.hac.lp1.d4c.nintendo.net", new List<ResouceRecord>());
            if (eaIP != null)
            {
                List<ResouceRecord> lsEaIP = new() { new ResouceRecord { Datas = eaIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                dicService.TryAdd("origin-a.akamaihd.net", lsEaIP);
            }
            dicService.TryAdd("ssl-lvlt.cdn.ea.com", new List<ResouceRecord>());
            if (battleIP != null)
            {
                List<ResouceRecord> lsBattleIP = new() { new ResouceRecord { Datas = battleIP, TTL = 100, QueryClass = 1, QueryType = QueryType.A } };
                dicService.TryAdd("blzddist1-a.akamaihd.net", lsBattleIP);
                dicService.TryAdd("blzddist2-a.akamaihd.net", lsBattleIP);
                dicService.TryAdd("blzddist3-a.akamaihd.net", lsBattleIP);
            }
            if (Properties.Settings.Default.HttpService)
            {
                dicService.TryAdd("www.msftconnecttest.com", lsLocalIP);
                dicService.TryAdd("ctest.cdn.nintendo.net", lsLocalIP);
            }
            if (Properties.Settings.Default.SetDns) ClassDNS.SetDns(Properties.Settings.Default.LocalIP);
            while (Form1.bServiceFlag)
            {
                try
                {
                    var client = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                    var buff = new byte[512];
                    int read = socket.ReceiveFrom(buff, ref client);
                    Task.Factory.StartNew(() =>
                    {
                        var dns = new DNS(buff, read);
                        if (dns.QR == 0 && dns.Opcode == 0 && dns.Querys.Count == 1)
                        {
                            string queryName = (dns.Querys[0].QueryName ?? string.Empty).ToLower();
                            switch (dns.Querys[0].QueryType)
                            {
                                case QueryType.A:
                                    if (dicService.TryGetValue(queryName, out List<ResouceRecord>? lsServiceIp))
                                    {
                                        dns.QR = 1;
                                        dns.RA = 1;
                                        dns.RD = 1;
                                        dns.ResouceRecords = lsServiceIp;
                                        socket?.SendTo(dns.ToBytes(), client);
                                        if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("DNS Query", queryName + " -> " + string.Join(", ", lsServiceIp.Select(a => new IPAddress(a.Datas ?? Array.Empty<byte>()).ToString()).ToArray()), ((IPEndPoint)client).Address.ToString(), 0x008000);
                                        return;
                                    }
                                    if (dicHosts1.TryGetValue(queryName, out List<ResouceRecord>? lsHostsIp))
                                    {
                                        if (lsHostsIp.Count >= 2) lsHostsIp = lsHostsIp.OrderBy(a => Guid.NewGuid()).Take(16).ToList();
                                        dns.QR = 1;
                                        dns.RA = 1;
                                        dns.RD = 1;
                                        dns.ResouceRecords = lsHostsIp;
                                        socket?.SendTo(dns.ToBytes(), client);
                                        if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("DNS Query", queryName + " -> " + string.Join(", ", lsHostsIp.Select(a => new IPAddress(a.Datas ?? Array.Empty<byte>()).ToString()).ToArray()), ((IPEndPoint)client).Address.ToString(), 0x0000FF);
                                        return;
                                    }
                                    var lsHostsIp2 = dicHosts2.Where(kvp => kvp.Key.IsMatch(queryName)).Select(x => x.Value).FirstOrDefault();
                                    if (lsHostsIp2 != null)
                                    {
                                        dicHosts1.TryAdd(queryName, lsHostsIp2);
                                        if (lsHostsIp2.Count >= 2) lsHostsIp2 = lsHostsIp2.OrderBy(a => Guid.NewGuid()).Take(16).ToList();
                                        dns.QR = 1;
                                        dns.RA = 1;
                                        dns.RD = 1;
                                        dns.ResouceRecords = lsHostsIp2;
                                        socket?.SendTo(dns.ToBytes(), client);
                                        if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("DNS Query", queryName + " -> " + string.Join(", ", lsHostsIp2.Select(a => new IPAddress(a.Datas ?? Array.Empty<byte>()).ToString()).ToArray()), ((IPEndPoint)client).Address.ToString(), 0x0000FF);
                                        return;
                                    }
                                    if (Properties.Settings.Default.DoH && !reDoHFilter.IsMatch(queryName))
                                    {
                                        string html = ClassWeb.HttpResponseContent(dohServer + "/resolve?name=" + ClassWeb.UrlEncode(queryName) + "&type=A", "GET", null, null, null, 6000);
                                        if (Regex.IsMatch(html.Trim(), @"^{.+}$"))
                                        {
                                            ClassDNS.Api? json = null;
                                            try
                                            {
                                                json = JsonSerializer.Deserialize<ClassDNS.Api>(html, Form1.jsOptions);
                                            }
                                            catch { }
                                            if (json != null)
                                            {
                                                dns.QR = 1;
                                                dns.RA = 1;
                                                dns.RD = 1;
                                                dns.ResouceRecords = new List<ResouceRecord>();
                                                if (json.Status == 0 && json.Answer != null)
                                                {
                                                    foreach (var answer in json.Answer)
                                                    {
                                                        if (answer.Type == 1 && IPAddress.TryParse(answer.Data, out IPAddress? ipAddress) && ipAddress.AddressFamily == AddressFamily.InterNetwork)
                                                        {
                                                            dns.ResouceRecords.Add(new ResouceRecord
                                                            {
                                                                Datas = ipAddress.GetAddressBytes(),
                                                                TTL = answer.TTL,
                                                                QueryClass = 1,
                                                                QueryType = QueryType.A
                                                            });
                                                        }
                                                    }
                                                }
                                                socket?.SendTo(dns.ToBytes(), client);
                                                if (Properties.Settings.Default.RecordLog && dns.ResouceRecords.Count >= 1) parentForm.SaveLog("DNSv4 查询", queryName + " -> " + string.Join(", ", json.Answer!.Where(x => x.Type == 1).Select(x => x.Data)), ((IPEndPoint)client).Address.ToString());
                                                return;
                                            }
                                        }
                                    }
                                    if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("DNS Query", queryName, ((IPEndPoint)client).Address.ToString());
                                    break;
                                case QueryType.AAAA:
                                    dns.QR = 1;
                                    dns.RA = 1;
                                    dns.RD = 1;
                                    dns.ResouceRecords = new List<ResouceRecord>();
                                    socket?.SendTo(dns.ToBytes(), client);
                                    return;
                            }
                        }
                        try
                        {
                            var proxy = new UdpClient();
                            proxy.Client.ReceiveTimeout = 6000;
                            proxy.Connect(iPEndPoint);
                            proxy.Send(buff, read);
                            var bytes = proxy.Receive(ref iPEndPoint);
                            socket?.SendTo(bytes, client);
                        }
                        catch (Exception ex)
                        {
                            if (Form1.bServiceFlag && Properties.Settings.Default.RecordLog) parentForm.SaveLog("DNS Query", ex.Message, ((IPEndPoint)client).Address.ToString());
                        }
                    });
                }
                catch { }
            }
        }

        public void Close()
        {
            socket?.Close();
            socket?.Dispose();
            socket = null;
        }

        public static void UpdateHosts()
        {
            dicHosts1.Clear();
            dicHosts2.Clear();
            DataTable dt = Form1.dtHosts.Copy();
            dt.RejectChanges();
            foreach (DataRow dr in dt.Rows)
            {
                if (!Convert.ToBoolean(dr["Enable"])) continue;
                string? hostName = dr["HostName"].ToString()?.Trim().ToLower();
                if (!string.IsNullOrEmpty(hostName) && IPAddress.TryParse(dr["IPv4"].ToString()?.Trim(), out IPAddress? ip))
                {
                    if (hostName.StartsWith("*."))
                    {
                        hostName = Regex.Replace(hostName, @"^\*\.", "");
                        Regex re = new("\\." + hostName.Replace(".", "\\.") + "$");
                        if (!dicHosts2.ContainsKey(re) && reHosts.IsMatch(hostName))
                        {
                            List<ResouceRecord> lsIp = new()
                            {
                                new ResouceRecord
                                {
                                    Datas = ip.GetAddressBytes(),
                                    TTL = 100,
                                    QueryClass = 1,
                                    QueryType = QueryType.A
                                }
                            };
                            dicHosts2.TryAdd(re, lsIp);
                        }
                    }
                    else if (!dicHosts1.ContainsKey(hostName) && reHosts.IsMatch(hostName))
                    {
                        List<ResouceRecord> lsIp = new()
                        {
                            new ResouceRecord
                            {
                                Datas = ip.GetAddressBytes(),
                                TTL = 100,
                                QueryClass = 1,
                                QueryType = QueryType.A
                            }
                        };
                        dicHosts1.TryAdd(hostName, lsIp);
                    }
                }
            }

            List<string> lsIpTmp = new();
            List<ResouceRecord> lsIp2 = new();
            foreach (string str in Properties.Settings.Default.IpsAkamai.Replace("，", ",").Split(','))
            {
                if (IPAddress.TryParse(str.Trim(), out IPAddress? address))
                {
                    string ip = address.ToString();
                    if (!lsIpTmp.Contains(ip))
                    {
                        lsIpTmp.Add(ip);
                        lsIp2.Add(new ResouceRecord
                        {
                            Datas = address.GetAddressBytes(),
                            TTL = 100,
                            QueryClass = 1,
                            QueryType = QueryType.A
                        });
                    }
                }
            }
            if (lsIp2.Count >= 1)
            {
                foreach (string str in Properties.Resource.Akamai.Split('\n'))
                {
                    string host = Regex.Replace(str, @"\#.+", "").Trim().ToLower();
                    if (string.IsNullOrEmpty(host)) continue;
                    if (host.StartsWith("*."))
                    {
                        host = Regex.Replace(host, @"^\*\.", "");
                        if (reHosts.IsMatch(host))
                        {
                            dicHosts2.TryAdd(new Regex("\\." + host.Replace(".", "\\.") + "$"), lsIp2);
                        }
                    }
                    else if (reHosts.IsMatch(host))
                    {
                        dicHosts1.TryAdd(host, lsIp2);
                    }
                }
                if (File.Exists(Form1.resourcePath + "\\Akamai.txt"))
                {
                    foreach (string str in File.ReadAllText(Form1.resourcePath + "\\Akamai.txt").Split('\n'))
                    {
                        string host = Regex.Replace(str, @"\#.+", "").Trim().ToLower();
                        if (string.IsNullOrEmpty(host)) continue;
                        if (host.StartsWith("*."))
                        {
                            host = Regex.Replace(host, @"^\*\.", "");
                            if (reHosts.IsMatch(host))
                            {
                                dicHosts2.TryAdd(new Regex("\\." + host.Replace(".", "\\.") + "$"), lsIp2);
                            }
                        }
                        else if (host.StartsWith("*"))
                        {
                            host = Regex.Replace(host, @"^\*", "");
                            if (reHosts.IsMatch(host))
                            {
                                dicHosts1.TryAdd(host, lsIp2);
                                dicHosts2.TryAdd(new Regex("\\." + host.Replace(".", "\\.") + "$"), lsIp2);
                            }
                        }
                        else if (reHosts.IsMatch(host))
                        {
                            dicHosts1.TryAdd(host, lsIp2);
                        }
                    }
                }
            }
        }

        public static void ClearDnsCache()
        {
            using Process p = new();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine("ipconfig /flushdns");
            p.StandardInput.WriteLine("exit");
            p.StandardInput.Close();
        }
    }

    public enum QueryType
    {
        A = 1,
        NS = 2,
        MD = 3,
        MF = 4,
        CNAME = 5,
        SOA = 6,
        MB = 7,
        MG = 8,
        MR = 9,
        WKS = 11,
        PTR = 12,
        HINFO = 13,
        MINFO = 14,
        MX = 15,
        TXT = 16,
        AAAA = 28,
        HTTPS = 65,
        AXFR = 252,
        ANY = 255
    }

    public class Query
    {
        public string? QueryName { get; set; }
        public QueryType QueryType { get; set; }
        public Int16 QueryClass { get; set; }

        public Query()
        {
        }

        public Query(Func<int, byte[]> read)
        {
            var name = new StringBuilder();
            var length = read(1)[0];
            while (length != 0)
            {
                for (var i = 0; i < length; i++)
                {
                    name.Append((char)read(1)[0]);
                }
                length = read(1)[0];
                if (length != 0)
                    name.Append('.');
            }
            QueryName = name.ToString();

            QueryType = (QueryType)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(read(2), 0));
            QueryClass = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(read(2), 0));
        }

        public virtual byte[] ToBytes()
        {
            var list = new List<byte>();

            if (QueryName != null)
            {
                var a = QueryName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < a.Length; i++)
                {
                    list.Add((byte)a[i].Length);
                    for (var j = 0; j < a[i].Length; j++)
                        list.Add((byte)a[i][j]);
                }
                list.Add(0);
            }

            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int16)QueryType)));
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(QueryClass)));

            return list.ToArray();
        }
    }

    public class ResouceRecord : Query
    {
        public Int16 Point { get; set; }
        public Int32 TTL { get; set; }
        public byte[]? Datas { get; set; }

        public ResouceRecord() : base()
        {
            var bytes = new byte[] { 0xc0, 0x0c };
            Point = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
        }

        public ResouceRecord(Func<int, byte[]> read) : base()
        {
            TTL = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(read(4), 0));
            var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(read(2), 0));
            Datas = read(length);
        }
        public override byte[] ToBytes()
        {
            var list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Point)));
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int16)QueryType)));
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(QueryClass)));
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(TTL)));
            if (Datas != null)
            {
                list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int16)Datas.Length)));
                list.AddRange(Datas);
            }

            return list.ToArray();
        }
    }

    public class DNS
    {
        public Int16 Host { get; set; }
        public int QR { get; set; }
        public int Opcode { get; set; }
        public int AA { get; set; }
        public int TC { get; set; }
        public int RD { get; set; }
        public int RA { get; set; }
        public int Rcode { get; set; }

        public List<Query> Querys { get; set; }
        public List<ResouceRecord>? ResouceRecords { get; set; }
        public Int16 AuthorizedResource { get; set; }
        public Int16 ResourceRecord { get; set; }

        public byte[] ToBytes()
        {
            var list = new List<byte>();
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Host));
            list.AddRange(bytes);
            var b = new byte();
            b = b.SetBits(QR, 0, 1)
                .SetBits(Opcode, 1, 4)
                .SetBits(AA, 5, 1)
                .SetBits(TC, 6, 1);

            b = b.SetBits(RD, 7, 1);
            list.Add(b);
            b = new byte();
            b = b.SetBits(RA, 0, 1)
                .SetBits(0, 1, 3)
                .SetBits(Rcode, 4, 4);
            list.Add(b);

            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int16)Querys.Count)));
            if (ResouceRecords != null)
                list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int16)ResouceRecords.Count)));
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(AuthorizedResource)));
            list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ResourceRecord)));

            foreach (var q in Querys)
            {
                list.AddRange(q.ToBytes());
            }
            if (ResouceRecords != null)
            {
                foreach (var r in ResouceRecords)
                {
                    list.AddRange(r.ToBytes());
                }
            }

            return list.ToArray();
        }

        private int index;
        private readonly byte[] package;
        private byte ReadByte()
        {
            return package[index++];
        }
        private byte[] ReadBytes(int count = 1)
        {
            var bytes = new byte[count];
            for (var i = 0; i < count; i++)
                bytes[i] = ReadByte();
            return bytes;
        }

        public DNS(byte[] buffer, int length)
        {
            package = new byte[length];
            for (var i = 0; i < length; i++)
                package[i] = buffer[i];

            Host = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ReadBytes(2), 0));

            var b1 = ReadByte();
            var b2 = ReadByte();

            QR = b1.GetBits(0, 1);
            Opcode = b1.GetBits(1, 4);
            AA = b1.GetBits(5, 1);
            TC = b1.GetBits(6, 1);
            RD = b1.GetBits(7, 1);

            RA = b2.GetBits(0, 1);
            Rcode = b2.GetBits(4, 4);

            var queryCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ReadBytes(2), 0));
            var rrCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ReadBytes(2), 0));

            AuthorizedResource = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ReadBytes(2), 0));
            ResourceRecord = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ReadBytes(2), 0));

            Querys = new List<Query>();
            for (var i = 0; i < queryCount; i++)
            {
                Querys.Add(new Query(ReadBytes));
            }

            for (var i = 0; i < rrCount; i++)
            {
                ResouceRecords?.Add(new ResouceRecord(ReadBytes));
            }
        }
    }

    public static class Extension
    {
        public static int GetBits(this byte b, int start, int length)
        {
            var temp = b >> (8 - start - length);
            var mask = 0;
            for (var i = 0; i < length; i++)
            {
                mask = (mask << 1) + 1;
            }

            return temp & mask;
        }

        public static byte SetBits(this byte b, int data, int start, int length)
        {
            var temp = b;

            var mask = 0xFF;
            for (var i = 0; i < length; i++)
            {
                mask -= (0x01 << (7 - (start + i)));
            }
            temp = (byte)(temp & mask);

            mask = ((byte)data).GetBits(8 - length, length);
            mask <<= (7 - start);

            return (byte)(temp | mask);
        }
    }

    internal class ClassDNS
    {
        public static void SetDns(string? dns = null)
        {
            using var key = Microsoft.Win32.Registry.LocalMachine;
            foreach (var item in DnsListen.dicDns)
            {
                var rk1 = key.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + item.Key);
                if (rk1 != null)
                {
                    rk1.SetValue("NameServer", string.IsNullOrEmpty(dns) ? item.Value.IPv4 : dns);
                    rk1.Close();
                }
                var rk2 = key.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters\Interfaces\" + item.Key);
                if (rk2 != null)
                {
                    rk2.SetValue("NameServer", string.IsNullOrEmpty(dns) ? item.Value.IPv6 : "::");
                    rk2.Close();
                }
            }
        }

        public static string? HostToIP(string hostName, string? dnsServer = null)
        {
            string? ip = null;
            if (string.IsNullOrEmpty(dnsServer))
            {
                IPAddress[]? ipAddresses = null;
                try
                {
                    ipAddresses = Array.FindAll(Dns.GetHostEntry(hostName).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                }
                catch { }
                if (ipAddresses != null && ipAddresses.Length >= 1) ip = ipAddresses[0].ToString();
            }
            else
            {
                string resultInfo = string.Empty;
                using (Process p = new())
                {
                    p.StartInfo = new ProcessStartInfo("nslookup", "-ty=A " + hostName + " " + dnsServer)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true
                    };
                    p.Start();
                    resultInfo = p.StandardOutput.ReadToEnd();
                    p.Close();
                }
                MatchCollection mc = Regex.Matches(resultInfo, @":\s*(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})");
                if (mc.Count == 2)
                    ip = mc[1].Groups["ip"].Value;
            }
            return ip;
        }

        public static string? DoH(string hostName)
        {
            string? ip = null;
            string html = ClassWeb.HttpResponseContent(DnsListen.dohServer + "/resolve?name=" + ClassWeb.UrlEncode(hostName) + "&type=A", "GET", null, null, null, 6000);
            if (Regex.IsMatch(html.Trim(), @"^{.+}$"))
            {
                try
                {
                    var json = JsonSerializer.Deserialize<ClassDNS.Api>(html, Form1.jsOptions);
                    if (json != null && json.Answer != null)
                    {
                        if (json.Status == 0 && json.Answer.Count >= 1)
                        {
                            ip = json.Answer.Where(x => x.Type == 1).Select(x => x.Data).FirstOrDefault();
                        }
                    }
                }
                catch { }
            }
            return ip;
        }

        public class Api
        {
            public int Status { get; set; }
            public bool TC { get; set; }
            public bool RD { get; set; }
            public bool RA { get; set; }
            public bool AD { get; set; }
            public bool CD { get; set; }
            public class Question
            {
                public string? Name { get; set; }
                public int Type { get; set; }
            }
            public List<Answer>? Answer { get; set; }
            public List<Answer>? Authority { get; set; }
            public List<Answer>? Additional { get; set; }
            public string? Edns_client_subnet { get; set; }
        }

        public class Answer
        {
            public string? Name { get; set; }
            public int TTL { get; set; }
            public int Type { get; set; }
            public string? Data { get; set; }
        }
    }
}

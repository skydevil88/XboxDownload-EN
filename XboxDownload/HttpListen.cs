﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System;

namespace XboxDownload
{
    internal class HttpListen
    {
        private readonly Form1 parentForm;
        private readonly ConcurrentDictionary<String, String> dicAppLocalUploadFile = new();
        Socket? socket = null;

        public HttpListen(Form1 parentForm)
        {
            this.parentForm = parentForm;
        }

        public void Listen()
        {
            int port = 80;
            IPEndPoint ipe = new(Properties.Settings.Default.ListenIP == 0 ? IPAddress.Parse(Properties.Settings.Default.LocalIP) : IPAddress.Any, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(ipe);
                socket.Listen(100);
            }
            catch (SocketException ex)
            {
                parentForm.Invoke(new Action(() =>
                {
                    parentForm.pictureBox1.Image = Properties.Resource.Xbox3;
                    MessageBox.Show($"Enabling HTTP Service Failure! \nmessage: {ex.Message}\n\nSolution: 1.Disable services that occupy the {port} port. 2.Listening IP selection (Any)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return;
            }
            while (Form1.bServiceFlag)
            {
                try
                {
                    Socket mySocket = socket.Accept();
                    ThreadPool.QueueUserWorkItem(delegate { TcpThread(mySocket); });
                }
                catch { }
            }
        }

        private void TcpThread(Socket mySocket)
        {
            if (mySocket.Connected)
            {
                mySocket.SendTimeout = 30000;
                mySocket.ReceiveTimeout = 30000;
                while (Form1.bServiceFlag && mySocket.Connected && mySocket.Poll(3000000, SelectMode.SelectRead))
                {
                    Byte[] _receive = new Byte[4096];
                    int _num = mySocket.Receive(_receive, 0, _receive.Length, SocketFlags.None, out _);
                    string _buffer = Encoding.ASCII.GetString(_receive, 0, _num);
                    Match result = Regex.Match(_buffer, @"(?<method>GET|OPTIONS|HEAD) (?<path>[^\s]+)");
                    if (!result.Success)
                    {
                        mySocket.Close();
                        continue;
                    }
                    string _method = result.Groups["method"].Value;
                    string _filePath = Regex.Replace(result.Groups["path"].Value.Trim(), @"^https?://[^/]+", "");
                    result = Regex.Match(_buffer, @"Host:(.+)");
                    if (!result.Success)
                    {
                        mySocket.Close();
                        continue;
                    }

                    string _hosts = result.Groups[1].Value.Trim().ToLower();
                    string _tmpPath = Regex.Replace(_filePath, @"\?.+$", ""), _localPath = string.Empty;
                    if (Properties.Settings.Default.LocalUpload)
                    {
                        if (File.Exists(Properties.Settings.Default.LocalPath + _tmpPath))
                            _localPath = Properties.Settings.Default.LocalPath + _tmpPath.Replace("/", "\\");
                        else if (File.Exists(Properties.Settings.Default.LocalPath + "\\" + Path.GetFileName(_tmpPath)))
                            _localPath = Properties.Settings.Default.LocalPath + "\\" + Path.GetFileName(_tmpPath);
                        else if (dicAppLocalUploadFile.ContainsKey(_filePath) && File.Exists(Properties.Settings.Default.LocalPath + "\\" + dicAppLocalUploadFile[_filePath]))
                        {
                            _tmpPath = dicAppLocalUploadFile[_filePath];
                            _localPath = Properties.Settings.Default.LocalPath + "\\" + _tmpPath;
                        }
                    }
                    string _extension = Path.GetExtension(_tmpPath).ToLowerInvariant();
                    if (Properties.Settings.Default.LocalUpload && !string.IsNullOrEmpty(_localPath))
                    {
                        FileStream? fs = null;
                        try
                        {
                            fs = new(_localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        catch (Exception ex)
                        {
                            if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("Local Upload", ex.Message, mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty, 0xFF0000);
                        }
                        if (fs != null)
                        {
                            if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("Local Upload", _localPath, mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty, 0x008000);
                            using BinaryReader br = new(fs);
                            string _contentRange = string.Empty, _status = "200 OK";
                            long _fileLength = br.BaseStream.Length, _startPosition = 0;
                            long _endPosition = _fileLength;
                            result = Regex.Match(_buffer, @"Range: bytes=(?<StartPosition>\d+)(-(?<EndPosition>\d+))?");
                            if (result.Success)
                            {
                                _startPosition = long.Parse(result.Groups["StartPosition"].Value);
                                if (_startPosition > br.BaseStream.Length) _startPosition = 0;
                                if (!string.IsNullOrEmpty(result.Groups["EndPosition"].Value))
                                    _endPosition = long.Parse(result.Groups["EndPosition"].Value) + 1;
                                _contentRange = "bytes " + _startPosition + "-" + (_endPosition - 1) + "/" + _fileLength;
                                _status = "206 Partial Content";
                            }

                            StringBuilder sb = new();
                            sb.Append("HTTP/1.1 " + _status + "\r\n");
                            sb.Append("Content-Type: " + ClassWeb.GetMimeMapping(_filePath) + "\r\n");
                            sb.Append("Content-Length: " + (_endPosition - _startPosition) + "\r\n");
                            if (_contentRange != null) sb.Append("Content-Range: " + _contentRange + "\r\n");
                            sb.Append("Accept-Ranges: bytes\r\n\r\n");

                            Byte[] _headers = Encoding.ASCII.GetBytes(sb.ToString());
                            mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);

                            br.BaseStream.Position = _startPosition;
                            int _size = 4096;
                            while (Form1.bServiceFlag && mySocket.Connected)
                            {
                                long _remaining = _endPosition - br.BaseStream.Position;
                                byte[] _response = new byte[_remaining <= _size ? _remaining : _size];
                                br.Read(_response, 0, _response.Length);
                                mySocket.Send(_response, 0, _response.Length, SocketFlags.None, out _);
                                if (_remaining <= _size) break;
                            }
                            fs.Close();
                            fs.Dispose();
                        }
                        else
                        {
                            Byte[] _response = Encoding.ASCII.GetBytes("Internal Server Error");
                            StringBuilder sb = new();
                            sb.Append("HTTP/1.1 500 Server Error\r\n");
                            sb.Append("Content-Type: text/html\r\n");
                            sb.Append("Content-Length: " + _response.Length + "\r\n\r\n");
                            Byte[] _headers = Encoding.ASCII.GetBytes(sb.ToString());
                            mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);
                            mySocket.Send(_response, 0, _response.Length, SocketFlags.None, out _);
                        }
                    }
                    else
                    {
                        bool _redirect = false;
                        string _newHosts = string.Empty;
                        if (Properties.Settings.Default.GameLink)
                        {
                            switch (_hosts)
                            {
                                case "xvcf1.xboxlive.com":
                                case "assets1.xboxlive.com":
                                case "d1.xboxlive.com":
                                case "assets1.xboxlive.cn":
                                case "d1.xboxlive.cn":
                                    _redirect = true;
                                    _newHosts = Regex.Replace(_hosts, @"1", "2");
                                    if (dicFilePath.TryAdd(_filePath, string.Empty))
                                        ThreadPool.QueueUserWorkItem(delegate { UpdateGameUrl(_hosts, _filePath, _extension); });
                                    break;
                                case "dlassets.xboxlive.com":
                                case "dlassets.xboxlive.cn":
                                    _redirect = true;
                                    _newHosts = Regex.Replace(_hosts, @"dlassets", "dlassets2");
                                    if (dicFilePath.TryAdd(_filePath, string.Empty))
                                        ThreadPool.QueueUserWorkItem(delegate { UpdateGameUrl(_hosts, _filePath, _extension); });
                                    break;
                                case "xvcf2.xboxlive.com":
                                case "assets2.xboxlive.com":
                                case "dlassets2.xboxlive.com":
                                case "d2.xboxlive.com":
                                case "assets2.xboxlive.cn":
                                case "dlassets2.xboxlive.cn":
                                case "d2.xboxlive.cn":
                                    if (dicFilePath.TryAdd(_filePath, string.Empty))
                                        ThreadPool.QueueUserWorkItem(delegate { UpdateGameUrl(_hosts, _filePath, _extension); });
                                    break;
                                case "us.cdn.blizzard.com":
                                case "eu.cdn.blizzard.com":
                                case "kr.cdn.blizzard.com":
                                case "level3.blizzard.com":
                                    if (Properties.Settings.Default.BattleStore && Properties.Settings.Default.BattleCDN)
                                    {
                                        _redirect = true;
                                        _newHosts = "blzddist1-a.akamaihd.net";
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (_hosts)
                            {
                                case "xvcf1.xboxlive.com":
                                case "xvcf2.xboxlive.com":
                                case "assets1.xboxlive.com":
                                case "assets2.xboxlive.com":
                                case "dlassets.xboxlive.com":
                                case "dlassets2.xboxlive.com":
                                case "d1.xboxlive.com":
                                case "d2.xboxlive.com":
                                case "assets1.xboxlive.cn":
                                case "assets2.xboxlive.cn":
                                case "dlassets.xboxlive.cn":
                                case "dlassets2.xboxlive.cn":
                                case "d1.xboxlive.cn":
                                case "d2.xboxlive.cn":
                                    if (dicFilePath.TryAdd(_filePath, string.Empty))
                                        ThreadPool.QueueUserWorkItem(delegate { UpdateGameUrl(_hosts, _filePath, _extension); });
                                    break;
                                case "us.cdn.blizzard.com":
                                case "eu.cdn.blizzard.com":
                                case "kr.cdn.blizzard.com":
                                case "level3.blizzard.com":
                                    if (Properties.Settings.Default.BattleStore && Properties.Settings.Default.BattleCDN)
                                    {
                                        _redirect = true;
                                        _newHosts = "blzddist1-a.akamaihd.net";
                                    }
                                    break;
                            }
                        }
                        
                        if (_redirect)
                        {
                            string _url = "http://" + _newHosts + _filePath;
                            StringBuilder sb = new();
                            sb.Append("HTTP/1.1 302 Moved Temporarily\r\n");
                            sb.Append("Content-Type: text/html\r\n");
                            sb.Append("Location: " + _url + "\r\n");
                            sb.Append("Content-Length: 0\r\n\r\n");
                            Byte[] _headers = Encoding.ASCII.GetBytes(sb.ToString());
                            mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);
                            if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("HTTP 302", _url, mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty, 0x008000);
                        }
                        else
                        {
                            bool bFileNotFound = true;
                            string _url = "http://" + _hosts + _filePath;
                            if (_hosts == "dl.delivery.mp.microsoft.com" || _extension == ".phf" || _extension == ".json")
                            {
                                string? ip = ClassDNS.DoH(_hosts);
                                if (!string.IsNullOrEmpty(ip))
                                {
                                    var headers = new Dictionary<string, string>() { { "Host", _hosts } };
                                    using HttpResponseMessage? response = ClassWeb.HttpResponseMessage(_url.Replace(_hosts, ip), "GET", null, null, headers);
                                    if (response != null && response.IsSuccessStatusCode)
                                    {
                                        bFileNotFound = false;
                                        byte[] buffer = response.Content.ReadAsByteArrayAsync().Result;
                                        string str = "HTTP/1.1 200 OK\r\n" + Regex.Replace(response.Content.Headers.ToString(), @"^Content-Length: .+\r\n", "") + "Content-Length: " + buffer.Length + "\r\n" + response.Headers;
                                        Byte[] _headers = Encoding.ASCII.GetBytes(str);
                                        mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);
                                        mySocket.Send(buffer, 0, buffer.Length, SocketFlags.None, out _);
                                        if (Properties.Settings.Default.RecordLog)
                                        {
                                            parentForm.SaveLog("HTTP " + ((int)response.StatusCode), _url, mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty);
                                            if (_hosts.EndsWith(".prod.dl.playstation.net") && _extension == ".json")
                                            {
                                                string html = response.Content.ReadAsStringAsync().Result;
                                                if (Regex.IsMatch(html, @"^{.+}$"))
                                                {
                                                    try
                                                    {
                                                        var json = JsonSerializer.Deserialize<PsGame.Game>(html, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                                        if (json != null && json.Pieces != null && json.Pieces.Count >= 1)
                                                        {
                                                            StringBuilder sbFile = new();
                                                            sbFile.AppendLine("Total download files: " + json.NumberOfSplitFiles + "，Size：" + ClassMbr.ConvertBytes(Convert.ToUInt64(json.OriginalFileSize)) + "，download link: ");
                                                            foreach (var pieces in json.Pieces)
                                                                sbFile.AppendLine(pieces.Url);
                                                            parentForm.SaveLog("Download Link", sbFile.ToString(), mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty, 0x008000);
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Properties.Settings.Default.LocalUpload && _hosts == "tlu.dl.delivery.mp.microsoft.com" && !dicAppLocalUploadFile.ContainsKey(_filePath))
                            {
                                string? ip = ClassDNS.DoH(_hosts);
                                if (!string.IsNullOrEmpty(ip))
                                {
                                    var headers = new Dictionary<string, string>() { { "Host", _hosts } };
                                    using HttpResponseMessage? response = ClassWeb.HttpResponseMessage(_url.Replace(_hosts, ip), "HEAD", null, null, headers);
                                    if (response != null && response.IsSuccessStatusCode)
                                    {
                                        if (response.Content.Headers.TryGetValues("Content-Disposition", out IEnumerable<string>? values))
                                        {
                                            string filename = Regex.Replace(values.FirstOrDefault() ?? string.Empty, @".+filename=", "");
                                            dicAppLocalUploadFile.AddOrUpdate(_filePath, filename, (oldkey, oldvalue) => filename);
                                        }
                                    }
                                }
                            }
                            else if (_hosts == "ctest.cdn.nintendo.net" && _tmpPath.ToLower() == "/")
                            {
                                bFileNotFound = false;
                                if (Properties.Settings.Default.NSBrowser)
                                {
                                    StringBuilder sb = new();
                                    sb.Append("HTTP/1.1 302 Moved Temporarily\r\n");
                                    sb.Append("Content-Type: text/html\r\n");
                                    sb.Append("Location: " + Properties.Settings.Default.NSHomepage + "\r\n");
                                    sb.Append("Content-Length: 0\r\n\r\n");
                                    Byte[] _headers = Encoding.ASCII.GetBytes(sb.ToString());
                                    mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);
                                }
                                else
                                {
                                    Byte[] _response = Encoding.ASCII.GetBytes("ok");
                                    StringBuilder sb = new();
                                    sb.Append("HTTP/1.1 200 OK\r\n");
                                    sb.Append("Content-Type: text/plain\r\n");
                                    sb.Append("X-Organization: Nintendo\r\n");
                                    sb.Append("Content-Length: " + _response.Length + "\r\n\r\n");
                                    Byte[] _headers = Encoding.ASCII.GetBytes(sb.ToString());
                                    mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);
                                    mySocket.Send(_response, 0, _response.Length, SocketFlags.None, out _);
                                    if (Properties.Settings.Default.RecordLog) parentForm.SaveLog("HTTP 200", _url, mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty);
                                }
                            }
                            if (bFileNotFound)
                            {
                                Byte[] _response = Encoding.ASCII.GetBytes("File not found.");
                                StringBuilder sb = new();
                                sb.Append("HTTP/1.1 404 Not Found\r\n");
                                sb.Append("Content-Type: text/html\r\n");
                                sb.Append("Content-Length: " + _response.Length + "\r\n\r\n");
                                Byte[] _headers = Encoding.ASCII.GetBytes(sb.ToString());
                                mySocket.Send(_headers, 0, _headers.Length, SocketFlags.None, out _);
                                mySocket.Send(_response, 0, _response.Length, SocketFlags.None, out _);
                                if (Properties.Settings.Default.RecordLog)
                                {

                                    int argb = 0;
                                    switch (_hosts)
                                    {
                                        case "assets1.xboxlive.com":
                                        case "assets2.xboxlive.com":
                                        case "dlassets.xboxlive.com":
                                        case "dlassets2.xboxlive.com":
                                        case "d1.xboxlive.com":
                                        case "d2.xboxlive.com":
                                        case "xvcf1.xboxlive.com":
                                        case "xvcf2.xboxlive.com":
                                        case "assets1.xboxlive.cn":
                                        case "assets2.xboxlive.cn":
                                        case "dlassets.xboxlive.cn":
                                        case "dlassets2.xboxlive.cn":
                                        case "d1.xboxlive.cn":
                                        case "d2.xboxlive.cn":
                                            argb = 0x008000;
                                            if (dicFilePath.TryAdd(_filePath, string.Empty))
                                                ThreadPool.QueueUserWorkItem(delegate { UpdateGameUrl(_hosts, _filePath, _extension); });
                                            break;
                                        case "tlu.dl.delivery.mp.microsoft.com":
                                        case "download.xbox.com":
                                        case "download.xbox.com.edgesuite.net":
                                        case "xbox-ecn102.vo.msecnd.net":
                                        case "gst.prod.dl.playstation.net":
                                        case "gs2.ww.prod.dl.playstation.net":
                                        case "zeus.dl.playstation.net":
                                        case "ares.dl.playstation.net":
                                            argb = 0x008000;
                                            break;
                                    }
                                    parentForm.SaveLog("HTTP 404", _url, mySocket.RemoteEndPoint != null ? ((IPEndPoint)mySocket.RemoteEndPoint).Address.ToString() : string.Empty, argb);
                                }
                            }
                        }
                    }
                }
            }
            if (mySocket.Connected)
            {
                try
                {
                    mySocket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    mySocket.Close();
                }
            }
            mySocket.Dispose();
        }

        public void Close()
        {
            socket?.Close();
            socket?.Dispose();
            socket = null;
        }

        readonly ConcurrentDictionary<String, String> dicFilePath = new();
        private static void UpdateGameUrl(string _hosts, string _filePath, string _extension)
        {
            if (Regex.IsMatch(_extension, @"\.(phf|xsp)$")) return;
            Match result = Regex.Match(_filePath, @"/(?<ContentId>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})/(?<Version>\d+\.\d+\.\d+\.\d+)\.\w{8}-\w{4}-\w{4}-\w{4}-\w{12}");
            if (result.Success)
            {
                string key = result.Groups["ContentId"].Value.ToLower();
                if (Regex.IsMatch(_filePath, @"_xs\.xvc$", RegexOptions.IgnoreCase))
                    key += "_xs";
                else if (!Regex.IsMatch(_filePath, @"\.msixvc$", RegexOptions.IgnoreCase))
                    key += "_x";
                Version version = new(result.Groups["Version"].Value);
                if (XboxGameDownload.dicXboxGame.TryGetValue(key, out XboxGameDownload.Products? XboxGame))
                {
                    if (XboxGame.Version >= version) return;
                }
                switch (_hosts)
                {
                    case "xvcf1.xboxlive.com":
                    case "xvcf2.xboxlive.com":
                    case "assets2.xboxlive.com":
                    case "d1.xboxlive.com":
                    case "d2.xboxlive.com":
                    case "assets1.xboxlive.cn":
                    case "assets2.xboxlive.cn":
                    case "d1.xboxlive.cn":
                    case "d2.xboxlive.cn":
                        _hosts = "assets1.xboxlive.com";
                        break;
                    case "dlassets2.xboxlive.com":
                    case "dlassets.xboxlive.cn":
                    case "dlassets2.xboxlive.cn":
                        _hosts = "dlassets.xboxlive.com";
                        break;
                }
                string? ip = ClassDNS.DoH(_hosts);
                if (!string.IsNullOrEmpty(ip))
                {
                    var headers = new Dictionary<string, string>() { { "Host", _hosts } };
                    using HttpResponseMessage? response = ClassWeb.HttpResponseMessage("http://" + ip + _filePath, "HEAD", null, null, headers);
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.TryGetValues("Content-Length", out IEnumerable<string>? values))
                        {
                            ulong filesize = ulong.Parse(values.FirstOrDefault() ?? "0");
                            XboxGame = new XboxGameDownload.Products
                            {
                                Version = version,
                                FileSize = filesize,
                                Url = "http://" + _hosts + _filePath
                            };
                            XboxGameDownload.dicXboxGame.AddOrUpdate(key, XboxGame, (oldkey, oldvalue) => XboxGame);
                            XboxGameDownload.SaveXboxGame();
                            _ = ClassWeb.HttpResponseContent(UpdateFile.homePage + "/Game/AddGameUrl?url=" + ClassWeb.UrlEncode(XboxGame.Url), "PUT", null, null, null, 30000, "XboxDownload");
                        }
                    }
                }
            }
        }
    }
}

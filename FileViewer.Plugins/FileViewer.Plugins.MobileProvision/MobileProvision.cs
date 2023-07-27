using FileViewer.Base;
using FileViewer.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace FileViewer.Plugins.MobileProvision
{
    class MobileProvision : BackgroundWorkBase, INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
        readonly IManager _manager;
        public MobileProvision(IManager manager)
        {
            this._manager = manager;
        }


        public string SignName { get; set; } = string.Empty;
        public DateTime SignExpirationDateTime { get; set; } = DateTime.Now;
        public string SignExpirationDate => ProcessDateDesc(SignExpirationDateTime);
        public Brush SignExpirationColor => ProcessDateColor(SignExpirationDateTime);
        public ObservableCollection<KeyValuePair<string, string>> BaseList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> EntitlementsList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> CertificatesList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> ProvisionedDevices { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public bool ShowProvisionedDevices => ProvisionedDevices.Count > 0;
        public bool ShowCertificates => CertificatesList.Count > 0;

        private readonly double height = SystemParameters.WorkArea.Height / 2;
        private readonly double width = SystemParameters.WorkArea.Width / 2;

        private string currentFilePath = string.Empty;
        public void ChangeFile(string filePath)
        {
            currentFilePath = filePath;
            InitBackGroundWork();
            bgWorker?.RunWorkerAsync(filePath);
            _manager.LoadFileSuccess(height, width, true, null, _manager.IsDarkMode() ? Utils.BackgroundDark : Utils.BackgroundHello);
        }

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundHello);
        }

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            byte[]? bytes = null;
            try
            {
                bytes = System.IO.File.ReadAllBytes((string)e.Argument!);
                var signedCms = new SignedCms();
                signedCms.Decode(bytes);
                e.Result = signedCms.ContentInfo.Content;
            }
            catch (Exception)
            {
                if (bytes != null)
                {
                    var text = Encoding.UTF8.GetString(bytes);
                    Match match = Regex.Match(text, "<plist[\\S\\s]+</plist>");
                    if (match.Success)
                    {
                        e.Result = bytes;
                        return;
                    }
                }
                e.Result = null;
            }
        }

        protected override void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
        }
        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null || ((byte[])e.Result).Length <= 0)
            {
                _manager.LoadFileFailed(currentFilePath);
                return;
            }
            ProcessXML((byte[])e.Result);
        }

        private void ProcessXML(byte[] data)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    Dictionary<string, object> node = (Dictionary<string, object>)PList.ReadPlist(stream, PlistType.Auto);
                    BaseList.Clear();
                    EntitlementsList.Clear();
                    CertificatesList.Clear();
                    ProvisionedDevices.Clear();
                    ProcessBase(node);
                    foreach (string key in node.Keys)
                    {
                        switch (key)
                        {
                            case "Name":
                                SignName = PList.ParsePNodeString(node[key]);
                                break;
                            case "ExpirationDate":
                                SignExpirationDateTime = (DateTime)node[key];
                                break;
                            case "Entitlements":
                                Dictionary<string, object> entitlements = (Dictionary<string, object>)node[key];
                                foreach (var entitlement in entitlements.Select(m => new KeyValuePair<string, string>(m.Key + ":", PList.ParsePNodeString(m.Value))))
                                {
                                    EntitlementsList.Add(entitlement);
                                }
                                if (entitlements["application-identifier"] != null)
                                {
                                    BaseList.Insert(Math.Min(BaseList.Count, 1), new KeyValuePair<string, string>("App ID:", PList.ParsePNodeString(entitlements["application-identifier"])));
                                }
                                break;
                            case "DeveloperCertificates":
                                ProcessCertificate(PList.ParsePNodeString(node[key], true));
                                break;
                            case "ProvisionedDevices":
                                List<object> provisionedDevicesNode = (List<object>)node[key];
                                foreach (var device in provisionedDevicesNode.Select(m => new KeyValuePair<string, string>("Device ID:", PList.ParsePNodeString(m))))
                                {
                                    ProvisionedDevices.Add(device);
                                }
                                break;
                        }
                    }
                }
                _manager.SetLoading(false);
            }
            catch (Exception)
            {
                _manager.LoadFileFailed(currentFilePath);
            }
        }

        private void ProcessBase(Dictionary<string, object> node)
        {
            BaseList.Add(new KeyValuePair<string, string>("App ID Name:", PList.ParsePNodeString(node["AppIDName"])));
            BaseList.Add(new KeyValuePair<string, string>("Team:", $"{PList.ParsePNodeString(node["TeamName"])} ({PList.ParsePNodeString(node["TeamIdentifier"])})"));
            BaseList.Add(new KeyValuePair<string, string>("Platform:", PList.ParsePNodeString(node["Platform"])));
            BaseList.Add(new KeyValuePair<string, string>("UUID:", PList.ParsePNodeString(node["UUID"])));
            BaseList.Add(new KeyValuePair<string, string>("Creation Date:", PList.ParsePNodeString(node["CreationDate"])));
            BaseList.Add(new KeyValuePair<string, string>("Expiration Date:", PList.ParsePNodeString(node["ExpirationDate"])));
        }

        private void ProcessCertificate(string base64Str)
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2(Encoding.UTF8.GetBytes(base64Str));
                CertificatesList.Add(new KeyValuePair<string, string>("Name:", ParseCertificateName(certificate.Subject)));
                CertificatesList.Add(new KeyValuePair<string, string>("Creation Date:", certificate.NotBefore.ToString()));
                CertificatesList.Add(new KeyValuePair<string, string>("Expiration Date:", certificate.GetExpirationDateString()));
                CertificatesList.Add(new KeyValuePair<string, string>("Serial Number:", certificate.SerialNumber));
            }
            catch (Exception)
            {
            }
        }

        private string ParseCertificateName(string subject)
        {
            var subjects = subject.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < subjects.Length; i++)
            {
                var item = subjects[i].Trim();
                if (item.ToLower().StartsWith("cn="))
                {
                    return item.Substring(3).Trim('"');
                }
            }
            return subject;
        }

        private Brush ProcessDateColor(DateTime dateTime)
        {
            return dateTime > DateTime.Now ? Brushes.Gray : Brushes.Red;
        }

        private string ProcessDateDesc(DateTime dateTime)
        {
            TimeSpan timeDiff = DateTime.Now - dateTime;
            bool expired = timeDiff.TotalMilliseconds > 0;
            string timeUnit;
            int timeLeft;
            if (Math.Abs(timeDiff.TotalDays) > 365)
            {
                timeUnit = "year";
                timeLeft = (int)(Math.Abs(timeDiff.TotalDays) / 365);
            }
            else if (Math.Abs(timeDiff.TotalDays) > 30)
            {
                timeUnit = "month";
                timeLeft = (int)(Math.Abs(timeDiff.TotalDays) / 30);
            }
            else if (Math.Abs(timeDiff.TotalDays) > 7)
            {
                timeUnit = "week";
                timeLeft = (int)(Math.Abs(timeDiff.TotalDays) / 7);
            }
            else if (Math.Abs(timeDiff.TotalDays) > 0)
            {
                timeUnit = "day";
                timeLeft = (int)Math.Abs(timeDiff.TotalDays);
            }
            else if (Math.Abs(timeDiff.TotalHours) > 0)
            {
                timeUnit = "hour";
                timeLeft = (int)Math.Abs(timeDiff.TotalHours);
            }
            else
            {
                timeUnit = "minite";
                timeLeft = (int)Math.Abs(timeDiff.TotalMinutes);
            }
            return $"{(expired ? "Expired" : "Expires")} in {timeLeft} {timeUnit}{(timeLeft > 1 ? "s" : "")}{(expired ? " ago" : "")}";
        }
    }
}

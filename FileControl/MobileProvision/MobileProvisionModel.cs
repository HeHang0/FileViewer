using FileViewer.FileHelper;
using FileViewer.Globle;
using PListNet;
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

namespace FileViewer.FileControl.MobileProvision
{
    public class MobileProvisionModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MobileProvisionModel()
        {
        }

        public string SignName { get; set; }
        private DateTime _signExpirationDate = DateTime.Now;
        public string SignExpirationDate
        {
            get
            {
                return ProcessDateDesc(_signExpirationDate);
            }
        }
        public Brush SignExpirationColor
        {
            get
            {
                return ProcessDateColor(_signExpirationDate);
            }
        }
        public ObservableCollection<KeyValuePair<string, string>> BaseList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> EntitlementsList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> CertificatesList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> ProvisionedDevices { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        public bool ShowProvisionedDevices 
        { 
            get
            {
                return ProvisionedDevices.Count > 0;
            }
        }
        public bool ShowCertificates
        { 
            get
            {
                return CertificatesList.Count > 0;
            }
        }

        private double height = SystemParameters.WorkArea.Height / 2;
        private double width = SystemParameters.WorkArea.Width / 2;

        private (string FilePath, FileExtension Ext) currentFilePath;
        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            currentFilePath = file;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnColorChange(Color.FromRgb(0xA1, 0xD5, 0xD3));
            GlobalNotify.OnSizeChange(height, width);
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] bytes = null;
            try
            {
                bytes = File.ReadAllBytes((string)e.Argument);
                var signedCms = new SignedCms();
                signedCms.Decode(bytes);
                e.Result = signedCms.ContentInfo.Content;
            }
            catch (Exception)
            {
                if(bytes != null)
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

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null || ((byte[])e.Result).Length <= 0)
            {
                GlobalNotify.OnFileLoadFailed(currentFilePath.FilePath);
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
                    PListNet.Nodes.DictionaryNode node = (PListNet.Nodes.DictionaryNode)PList.Load(stream);
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
                                SignName = Utils.ParsePNodeString(node[key]);
                                break;
                            case "ExpirationDate":
                                PListNet.Nodes.DateNode expirationDateNode = (PListNet.Nodes.DateNode)node[key];
                                _signExpirationDate = expirationDateNode.Value;
                                break;
                            case "Entitlements":
                                PListNet.Nodes.DictionaryNode entitlements = (PListNet.Nodes.DictionaryNode)node[key];
                                EntitlementsList.AddRange(entitlements.Select(m => new KeyValuePair<string, string>(m.Key + ":", Utils.ParsePNodeString(m.Value))));
                                if (entitlements["application-identifier"] != null)
                                {
                                    BaseList.Insert(Math.Min(BaseList.Count, 1), new KeyValuePair<string, string>("App ID:", Utils.ParsePNodeString(entitlements["application-identifier"])));
                                }
                                break;
                            case "DeveloperCertificates":
                                ProcessCertificate(Utils.ParsePNodeString(node[key], true));
                                break;
                            case "ProvisionedDevices":
                                PListNet.Nodes.ArrayNode provisionedDevicesNode = (PListNet.Nodes.ArrayNode)node[key];
                                ProvisionedDevices.AddRange(provisionedDevicesNode.Select(m => new KeyValuePair<string, string>("Device ID:", Utils.ParsePNodeString(m))));
                                break;
                        }
                    }
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowProvisionedDevices"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SignExpirationColor"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SignExpirationDate"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowCertificates"));
                GlobalNotify.OnLoadingChange(false);
            }
            catch (Exception)
            {
                GlobalNotify.OnFileLoadFailed(currentFilePath.FilePath);
            }
        }

        private void ProcessBase(PListNet.Nodes.DictionaryNode node)
        {
            BaseList.Add(new KeyValuePair<string, string>("App ID Name:", Utils.ParsePNodeString(node["AppIDName"])));
            BaseList.Add(new KeyValuePair<string, string>("Team:", $"{Utils.ParsePNodeString(node["TeamName"])} ({Utils.ParsePNodeString(node["TeamIdentifier"])})"));
            BaseList.Add(new KeyValuePair<string, string>("Platform:", Utils.ParsePNodeString(node["Platform"])));
            BaseList.Add(new KeyValuePair<string, string>("UUID:", Utils.ParsePNodeString(node["UUID"])));
            BaseList.Add(new KeyValuePair<string, string>("Creation Date:", Utils.ParsePNodeString(node["CreationDate"])));
            BaseList.Add(new KeyValuePair<string, string>("Expiration Date:", Utils.ParsePNodeString(node["ExpirationDate"])));
        }

        private void ProcessCertificate(string base64Str)
        {
            X509Certificate2 certificate = new X509Certificate2(Encoding.UTF8.GetBytes(base64Str));
            CertificatesList.Add(new KeyValuePair<string, string>("Name:", ParseCertificateName(certificate.Subject)));
            CertificatesList.Add(new KeyValuePair<string, string>("Creation Date:", certificate.NotBefore.ToString()));
            CertificatesList.Add(new KeyValuePair<string, string>("Expiration Date:", certificate.GetExpirationDateString()));
            CertificatesList.Add(new KeyValuePair<string, string>("Serial Number:", certificate.SerialNumber));
        }

        private string ParseCertificateName(string subject)
        {
            var subjects = subject.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < subjects.Length; i++)
            {
                var item = subjects[i].Trim();
                if(item.ToLower().StartsWith("cn="))
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

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}

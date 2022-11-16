using FileViewer.FileHelper;
using FileViewer.Globle;
using PListNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public string SignType { get; set; }
        public string SignExpirationDate { get; set; }
        public List<KeyValuePair<string, string>> BaseList { get; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> EntitlementsList { get; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> CertificatesList { get; } = new List<KeyValuePair<string, string>>();

        private double height = SystemParameters.WorkArea.Height / 2;
        private double width = SystemParameters.WorkArea.Width / 2;

        private (string FilePath, FileExtension Ext) currentFilePath;
        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            currentFilePath = file;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnSizeChange(height, width);

        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string text = File.ReadAllText((string)e.Argument);
                Match match = Regex.Match(text, "<plist[\\S\\s]+</plist>");
                if(match.Success)
                {
                    e.Result = "<!DOCTYPE plist PUBLIC \" -//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n"+match.Value;
                }
                else
                {
                    e.Result = "";
                }
            }
            catch (Exception)
            {
                e.Result = "";
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = (string)e.Result;
            if (string.IsNullOrWhiteSpace(result))
            {
                GlobalNotify.OnFileLoadFailed(currentFilePath.FilePath);
                return;
            }
            ProcessXML(result);
        }

        private void ProcessXML(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            try
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    PListNet.Nodes.DictionaryNode node = (PListNet.Nodes.DictionaryNode)PList.Load(stream);
                    BaseList.Clear();
                    EntitlementsList.Clear();
                    CertificatesList.Clear();
                    DateTime creationDate = DateTime.Now;
                    DateTime expirationDate = DateTime.Now;
                    string developerCertificatesBase64 = "";
                    string teamName = "";
                    foreach (string key in node.Keys)
                    {
                        switch (key)
                        {
                            case "Name":
                                PNode<string> name = (PNode<string>)node[key];
                                SignType = name.Value; break;
                            case "ExpirationDate":
                                PListNet.Nodes.DateNode expirationDateNode = (PListNet.Nodes.DateNode)node[key];
                                expirationDate = expirationDateNode.Value;
                                SignExpirationDate = ProcessDateDesc(expirationDate); break;
                            case "Entitlements":
                                PListNet.Nodes.DictionaryNode entitlements = (PListNet.Nodes.DictionaryNode)node[key];
                                EntitlementsList.AddRange(entitlements.ToList().Select(m => new KeyValuePair<string, string>(m.Key, ProcessNodeString(m.Value))));
                                break;
                            case "DeveloperCertificates":
                                developerCertificatesBase64 = ProcessNodeString(node[key], true);
                                break;
                            case "DER-Encoded-Profile":
                            case "ProvisionedDevices":
                            case "Version":
                                break;
                            case "CreationDate":
                                PListNet.Nodes.DateNode creationDateNode = (PListNet.Nodes.DateNode)node[key];
                                creationDate = creationDateNode.Value;
                                break;
                            default:
                                if (key == "TeamName") teamName = ProcessNodeString(node[key]);
                                BaseList.Add(new KeyValuePair<string, string>(key, ProcessNodeString(node[key]))); break;
                        }
                    }
                    BaseList.Add(new KeyValuePair<string, string>("Creation Date", creationDate.ToString()));
                    BaseList.Add(new KeyValuePair<string, string>("Expiration Date", expirationDate.ToString()));
                    if(!string.IsNullOrWhiteSpace(teamName) && !string.IsNullOrWhiteSpace(developerCertificatesBase64))
                    {
                        CertificatesList.Add(new KeyValuePair<string, string>("Name", ProcessCertificate(developerCertificatesBase64, teamName)));
                    }
                }
                GlobalNotify.OnLoadingChange(false);
            }
            catch (Exception)
            {
                GlobalNotify.OnFileLoadFailed(currentFilePath.FilePath);
            }
        }

        private string ProcessCertificate(string base64Str, string teamName)
        {
            string desc = Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
            int index = desc.IndexOf(teamName);
            int length = teamName.Length;
            if (index > 0)
            {
                for (int i = index-1; i > 0; i--)
                {
                    if(desc[i] == ';')
                    {
                        length += (index - i);
                        index = i+1;
                        break;
                    }
                }
            }
            if(index >= 0)
            {
                return desc.Substring(index, length);
            }
            return teamName;
            
        }

        private string ProcessNodeString(PNode node, bool arrayFirst=false)
        {
            if (node.GetType() == typeof(PListNet.Nodes.DictionaryNode))
            {
                PListNet.Nodes.DictionaryNode value = (PListNet.Nodes.DictionaryNode)node;
                return string.Join(", ", value.Values.ToList().Select(m => ProcessNodeString(m)));
            }
            else if (node.GetType() == typeof(PListNet.Nodes.BooleanNode))
            {
                PListNet.Nodes.BooleanNode value = (PListNet.Nodes.BooleanNode)node;
                return value.Value.ToString();
            }
            else if (node.GetType() == typeof(PListNet.Nodes.ArrayNode))
            {
                PListNet.Nodes.ArrayNode value = (PListNet.Nodes.ArrayNode)node;
                if (arrayFirst && value.Count > 0)
                {
                    return ProcessNodeString(value[0]);
                }
                return string.Join(", ", value.ToList().Select(m => ProcessNodeString(m)));
            }
            else if (node.GetType() == typeof(PListNet.Nodes.DataNode))
            {
                PListNet.Nodes.DataNode value = (PListNet.Nodes.DataNode)node;
                return Convert.ToBase64String(value.Value);
            }
            else if (node.GetType() == typeof(PListNet.Nodes.DateNode))
            {
                PListNet.Nodes.DateNode value = (PListNet.Nodes.DateNode)node;
                return value.Value.ToString();
            }
            else if (node.GetType() == typeof(PListNet.Nodes.FillNode))
            {
                PListNet.Nodes.FillNode value = (PListNet.Nodes.FillNode)node;
                return value.ToString();
            }
            else if (node.GetType() == typeof(PListNet.Nodes.IntegerNode))
            {
                PListNet.Nodes.IntegerNode value = (PListNet.Nodes.IntegerNode)node;
                return value.Value.ToString();
            }
            else if (node.GetType() == typeof(PListNet.Nodes.RealNode))
            {
                PListNet.Nodes.RealNode value = (PListNet.Nodes.RealNode)node;
                return value.Value.ToString();
            }
            else if (node.GetType() == typeof(PListNet.Nodes.StringNode))
            {
                PListNet.Nodes.StringNode value = (PListNet.Nodes.StringNode)node;
                return value.Value.ToString();
            }
            else if (node.GetType() == typeof(PListNet.Nodes.UidNode))
            {
                PListNet.Nodes.UidNode value = (PListNet.Nodes.UidNode)node;
                return value.Value.ToString();
            }
            else
            {
                return "";
            }
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

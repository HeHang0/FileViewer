using ApkReader;
using ApkReader.Arsc;
using FileViewer.FileHelper;
using FileViewer.Globle;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using PListNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace FileViewer.FileControl.App
{
    public class AppModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource HelloBack => Utils.GetBitmapSource(Properties.Resources.HelloBack);

        public ImageSource ThumbnailImage { get; private set; }

        public string Name { get; set; }

        public string Size { get; set; }

        public string Package { get; set; }

        public string Version { get; set; }

        public string MinOSVersion { get; set; }

        public string TargetOSVersion { get; set; }

        public string Permissions { get; set; }

        public string DeviceFamily { get; set; }

        public bool ShowPermissions
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Permissions);
            }
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        private string filePath;

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            OnColorChanged(Color.FromRgb(0xA1, 0xD5, 0xD3));
            GlobalNotify.OnSizeChange(450, 800);
            GlobalNotify.OnLoadingChange(true);
            InitBackGroundWork();
            filePath = file.FilePath;
            bgWorker.RunWorkerAsync(file.FilePath);
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var appPath = e.Argument as string;
            try
            {
                var extension = Path.GetExtension(appPath);
                if(extension.ToLower() == ".ipa")
                {
                    e.Result = ParseIpaInfo(appPath);
                }
                else
                {
                    e.Result = ParseAndroidInfo(appPath);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                e.Result = null;
            }
        }

        private (ApkInfo, long, byte[]) ParseIpaInfo(string ipaPath)
        {
            byte[] apkIcon = null;
            var fileInfo = new FileInfo(ipaPath);
            var fileSize = fileInfo.Length;
            ApkInfo ipaInfo = new ApkInfo();
            using (ZipArchive archive = ZipFile.OpenRead(ipaPath))
            {
                string iconName = string.Empty;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.ToLower() == "info.plist" && entry.FullName.Count(m => m == '/') == 2)
                    {
                        byte[] plistBytes = null;
                        using (Stream stream = entry.Open())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                plistBytes = ms.ToArray();
                            }
                        }
                        using (MemoryStream stream = new MemoryStream(plistBytes))
                        {
                            PListNet.Nodes.DictionaryNode node = (PListNet.Nodes.DictionaryNode)PList.Load(stream);
                            node.TryGetValue("CFBundleDisplayName", out PNode nameNode);
                            ipaInfo.Label = Utils.ParsePNodeString(nameNode);
                            node.TryGetValue("CFBundleIdentifier", out PNode packageNode);
                            ipaInfo.PackageName = Utils.ParsePNodeString(packageNode);
                            node.TryGetValue("CFBundleShortVersionString", out PNode versionNode);
                            ipaInfo.VersionName = Utils.ParsePNodeString(versionNode);
                            node.TryGetValue("MinimumOSVersion", out PNode minOSNode);
                            ipaInfo.MinSdkVersion = "IOS " + Utils.ParsePNodeString(minOSNode);
                            node.TryGetValue("DTPlatformVersion", out PNode targetOSNode);
                            ipaInfo.TargetSdkVersion = "IOS " + Utils.ParsePNodeString(targetOSNode);
                            node.TryGetValue("UIDeviceFamily", out PNode familyNode);
                            if (familyNode != null && familyNode.GetType() == typeof(PListNet.Nodes.ArrayNode))
                            {
                                foreach (var item in ((PListNet.Nodes.ArrayNode)familyNode))
                                {
                                    var deviceFamily = Utils.ParsePNodeString(item);
                                    iosDeviceFamily.TryGetValue(deviceFamily, out string deviceFamilyDesc);
                                    if (!string.IsNullOrWhiteSpace(deviceFamilyDesc))
                                    {
                                        ipaInfo.Densities.Add(deviceFamilyDesc);
                                    }
                                }
                            }

                            node.TryGetValue("CFBundleIcons", out PNode iconsNode);
                            if (iconsNode != null && iconsNode.GetType() == typeof(PListNet.Nodes.DictionaryNode))
                            {
                                ((PListNet.Nodes.DictionaryNode)iconsNode).TryGetValue("CFBundlePrimaryIcon", out PNode primaryIconNode);
                                if (primaryIconNode != null && primaryIconNode.GetType() == typeof(PListNet.Nodes.DictionaryNode))
                                {
                                    ((PListNet.Nodes.DictionaryNode)primaryIconNode).TryGetValue("CFBundleIconFiles", out PNode iconFilesNode);
                                    if (iconFilesNode != null)
                                    {
                                        iconName = Utils.ParsePNodeString(iconFilesNode, arrayLast: true);
                                    }
                                }
                            }
                            ipaInfo.Permissions.AddRange(node.Keys.Where(m => m.StartsWith("NS") && m.EndsWith("UsageDescription")).Select(m => m.Substring(2, m.Length - 18)));
                        }
                        break;
                    }
                }
                if(!string.IsNullOrWhiteSpace(iconName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name.StartsWith(iconName))
                        {
                            using (Stream stream = entry.Open())
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    apkIcon = ms.ToArray();
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return (ipaInfo, fileSize, apkIcon);
        }

        private (ApkInfo, long, byte[]) ParseAndroidInfo(string apkPath)
        {
            byte[] apkIcon = null;
            var fileInfo = new FileInfo(apkPath);
            var fileSize = fileInfo.Length;
            var apkReader = new ApkReader.ApkReader();
            InjectApkInfoHandler(apkReader);
            ApkInfo apkInfo = apkReader.Read(apkPath);
            string iconPath = "";
            int iconIndexLast = 0;
            string numberPattern = @"\d+";
            foreach (var item in apkInfo.Icons)
            {
                if (string.IsNullOrWhiteSpace(item.Value) || !item.Value.ToLower().EndsWith(".png")) continue;
                Match m = Regex.Match(item.Key, numberPattern);
                if (m.Success)
                {
                    int.TryParse(m.Value, out int iconIndex);
                    if (iconIndex > iconIndexLast)
                    {
                        iconIndexLast = iconIndex;
                        iconPath = item.Value;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                iconPath = apkInfo.Icon;
            }
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(apkPath))
                {
                    ZipArchiveEntry iconEntry = archive.GetEntry(iconPath);
                    if (iconEntry != null)
                    {
                        using (Stream stream = iconEntry.Open())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                apkIcon = ms.ToArray();
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
            }
            androidVersions.TryGetValue(apkInfo.MinSdkVersion, out string minSdkVersion);
            androidVersions.TryGetValue(apkInfo.TargetSdkVersion, out string targetSdkVersion);
            if (!string.IsNullOrWhiteSpace(minSdkVersion)) apkInfo.MinSdkVersion = minSdkVersion;
            if (!string.IsNullOrWhiteSpace(targetSdkVersion)) apkInfo.TargetSdkVersion = targetSdkVersion;
            return (apkInfo, fileSize, apkIcon);
        }


        class MyApkInfoHandler : IApkInfoHandler<ApkInfo>
        {
            IApkInfoHandler<ApkInfo> parentHandler;
            public MyApkInfoHandler()
            {
                // Get the assembly that contains the internal type.
                Assembly assembly = Assembly.Load("ApkReader");

                // Get the type of the internal class.
                Type type = assembly.GetType("ApkReader.ApkInfoHandler");

                // Create an instance of the internal class.
                parentHandler = (IApkInfoHandler<ApkInfo>)Activator.CreateInstance(type);
            }
            public void Execute(XmlDocument androidManifest, ArscFile resources, ApkInfo apkInfo)
            {
                parentHandler.Execute(androidManifest, resources, apkInfo);
                apkInfo.Densities.Clear();
                apkInfo.Densities.Add("Phone");
                apkInfo.Densities.Add("Pad");
                foreach (XmlNode childNode in androidManifest.DocumentElement.ChildNodes)
                {
                    if (childNode.LocalName != "uses-feature") continue;
                    string usesPermission = childNode.Attributes?.GetNamedItem("name")?.Value;
                    switch (usesPermission)
                    {
                        case "android.software.leanback":
                            apkInfo.Densities.Add("TV");
                            break;
                        case "android.hardware.type.automotive":
                            apkInfo.Densities.Add("Automotive");
                            break;
                        case "android.hardware.type.watch":
                            apkInfo.Densities.Add("Wear");
                            break;
                    }
                }
            }
        }

        private void InjectApkInfoHandler(ApkReader<ApkInfo> apkReader)
        {
            Type type = typeof(ApkReader<ApkInfo>);
            FieldInfo fieldInfo = type.GetField("_apkInfoHandlers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            fieldInfo.SetValue(apkReader, new List<IApkInfoHandler<ApkInfo>>() { new MyApkInfoHandler() });
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Result == null)
            {
                GlobalNotify.OnFileLoadFailed(filePath);
                return;
            }
            var (apkInfo, fileSize, apkIcon) = ((ApkInfo, long, byte[]))e.Result;
            Size = $"{fileSize.ToSizeString()} ({fileSize}字节)";
            Name = apkInfo.Label;
            if(apkIcon != null)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(apkIcon))
                    {
                        ThumbnailImage = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                catch (System.Exception)
                {
                    ThumbnailImage = Utils.GetBitmapSource(Properties.Resources.preview);
                }
            }
            else
            {
                ThumbnailImage = Utils.GetBitmapSource(Properties.Resources.preview);
            }
            Package = apkInfo.PackageName;
            Version = apkInfo.VersionName;
            MinOSVersion = apkInfo.MinSdkVersion;
            TargetOSVersion = apkInfo.TargetSdkVersion;
            Permissions = string.Join("\n", apkInfo.Permissions);
            DeviceFamily = string.Join("、", apkInfo.Densities);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowPermissions"));
            GlobalNotify.OnLoadingChange(false);
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        Dictionary<string, string> iosDeviceFamily = new Dictionary<string, string>()
        {
            { "1", "iPhone" },
            { "2", "iPad" },
            { "3", "Apple TV" },
            { "4", "Apple Watch" },
            { "5", "HomePod" },
            { "6", "Mac" },
        };

        Dictionary<string, string> androidVersions = new Dictionary<string, string>()
        {
            {"33", "Android 13"},
            {"32", "Android 12L"},
            {"31", "Android 12"},
            {"30", "Android 11"},
            {"29", "Android 10"},
            {"28", "Pie (Android 9)"},
            {"27", "Oreo (Android 8.1)"},
            {"26", "Oreo (Android 8.0)"},
            {"25", "Nougat (Android 7.1)"},
            {"24", "Nougat (Android 7.0)"},
            {"23", "Marshmallow (Android 6.0)"},
            {"22", "Lollipop (Android 5.1)"},
            {"21", "Lollipop (Android 5.0)"},
            {"19", "KitKat (Android 4.4)"},
            {"18", "Jelly Bean (Android 4.3)"},
            {"17", "Jelly Bean (Android 4.2)"},
            {"16", "Jelly Bean (Android 4.1)"},
            {"15", "Ice Cream Sandwich (Android 4.0.3 - 4.0.4)"},
            {"14", "Ice Cream Sandwich (Android 4.0.1 - 4.0.2)"},
            {"13", "Honeycomb (Android 3.2)"},
            {"12", "Honeycomb (Android 3.1)"},
            {"11", "Honeycomb (Android 3.0)"},
            {"10", "Gingerbread (Android 2.3.3 - 2.3.7)"},
            {"9", "Gingerbread (Android 2.3 - 2.3.2)"},
            {"8", "Froyo (Android 2.2)"},
            {"7", "Eclair (Android 2.1)"},
            {"6", "Eclair (Android 2.0.1)"},
            {"5", "Eclair (Android 2.0)"},
            {"4", "Donut (Android 1.6)"},
            {"3", "Cupcake (Android 1.5)"},
            {"2", "Android 1.1"},
            {"1", "Android 1.0"}
        };
    }
}

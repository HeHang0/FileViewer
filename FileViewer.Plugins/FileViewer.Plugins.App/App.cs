using ApkReader;
using ApkReader.Arsc;
using FileViewer.Base;
using FileViewer.Icns.IcnsParser;
using FileViewer.Tools;
using PListNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using File = System.IO.File;

namespace FileViewer.Plugins.App
{
    public class App : BackgroundWorkBase, INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
        readonly IManager manager;
        public App(IManager manager)
        {
            this.manager = manager;
        }

        private readonly ImageSource? _helloBack = Utils.GetBitmapSource(Utils.ImageHelloBack);
        public ImageSource? HelloBack { get; set; }

        public ImageSource? ThumbnailImage { get; private set; }

        public string? Name { get; set; }

        public string? Size { get; set; }

        public string? Package { get; set; }

        public string? Version { get; set; }

        public string? MinOSVersion { get; set; }

        public string? TargetOSVersion { get; set; }

        public string? Permissions { get; set; }

        public string? DeviceFamily { get; set; }

        public bool ShowPermissions => !string.IsNullOrWhiteSpace(Permissions);

        private string? _filePath;
        public void ChangeFile(string filePath)
        {
            _filePath = filePath;
            InitBackGroundWork();
            bgWorker?.RunWorkerAsync(filePath);
            manager.SetSize(480, 800);
            manager.SetResizeMode(false);
            ChangeTheme(manager.IsDarkMode());
        }

        public void ChangeTheme(bool dark)
        {
            manager.SetColor(dark ? Utils.BackgroundDark : System.Windows.Media.Color.FromRgb(0xA1, 0xD5, 0xD3));
            HelloBack = dark ? null : _helloBack;
        }

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            try
            {
                string appPath = (string)e.Argument!;
                string extension = Path.GetExtension(appPath).ToLower();
                switch (extension)
                {
                    case ".apk":
                        e.Result = ParseAndroidInfo(appPath);
                        break;
                    case ".ipa":
                        e.Result = ParseIpaInfo(appPath);
                        break;
                    case ".app":
                        e.Result = ParseMacAppInfo(appPath);
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
        }

        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                if (_filePath != null) manager.LoadFileFailed(_filePath);
                return;
            }
            var (apkInfo, fileSize, apkIcon) = ((ApkInfo, long, byte[]))e.Result;
            Size = $"{fileSize.ToSizeString()} ({fileSize}字节)";
            Name = apkInfo.Label;
            if (apkIcon != null)
            {
                try
                {
                    using MemoryStream stream = new(apkIcon);
                    ThumbnailImage = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch (Exception)
                {
                    ThumbnailImage = Utils.GetBitmapSource(Utils.ImagePreview);
                }
            }
            else
            {
                ThumbnailImage = Utils.GetBitmapSource(Utils.ImagePreview);
            }
            Package = apkInfo.PackageName;
            Version = apkInfo.VersionName;
            MinOSVersion = apkInfo.MinSdkVersion;
            TargetOSVersion = apkInfo.TargetSdkVersion;
            Permissions = string.Join("\n", apkInfo.Permissions);
            DeviceFamily = string.Join("、", apkInfo.Densities);
            manager.SetLoading(false);
        }

        private long GetDirectorySize(string dirPath)
        {
            return GetDirectorySize(new DirectoryInfo(dirPath));
        }

        private long GetDirectorySize(DirectoryInfo dirInfo)
        {
            long totalSize = 0;
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                totalSize += fileInfo.Length;
            }
            foreach (var dirItemInfo in dirInfo.GetDirectories())
            {
                totalSize += GetDirectorySize(dirItemInfo);
            }
            return totalSize;
        }

        private (ApkInfo, long, byte[]?) ParseMacAppInfo(string appPath)
        {
            if (!Directory.Exists(appPath)) throw new DirectoryNotFoundException();
            string infoPath = Path.Combine(appPath, "Contents", "Info.plist");
            if (!File.Exists(infoPath)) throw new FileNotFoundException();
            byte[]? appIcon = null;
            var fileSize = GetDirectorySize(appPath);
            ApkInfo appInfo = new();
            using (FileStream stream = File.OpenRead(infoPath))
            {
                PListNet.Nodes.DictionaryNode node = (PListNet.Nodes.DictionaryNode)PList.Load(stream);
                node.TryGetValue("CFBundleName", out PNode nameNode);
                appInfo.Label = ParsePNodeString(nameNode);
                node.TryGetValue("CFBundleIdentifier", out PNode packageNode);
                appInfo.PackageName = ParsePNodeString(packageNode);
                node.TryGetValue("CFBundleShortVersionString", out PNode versionNode);
                appInfo.VersionName = ParsePNodeString(versionNode);
                node.TryGetValue("LSMinimumSystemVersion", out PNode minOSNode);
                appInfo.MinSdkVersion = "MacOS " + ParsePNodeString(minOSNode);
                node.TryGetValue("DTSDKName", out PNode targetOSNode);
                appInfo.TargetSdkVersion = "MacOS " + Regex.Replace(ParsePNodeString(targetOSNode), "[a-zA-Z]", "");
                node.TryGetValue("CFBundleSupportedPlatforms", out PNode familyNode);
                if (familyNode != null && familyNode.GetType() == typeof(PListNet.Nodes.ArrayNode))
                {
                    foreach (var item in ((PListNet.Nodes.ArrayNode)familyNode))
                    {
                        appInfo.Densities.Add(ParsePNodeString(item));
                    }
                }

                node.TryGetValue("CFBundleIconFile", out PNode iconsNode);
                string appIconName = ParsePNodeString(iconsNode);
                string resourcePath = Path.Combine(appPath, "Contents", "Resources");
                if (Directory.Exists(resourcePath))
                {

                    foreach (var resourceInfo in Directory.EnumerateFiles(resourcePath, appIconName + "*"))
                    {
                        try
                        {
                            var iconBitmap = GetIcnsMax(resourceInfo);
                            if (iconBitmap != null)
                            {
                                using MemoryStream ms = new();
                                iconBitmap.Save(ms, ImageFormat.Png);
                                appIcon = ms.ToArray();
                                break;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            return (appInfo, fileSize, appIcon);
        }

        private (ApkInfo, long, byte[]?) ParseIpaInfo(string ipaPath)
        {
            byte[]? ipaIcon = null;
            var fileInfo = new FileInfo(ipaPath);
            var fileSize = fileInfo.Length;
            ApkInfo ipaInfo = new();
            using (ZipArchive archive = ZipFile.OpenRead(ipaPath))
            {
                string iconName = string.Empty;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.ToLower() == "info.plist" && entry.FullName.Count(m => m == '/') == 2)
                    {
                        byte[]? plistBytes = null;
                        using (Stream stream = entry.Open())
                        {
                            using MemoryStream ms = new();
                            stream.CopyTo(ms);
                            plistBytes = ms.ToArray();
                        }
                        using (MemoryStream stream = new(plistBytes))
                        {
                            PListNet.Nodes.DictionaryNode node = (PListNet.Nodes.DictionaryNode)PList.Load(stream);
                            node.TryGetValue("CFBundleDisplayName", out PNode nameNode);
                            ipaInfo.Label = ParsePNodeString(nameNode);
                            node.TryGetValue("CFBundleIdentifier", out PNode packageNode);
                            ipaInfo.PackageName = ParsePNodeString(packageNode);
                            node.TryGetValue("CFBundleShortVersionString", out PNode versionNode);
                            ipaInfo.VersionName = ParsePNodeString(versionNode);
                            node.TryGetValue("MinimumOSVersion", out PNode minOSNode);
                            ipaInfo.MinSdkVersion = "iOS " + ParsePNodeString(minOSNode);
                            node.TryGetValue("DTPlatformVersion", out PNode targetOSNode);
                            ipaInfo.TargetSdkVersion = "iOS " + ParsePNodeString(targetOSNode);
                            node.TryGetValue("UIDeviceFamily", out PNode familyNode);
                            if (familyNode != null && familyNode.GetType() == typeof(PListNet.Nodes.ArrayNode))
                            {
                                foreach (var item in ((PListNet.Nodes.ArrayNode)familyNode))
                                {
                                    var deviceFamily = ParsePNodeString(item);
                                    iosDeviceFamily.TryGetValue(deviceFamily, out string? deviceFamilyDesc);
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
                                        iconName = ParsePNodeString(iconFilesNode, arrayLast: true);
                                    }
                                }
                            }
                            ipaInfo.Permissions.AddRange(node.Keys.Where(m => m.StartsWith("NS") && m.EndsWith("UsageDescription")).Select(m => m[2..^16]));
                        }
                        break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(iconName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name.StartsWith(iconName))
                        {
                            using Stream stream = entry.Open();
                            using MemoryStream ms = new();
                            stream.CopyTo(ms);
                            ipaIcon = ms.ToArray();
                            break;
                        }
                    }
                }
            }
            return (ipaInfo, fileSize, ipaIcon);
        }

        private (ApkInfo, long, byte[]?) ParseAndroidInfo(string apkPath)
        {
            byte[]? apkIcon = null;
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
                    if (int.TryParse(m.Value, out int iconIndex) && iconIndex > iconIndexLast)
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
                using ZipArchive archive = ZipFile.OpenRead(apkPath);
                ZipArchiveEntry? iconEntry = archive.GetEntry(iconPath);
                if (iconEntry != null)
                {
                    using Stream stream = iconEntry.Open();
                    using MemoryStream ms = new();
                    stream.CopyTo(ms);
                    apkIcon = ms.ToArray();
                }
            }
            catch (System.Exception)
            {
            }
            androidVersions.TryGetValue(apkInfo.MinSdkVersion, out string? minSdkVersion);
            androidVersions.TryGetValue(apkInfo.TargetSdkVersion, out string? targetSdkVersion);
            if (!string.IsNullOrWhiteSpace(minSdkVersion)) apkInfo.MinSdkVersion = minSdkVersion;
            if (!string.IsNullOrWhiteSpace(targetSdkVersion)) apkInfo.TargetSdkVersion = targetSdkVersion;
            return (apkInfo, fileSize, apkIcon);
        }

        public static Bitmap? GetIcnsMax(string icnsPath)
        {
            return IcnsImageParser.GetImages(icnsPath).OrderByDescending(m => m.Bitmap.Height).FirstOrDefault()?.Bitmap;
        }

        private static string ParsePNodeString(PNode node, bool arrayFirst = false, bool arrayLast = false)
        {
            if (node == null) return string.Empty;
            var nodeType = node.GetType();
            if (nodeType == typeof(PListNet.Nodes.DictionaryNode))
            {
                PListNet.Nodes.DictionaryNode value = (PListNet.Nodes.DictionaryNode)node;
                return string.Join(", ", value.Values.Select(m => ParsePNodeString(m)));
            }
            else if (nodeType == typeof(PListNet.Nodes.BooleanNode))
            {
                PListNet.Nodes.BooleanNode value = (PListNet.Nodes.BooleanNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.ArrayNode))
            {
                PListNet.Nodes.ArrayNode value = (PListNet.Nodes.ArrayNode)node;
                if (arrayFirst && value.Count > 0)
                {
                    return ParsePNodeString(value[0]);
                }
                else if (arrayLast && value.Count > 0)
                {
                    return ParsePNodeString(value[^1]);
                }
                return string.Join(", ", value.Select(m => ParsePNodeString(m)));
            }
            else if (nodeType == typeof(PListNet.Nodes.DataNode))
            {
                PListNet.Nodes.DataNode value = (PListNet.Nodes.DataNode)node;
                return Convert.ToBase64String(value.Value);
            }
            else if (nodeType == typeof(PListNet.Nodes.DateNode))
            {
                PListNet.Nodes.DateNode value = (PListNet.Nodes.DateNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.FillNode))
            {
                PListNet.Nodes.FillNode value = (PListNet.Nodes.FillNode)node;
                return value?.ToString() ?? string.Empty;
            }
            else if (nodeType == typeof(PListNet.Nodes.IntegerNode))
            {
                PListNet.Nodes.IntegerNode value = (PListNet.Nodes.IntegerNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.RealNode))
            {
                PListNet.Nodes.RealNode value = (PListNet.Nodes.RealNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.StringNode))
            {
                PListNet.Nodes.StringNode value = (PListNet.Nodes.StringNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.UidNode))
            {
                PListNet.Nodes.UidNode value = (PListNet.Nodes.UidNode)node;
                return value.Value.ToString();
            }
            else
            {
                return node?.ToString() ?? string.Empty;
            }
        }

        class MyApkInfoHandler : IApkInfoHandler<ApkInfo>
        {
            readonly IApkInfoHandler<ApkInfo>? parentHandler;
            public MyApkInfoHandler()
            {
                // Get the assembly that contains the internal type.
                Assembly assembly = Assembly.Load("ApkReader");

                // Get the type of the internal class.
                Type? type = assembly.GetType("ApkReader.ApkInfoHandler");
                if (type == null) return;
                // Create an instance of the internal class.
                parentHandler = (IApkInfoHandler<ApkInfo>?)Activator.CreateInstance(type);
            }
            public void Execute(XmlDocument androidManifest, ArscFile resources, ApkInfo apkInfo)
            {
                parentHandler?.Execute(androidManifest, resources, apkInfo);
                apkInfo.Densities.Clear();
                apkInfo.Densities.Add("Phone");
                apkInfo.Densities.Add("Pad");
                var nodes = androidManifest.DocumentElement?.ChildNodes;
                if (nodes == null) return;
                foreach (XmlNode childNode in nodes)
                {
                    if (childNode.LocalName != "uses-feature") continue;
                    string usesPermission = childNode.Attributes?.GetNamedItem("name")?.Value ?? string.Empty;
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

        private static void InjectApkInfoHandler(ApkReader<ApkInfo> apkReader)
        {
            Type type = typeof(ApkReader<ApkInfo>);
            FieldInfo? fieldInfo = type.GetField("_apkInfoHandlers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            fieldInfo?.SetValue(apkReader, new List<IApkInfoHandler<ApkInfo>>() { new MyApkInfoHandler() });
        }

        readonly Dictionary<string, string> iosDeviceFamily = new()
        {
            { "1", "iPhone" },
            { "2", "iPad" },
            { "3", "Apple TV" },
            { "4", "Apple Watch" },
            { "5", "HomePod" },
            { "6", "Mac" },
        };

        readonly Dictionary<string, string> androidVersions = new()
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

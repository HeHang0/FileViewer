using ModernWpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileViewer
{
    public static class Settings
    {
        public static List<SettingItem> PluginSetting { get; set; }

        public static ApplicationTheme? Theme { get; set; }

        public class SettingItem
        {
            public int Index { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            public string DllPath { get; set; } = string.Empty;
            public bool IsEnabled { get; set; } = true;
            public List<KeyValuePair<string, bool>> Extensions { get; set; } = new();
        }

        static Settings()
        {
            PluginSetting = ReadPlugins();
            Theme = ReadTheme();
            if (ThemeManager.Current.ApplicationTheme != Theme)
                ThemeManager.Current.ApplicationTheme = Theme;
        }

        public static bool ExtensionEnabled(string extension, string pluginName, string pluginPath)
        {
            var index = FindIndex(pluginName, pluginPath);
            if (index < 0) return true;
            var extIndex = PluginSetting[index].Extensions.FindIndex(m => m.Key == extension);
            return index >= 0 && PluginSetting[index].Extensions[extIndex].Value;
        }

        public static int FindIndex(string name, string dllPath)
        {
            return PluginSetting.FindIndex(item =>
            {
                return name == item.Name &&
                string.Equals(Path.GetFullPath(dllPath),
                Path.GetFullPath(item.DllPath),
                StringComparison.OrdinalIgnoreCase);
            });
        }

        public static void SavePlugins()
        {
            try
            {
                File.WriteAllText(AppPluginSettingPath, JsonConvert.SerializeObject(PluginSetting));
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        public static void SaveTheme()
        {
            try
            {
                File.WriteAllText(AppThemeSettingPath, Theme?.ToString() ?? string.Empty);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        static List<SettingItem> ReadPlugins()
        {
            if (File.Exists(AppPluginSettingPath))
            {
                try
                {
                    string text = File.ReadAllText(AppPluginSettingPath);
                    List<SettingItem>? result = JsonConvert.DeserializeObject<List<SettingItem>>(text);
                    if (result != null) return result;
                }
                catch (Exception)
                {
                }
            }
            return new List<SettingItem>();
        }

        static ApplicationTheme? ReadTheme()
        {
            if (File.Exists(AppThemeSettingPath))
            {
                try
                {
                    string text = File.ReadAllText(AppThemeSettingPath).ToLower();
                    return text switch
                    {
                        "dark" => (ApplicationTheme?)ApplicationTheme.Dark,
                        "light" => (ApplicationTheme?)ApplicationTheme.Light,
                        _ => null,
                    };
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        static string AppThemeSettingPath
        {
            get
            {
                return Path.Combine(AppDataPath, "theme.json");
            }
        }

        static string AppPluginSettingPath
        {
            get
            {
                return Path.Combine(AppDataPath, "plugins.json");
            }
        }

        static string AppDataPath
        {
            get
            {
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appDataPath = Path.Combine(roming, AppName);
                if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
                return appDataPath;
            }
        }

        static string AppName
        {
            get
            {
                string? appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                return string.IsNullOrWhiteSpace(appName) ? "FileViewer" : appName;
            }
        }
    }
}

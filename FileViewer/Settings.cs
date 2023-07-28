using ModernWpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace FileViewer
{
    public static class Settings
    {
        public static List<SettingItem> PluginSetting { get; set; }

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
            ThemeManager.Current.ApplicationTheme = ReadTheme();
            ThemeManager.Current.AccentColor = ReadAccentColor();
            DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(ThemeManager.ApplicationThemeProperty, ThemeManager.Current.GetType());
            desc.AddValueChanged(ThemeManager.Current, SaveTheme);
            desc = DependencyPropertyDescriptor.FromProperty(ThemeManager.AccentColorProperty, ThemeManager.Current.GetType());
            desc.AddValueChanged(ThemeManager.Current, SaveAccentColor);
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
                File.WriteAllText(AppPluginSettingPath, JsonSerializer.Serialize(PluginSetting));
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        static void SaveTheme(object? sender, EventArgs args)
        {
            try
            {
                File.WriteAllText(AppThemeSettingPath, ThemeManager.Current.ApplicationTheme?.ToString() ?? string.Empty);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        static void SaveAccentColor(object? sender, EventArgs args)
        {
            try
            {
                File.WriteAllText(AppAccentColorSettingPath, ThemeManager.Current.AccentColor?.ToString() ?? string.Empty);
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
                    List<SettingItem>? result = JsonSerializer.Deserialize<List<SettingItem>>(text);
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

        static Color? ReadAccentColor()
        {
            if (File.Exists(AppAccentColorSettingPath))
            {
                try
                {
                    string text = File.ReadAllText(AppAccentColorSettingPath);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return (Color)ColorConverter.ConvertFromString(text);
                    }
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

        static string AppAccentColorSettingPath
        {
            get
            {
                return Path.Combine(AppDataPath, "color.json");
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

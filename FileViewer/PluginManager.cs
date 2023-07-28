using FileViewer.Base;
using FileViewer.Plugins.Hello;
using FileViewer.Plugins.Universal;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer
{
    public class PluginManager : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
        readonly IPlugin universalPlugin;

        public ObservableCollection<Plugin> Plugins { get; } = new();
        private readonly HashSet<Type> _existsType = new();

        public PluginManager()
        {
            universalPlugin = new PluginUniversal();
            LoadDefaltPlugin();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadPluginFormAssembly(assembly, (assembly.FullName ?? string.Empty));
            }
            LoadPlugins(FindPlugins());
            SortPlugins();
        }

        private void LoadDefaltPlugin()
        {
            LoadPluginFormType(typeof(Plugins.App.PluginApp));
            LoadPluginFormType(typeof(Plugins.Compressed.PluginCompressed));
            LoadPluginFormType(typeof(Plugins.Image.PluginImage));
            LoadPluginFormType(typeof(Plugins.Media.PluginMedia));
            LoadPluginFormType(typeof(Plugins.MobileProvision.PluginMobileProvision));
            LoadPluginFormType(typeof(Plugins.MonacoEditor.PluginMonacoEditor));
            LoadPluginFormType(typeof(Plugins.Office.PluginOffice));
            LoadPluginFormType(typeof(Plugins.Pdf.PluginPdf));
            LoadPluginFormType(typeof(Plugins.Text.PluginText));
            LoadPluginFormType(typeof(Plugins.Browser.PluginBrowser));
            LoadPluginFormType(typeof(Plugins.Fonts.FontsControl));
        }

        public IPlugin GetPlugin(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            bool isDirectory = Directory.Exists(filePath);
            foreach (var plugin in Plugins)
            {
                if (plugin.IsEnabled &&
                    plugin.Available &&
                    plugin.IsSupported(extension, isDirectory))
                {
                    return plugin;
                }
            }
            return universalPlugin;
        }

        public IPlugin GetPlugin()
        {
            return universalPlugin;
        }

        private void SortPlugins()
        {
            List<Plugin> sortedList = Plugins.OrderBy(m =>
            {
                var index = Settings.FindIndex(m.PluginName, m.DllPath);
                if (index >= 0)
                {
                    m.IsEnabled = Settings.PluginSetting[index].IsEnabled;
                }
                return index;
            }).ToList();
            Plugins.Clear();
            foreach (var item in sortedList)
            {
                Plugins.Add(item);
            }
        }

        private void LoadPlugins(IEnumerable<string> plugins)
        {
            foreach (string dllPath in plugins)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    LoadPluginFormAssembly(assembly, dllPath);
                }
                catch (Exception)
                {
                }
            }
        }

        private void LoadPluginFormType(Type type)
        {
            LoadPluginFormAssembly(type.Assembly);
        }

        private void LoadPluginFormAssembly(Assembly? assembly, string? dllPath = "")
        {
            if (assembly == null) return;
            if (string.IsNullOrWhiteSpace(dllPath)) dllPath = assembly.FullName ?? string.Empty;
            try
            {
                Type baseType = typeof(IPlugin);
                var allTypes = assembly.GetTypes();
                var types = allTypes?.Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseType));
                if (types == null) return;
                foreach (Type type in types)
                {
                    if (_existsType.Contains(type) || type == typeof(PluginHello) || type == typeof(PluginUniversal)) continue;
                    _existsType.Add(type);
                    ConstructorInfo[] constructors = type.GetConstructors();
                    bool constructorOK = constructors.Any(ctor => ctor.IsPublic && ctor.GetParameters().Length == 0);
                    if (!constructorOK) continue;
                    object? instance = Activator.CreateInstance(type);
                    if (instance == null) continue;

                    IPlugin implementation = (IPlugin)instance;
                    var plugin = new Plugin(implementation, dllPath)
                    {
                        ProcessExternalDll = ProcessExternalDll
                    };
                    Plugins.Add(plugin);
                }
            }
            catch (Exception)
            {
            }
        }

        private static List<string> FindPlugins()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string searchPattern = "FileViewer.Plugins.*.dll";
            var pluginsDlls = new List<string>();
            var pluginsDirectory = Path.Combine(AppDataPath, "Plugins");
            try
            {
                pluginsDlls.AddRange(Directory.EnumerateFiles(currentDirectory, searchPattern));
            }
            catch (Exception)
            {
            }
            try
            {
                pluginsDlls.AddRange(Directory.EnumerateFiles(pluginsDirectory, searchPattern));
            }
            catch (Exception)
            {
            }
            return pluginsDlls;
        }

        private void ProcessExternalDll(string filePath)
        {
            try
            {
                Assembly assembly = Assembly.Load(File.ReadAllBytes(filePath));
                Type[] allTypes = assembly.GetTypes();
                Type baseType = typeof(IPlugin);
                var types = allTypes?.Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(baseType));
                if (types == null || !types.Any()) return;
                var success = false;
                var pluginsDirectory = Path.Combine(AppDataPath, "Plugins");
                if (!Directory.Exists(pluginsDirectory)) Directory.CreateDirectory(pluginsDirectory);
                var destPath = Path.Combine(pluginsDirectory, Path.GetFileName(filePath));
                foreach (Type type in types!)
                {
                    ConstructorInfo[] constructors = type.GetConstructors();
                    bool constructorOK = constructors.Any(ctor => ctor.IsPublic && ctor.GetParameters().Length == 0);
                    if (!constructorOK) continue;
                    object? instance = Activator.CreateInstance(type);
                    if (instance == null) continue;

                    IPlugin implementation = (IPlugin)instance;
                    Plugins.Insert(0, new Plugin(implementation, destPath));
                    success = true;
                }
                if (success)
                {
                    File.Copy(filePath, destPath);
                }
            }
            catch (Exception)
            {
            }
        }

        private static string AppDataPath
        {
            get
            {
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(roming, Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "FileViewer");
            }
        }

        public ICommand SaveCommand => new DelegateCommand(Save);

        private void Save()
        {
            Settings.PluginSetting.Clear();
            for (int i = 0; i < Plugins.Count; i++)
            {
                var settingItem = new Settings.SettingItem()
                {
                    Index = i,
                    Name = Plugins[i].PluginName,
                    DllPath = Plugins[i].DllPath,
                    IsEnabled = Plugins[i].IsEnabled,
                    Extensions = new List<KeyValuePair<string, bool>>()
                };
                for (int j = 0; j < Plugins[i].Extensions.Count; j++)
                {
                    var ext = Plugins[i].Extensions[j];
                    var kp = new KeyValuePair<string, bool>(ext.ExtensionName, ext.IsEnabled);
                    settingItem.Extensions.Add(kp);
                }
                Settings.PluginSetting.Add(settingItem);
            }
            Settings.SavePlugins();
        }

        public class Extension : INotifyPropertyChanged
        {
#pragma warning disable CS0067
            public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
            readonly Action<string, bool>? _action;
            public Extension(string name, bool isEnabled, Action<string, bool>? action)
            {
                _action = action;
                IsEnabled = isEnabled;
                ExtensionName = name;
            }

            public string ExtensionName { get; }
            public bool IsEnabled { get; set; }
            public ICommand SwitchEnabled => new DelegateCommand<MouseButtonEventArgs>(e =>
            {
                e.Handled = true;
                IsEnabled = !IsEnabled;
                _action?.Invoke(ExtensionName, IsEnabled);
            });
        }

        public class Plugin : IPlugin, INotifyPropertyChanged
        {
#pragma warning disable CS0067
            public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
            readonly IPlugin _iPlugin;
            readonly string _pluginPath;
            readonly Dictionary<string, bool> _extensionsMap = new();
            readonly ObservableCollection<Extension> _extensions = new();
            public Action<string>? ProcessExternalDll;
            internal Plugin(IPlugin iPlugin, string pluginPath, bool? isEnable = null)
            {
                _iPlugin = iPlugin;
                _pluginPath = pluginPath;
                IsEnabled = isEnable ?? iPlugin.Available;
                foreach (string item in iPlugin.SupportedExtensions)
                {
                    AddExtension(item, Settings.ExtensionEnabled(item, iPlugin.PluginName, _pluginPath));
                }
            }

            private void AddExtension(string name, bool isEnabled)
            {
                if (!_extensionsMap.ContainsKey(name))
                {
                    _extensionsMap.Add(name, isEnabled);
                    _extensions.Add(new Extension(name, isEnabled, ExtEnableChanged));
                }
            }

            private void ExtEnableChanged(string name, bool isEnabled)
            {
                if (!isEnabled && !_iPlugin.SupportedExtensions.Contains(name))
                {
                    _extensionsMap.Remove(name);
                    for (int i = 0; i < _extensions.Count; i++)
                    {
                        if (_extensions[i].ExtensionName == name)
                        {
                            _extensions.RemoveAt(i);
                            return;
                        }
                    }
                }
                else
                {
                    _extensionsMap[name] = isEnabled;
                }
            }

            public ICommand DropFile => new DelegateCommand<DragEventArgs>(e =>
            {
                if (!(e?.Data.GetDataPresent(DataFormats.FileDrop) ?? false))
                {
                    return;
                }
                e.Handled = true;

                if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
                foreach (string item in files)
                {
                    var name = Path.GetFileName(item).ToLower();
                    if (name.StartsWith("fileviewer.plugins.") && name.EndsWith(".dll"))
                    {
                        ProcessExternalDll?.Invoke(item);
                    }
                    else
                    {
                        AddExtension(Path.GetExtension(item).ToLower(), true);
                    }
                }
                IsExpanded = true;
            });

            public ICommand SwitchExpand => new DelegateCommand(() =>
            {
                IsExpanded = !IsExpanded;
            });

            public static ICommand DragOver => new DelegateCommand<DragEventArgs>(e =>
            {
                if (!(e?.Data.GetDataPresent(DataFormats.FileDrop) ?? false))
                {
                    return;
                }
                e.Handled = true;
            });

            public bool IsSupported(string extension, bool isDirectory)
            {
                return _extensionsMap.TryGetValue(extension, out bool enabled) && enabled && (!isDirectory || SupportedDirectory);
            }

            public bool IsEnabled { get; set; }

            public bool IsExpanded { get; set; } = false;

            public string EnabledText => IsEnabled ? "已启用" : "已停用";

            public ObservableCollection<Extension> Extensions => _extensions;

            public IEnumerable<string> SupportedExtensions => _iPlugin.SupportedExtensions;

            public bool Available => _iPlugin.Available;

            public bool SupportedDirectory => _iPlugin.SupportedDirectory;

            public string PluginName => _iPlugin.PluginName;

            public string PluginTypeText => File.Exists(_pluginPath) ? "自定义" : "内置";

            public Brush PluginTypeColor => File.Exists(_pluginPath) ? Brushes.Orange : Brushes.LawnGreen;

            public string DllPath => _pluginPath;

            public string Description => _iPlugin.Description;

            public ImageSource? Icon => _iPlugin.Icon ?? Utils.GetBitmapSource(Utils.ImagePreview);

            public void ChangeFile(string filePath)
            {
                _iPlugin.ChangeFile(filePath);
            }

            public void ChangeTheme(bool dark)
            {
                _iPlugin.ChangeTheme(dark);
            }

            public void Cancel()
            {
                _iPlugin.Cancel();
            }

            public UserControl GetUserControl(IManager manager)
            {
                return _iPlugin.GetUserControl(manager);
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace FileViewer.Monitor
{
    public class ThemeListener
    {
        public delegate void ThemeChangedEventHandler(bool isDark);
        public event ThemeChangedEventHandler ThemeChanged;
        public ThemeListener()
        {
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
        }

        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                ThemeChanged?.Invoke(IsDarkMode());
            }
        }

        private const string _registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string _registryValueName = "SystemUsesLightTheme";


        public static bool IsDarkMode()
        {
            object registryValueObject;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(_registryKeyPath))
            {
                registryValueObject = key?.GetValue(_registryValueName);
                if (registryValueObject != null)
                {
                    int registryValue = (int)registryValueObject;
                    return registryValue <= 0;
                }
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(_registryKeyPath))
            {
                registryValueObject = key?.GetValue(_registryValueName);
                if (registryValueObject != null)
                {
                    int registryValue = (int)registryValueObject;
                    return registryValue <= 0;
                }
            }
            return false;
        }
    }
}

using Microsoft.Win32;
using System;

namespace NetworkWatcherExtension
{
    public static class ProxyHelper
    {
        private const string RegistryPath =
            @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

        public static void SetSystemProxy(string ip, int port)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue("ProxyEnable", 1);
                        key.SetValue("ProxyServer", $"{ip}:{port}");
                        key.SetValue("ProxyOverride", "<local>");
                    }
                }

                // Notify Windows that proxy settings changed
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set system proxy: {ex.Message}", ex);
            }
        }

        public static void UnsetSystemProxy()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue("ProxyEnable", 0);
                        key.DeleteValue("ProxyServer", false);
                    }
                }

                // Notify Windows that proxy settings changed
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to unset system proxy: {ex.Message}", ex);
            }
        }

        // WinInet API imports to notify system of proxy changes
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        [System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(
            IntPtr hInternet,
            int dwOption,
            IntPtr lpBuffer,
            int dwBufferLength);
    }
}

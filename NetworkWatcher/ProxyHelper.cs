using Microsoft.Win32;

public static class ProxyHelper
{
    private const string RegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    public static void SetProxy(string ip, int port)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);

        key.SetValue("ProxyEnable", 1);
        key.SetValue("ProxyServer", $"{ip}:{port}");
        key.SetValue("ProxyOverride", "<local>");
    }

    public static void UnsetProxy()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);

        key.SetValue("ProxyEnable", 0);
    }
}
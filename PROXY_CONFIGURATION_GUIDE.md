# Configuring Your Application to Use Network Watcher

## Method 1: HttpClient with Proxy (Recommended)

```csharp
using System;
using System.Net;
using System.Net.Http;

// Create HttpClientHandler with proxy configuration
var handler = new HttpClientHandler
{
    Proxy = new WebProxy("http://127.0.0.1:8888"),
    UseProxy = true,
    
    // Optional: Disable SSL validation for debugging
    ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

// Create HttpClient with the handler
using var client = new HttpClient(handler);

// Make requests - they will go through Network Watcher
var response = await client.GetAsync("https://api.example.com/data");
var content = await response.Content.ReadAsStringAsync();
```

## Method 2: WebRequest with Proxy

```csharp
using System;
using System.Net;

// Set default proxy for all WebRequest calls
WebRequest.DefaultWebProxy = new WebProxy("http://127.0.0.1:8888");

// Make request
var request = WebRequest.Create("https://api.example.com/data");
using var response = request.GetResponse();
using var stream = response.GetResponseStream();
using var reader = new StreamReader(stream);
var content = reader.ReadToEnd();
```

## Method 3: System-wide Proxy (Windows)

You can set the system proxy, which will affect all applications:

```csharp
using Microsoft.Win32;

public static class ProxyHelper
{
    private const string RegistryPath = 
        @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    public static void SetSystemProxy()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
        key.SetValue("ProxyEnable", 1);
        key.SetValue("ProxyServer", "127.0.0.1:8888");
        key.SetValue("ProxyOverride", "<local>");
    }

    public static void UnsetSystemProxy()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
        key.SetValue("ProxyEnable", 0);
    }
}

// Usage
ProxyHelper.SetSystemProxy();
// ... your application code ...
ProxyHelper.UnsetSystemProxy();
```

## Method 4: ASP.NET Core Development Server

Add this to your `Program.cs` or `Startup.cs`:

```csharp
// In Program.cs (ASP.NET Core 6+)
var builder = WebApplication.CreateBuilder(args);

// Configure HttpClient with proxy
builder.Services.AddHttpClient("default")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        Proxy = new WebProxy("http://127.0.0.1:8888"),
        UseProxy = true,
        ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
```

## Method 5: Environment Variables

Set these environment variables before launching your application:

**Windows (PowerShell):**
```powershell
$env:HTTP_PROXY = "http://127.0.0.1:8888"
$env:HTTPS_PROXY = "http://127.0.0.1:8888"
```

**Windows (Command Prompt):**
```cmd
set HTTP_PROXY=http://127.0.0.1:8888
set HTTPS_PROXY=http://127.0.0.1:8888
```

**Linux/Mac:**
```bash
export HTTP_PROXY=http://127.0.0.1:8888
export HTTPS_PROXY=http://127.0.0.1:8888
```

## Testing with cURL

```bash
curl -x http://127.0.0.1:8888 https://api.example.com/data
```

## Testing with Browser

Configure your browser to use manual proxy configuration:
- HTTP Proxy: `127.0.0.1`
- Port: `8888`
- HTTPS Proxy: `127.0.0.1`
- Port: `8888`

## Important Notes

1. **HTTPS Certificate**: For HTTPS inspection, the Network Watcher proxy installs a root certificate. Your application may need to trust this certificate.

2. **Localhost Bypass**: By default, the proxy configuration includes `<local>` in the bypass list, meaning requests to `localhost` or `127.0.0.1` won't go through the proxy.

3. **Certificate Validation**: In production, never disable certificate validation. Only disable it for debugging with Network Watcher.

4. **Performance**: Using a proxy adds latency. Remember to disable it when not debugging network traffic.

## Example: Complete Console Application

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Network Watcher...");
        Console.WriteLine("Make sure Network Watcher proxy is running!");
        Console.WriteLine();

        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy("http://127.0.0.1:8888"),
            UseProxy = true,
            ServerCertificateCustomValidationCallback = 
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var client = new HttpClient(handler);

        try
        {
            // Test GET request
            Console.WriteLine("Making GET request...");
            var getResponse = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            var getContent = await getResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {getResponse.StatusCode}");
            Console.WriteLine($"Content: {getContent.Substring(0, Math.Min(100, getContent.Length))}...");
            Console.WriteLine();

            // Test POST request with JSON
            Console.WriteLine("Making POST request...");
            var postContent = new StringContent(
                "{\"title\":\"Test\",\"body\":\"Testing Network Watcher\",\"userId\":1}",
                System.Text.Encoding.UTF8,
                "application/json");
            
            var postResponse = await client.PostAsync(
                "https://jsonplaceholder.typicode.com/posts",
                postContent);
            var postResult = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {postResponse.StatusCode}");
            Console.WriteLine($"Content: {postResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Is Network Watcher running?");
        }

        Console.WriteLine();
        Console.WriteLine("Check Network Watcher window in Visual Studio!");
        Console.ReadLine();
    }
}
```

## Quick Start Checklist

1. ☐ Start Visual Studio with Network Watcher extension
2. ☐ Open Network Watcher window (View → Other Windows → Network Watcher)
3. ☐ Click "Start Proxy"
4. ☐ Configure your application to use proxy `127.0.0.1:8888`
5. ☐ Run your application
6. ☐ Watch traffic appear in Network Watcher!
7. ☐ Use filters to find specific requests
8. ☐ Click requests to view details and prettified JSON

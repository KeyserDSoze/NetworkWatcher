using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Helpers;

class Program
{
    static ProxyServer proxy;
    static string logFile;
    static string domainFilter;
    static readonly object fileLock = new object();

    static async Task Main(string[] args)
    {
        domainFilter = ParseArgs(args);

        if (string.IsNullOrWhiteSpace(domainFilter))
        {
            Console.WriteLine("========================================");
            Console.WriteLine(" NetworkWatcher");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Enter domain to monitor.");
            Console.WriteLine("Leave empty to monitor ALL traffic.");
            Console.WriteLine();
            Console.Write("Domain: ");

            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                domainFilter = "all";
            else
                domainFilter = input.ToLower();
        }

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine(" NetworkWatcher started");
        Console.WriteLine("========================================");

        Console.WriteLine($" Domain filter : {domainFilter}");
        Console.WriteLine();

        Directory.CreateDirectory("logs");

        logFile = Path.Combine("logs",
            $"traffic_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

        Console.WriteLine($" Log file      : {logFile}");
        Console.WriteLine();

        AppDomain.CurrentDomain.ProcessExit += (_, __) => Shutdown();
        Console.CancelKeyPress += (_, __) => Shutdown();

        try
        {
            proxy = new ProxyServer();

            proxy.CertificateManager.CreateRootCertificate(false);
            proxy.CertificateManager.TrustRootCertificate(true);

            proxy.BeforeRequest += OnRequest;
            proxy.BeforeResponse += OnResponse;

            var endpoint = new ExplicitProxyEndPoint(
                System.Net.IPAddress.Any,
                8888,
                true);

            proxy.AddEndPoint(endpoint);
            proxy.Start();

            ProxyHelper.SetProxy("127.0.0.1", 8888);

            Console.WriteLine(" Proxy is active");
            Console.WriteLine(" Press ENTER to stop...");
            Console.WriteLine();

            Console.ReadLine();
        }
        finally
        {
            Shutdown();
        }
    }

    static string ParseArgs(string[] args)
    {
        if (args == null || args.Length == 0)
            return null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--domain" && i + 1 < args.Length)
                return args[i + 1].ToLower();
        }

        return null;
    }

    static bool ShouldLog(string url)
    {
        if (domainFilter == "all")
            return true;

        return url.Contains(domainFilter,
            StringComparison.OrdinalIgnoreCase);
    }

    static async Task OnRequest(object sender, SessionEventArgs e)
    {
        var request = e.HttpClient.Request;

        if (!ShouldLog(request.Url))
            return;

        var sb = new StringBuilder();

        sb.AppendLine("==================================================================");
        sb.AppendLine($"Timestamp : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("==================================================================");

        sb.AppendLine("REQUEST");
        sb.AppendLine("------------------------------------------------------------------");

        sb.AppendLine($"{request.Method} {request.Url}");
        sb.AppendLine();

        sb.AppendLine("Headers:");
        foreach (var h in request.Headers)
            sb.AppendLine($"{h.Name}: {h.Value}");

        if (request.HasBody)
        {
            var body = await e.GetRequestBodyAsString();

            sb.AppendLine();
            sb.AppendLine("Body:");
            sb.AppendLine(body);
        }

        WriteLog(sb.ToString(), ConsoleColor.Cyan);
    }

    static async Task OnResponse(object sender, SessionEventArgs e)
    {
        var request = e.HttpClient.Request;
        var response = e.HttpClient.Response;

        if (!ShouldLog(request.Url))
            return;

        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("RESPONSE");
        sb.AppendLine("------------------------------------------------------------------");

        sb.AppendLine($"Status : {(int)response.StatusCode} {response.StatusCode}");
        sb.AppendLine();

        sb.AppendLine("Headers:");
        foreach (var h in response.Headers)
            sb.AppendLine($"{h.Name}: {h.Value}");

        if (response.HasBody)
        {
            var body = await e.GetResponseBodyAsString();

            sb.AppendLine();
            sb.AppendLine("Body:");
            sb.AppendLine(body);
        }

        sb.AppendLine("==================================================================");
        sb.AppendLine();

        WriteLog(sb.ToString(), ConsoleColor.Green);
    }

    static void WriteLog(string text, ConsoleColor color)
    {
        lock (fileLock)
        {
            File.AppendAllText(logFile, text);

            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    static void Shutdown()
    {
        try
        {
            Console.WriteLine();
            Console.WriteLine("Shutting down proxy...");

            ProxyHelper.UnsetProxy();
            proxy?.Stop();

            Console.WriteLine("Proxy disabled.");
            Console.WriteLine("NetworkWatcher stopped.");
        }
        catch { }
    }
}
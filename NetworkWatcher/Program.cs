using System;
using System.IO;
using System.Text;
using System.Text.Json;
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
    static bool prettyPrintJson;
    static readonly object fileLock = new object();

    static async Task Main(string[] args)
    {
        var parsedArgs = ParseArgs(args);
        domainFilter = parsedArgs.domain;
        prettyPrintJson = parsedArgs.prettify;

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

        if (!parsedArgs.prettifySpecified)
        {
            Console.WriteLine();
            Console.Write("Enable JSON prettify and colorization? (Y/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();
            prettyPrintJson = string.IsNullOrWhiteSpace(response) || response == "y" || response == "yes";
        }

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine(" NetworkWatcher started");
        Console.WriteLine("========================================");

        Console.WriteLine($" Domain filter : {domainFilter}");
        Console.WriteLine($" JSON prettify : {(prettyPrintJson ? "enabled" : "disabled")}");
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

    static (string domain, bool prettify, bool prettifySpecified) ParseArgs(string[] args)
    {
        string domain = null;
        bool prettify = false;
        bool prettifySpecified = false;

        if (args == null || args.Length == 0)
            return (domain, prettify, prettifySpecified);

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--domain" && i + 1 < args.Length)
            {
                domain = args[i + 1].ToLower();
            }
            else if (args[i] == "-p" || args[i] == "--prettify")
            {
                prettify = true;
                prettifySpecified = true;
            }
        }

        return (domain, prettify, prettifySpecified);
    }

    static bool ShouldLog(string url)
    {
        if (domainFilter == "all")
            return true;

        return url.Contains(domainFilter,
            StringComparison.OrdinalIgnoreCase);
    }

    static bool TryParseJson(string text, out JsonDocument document)
    {
        document = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            document = JsonDocument.Parse(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    static string FormatJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch
        {
            return json;
        }
    }

    static void WriteJsonToConsole(string json)
    {
        int i = 0;
        while (i < json.Length)
        {
            char c = json[i];

            if (c == '{' || c == '}' || c == '[' || c == ']' || c == ',' || c == ':')
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(c);
            }
            else if (c == '"')
            {
                // String value or key
                int start = i;
                i++;
                while (i < json.Length && json[i] != '"')
                {
                    if (json[i] == '\\') i++; // Skip escaped characters
                    i++;
                }

                string content = json.Substring(start, i - start + 1);

                // Check if it's a key (followed by ':') or value
                int nextNonWhitespace = i + 1;
                while (nextNonWhitespace < json.Length && char.IsWhiteSpace(json[nextNonWhitespace]))
                    nextNonWhitespace++;

                if (nextNonWhitespace < json.Length && json[nextNonWhitespace] == ':')
                {
                    // It's a key
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    // It's a string value
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.Write(content);
            }
            else if (char.IsDigit(c) || c == '-' || c == '+' || c == '.')
            {
                // Number
                int start = i;
                while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '.' || json[i] == '-' || json[i] == '+' || json[i] == 'e' || json[i] == 'E'))
                    i++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(json.Substring(start, i - start));
                i--;
            }
            else if (i + 4 <= json.Length && json.Substring(i, 4) == "true")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("true");
                i += 3;
            }
            else if (i + 5 <= json.Length && json.Substring(i, 5) == "false")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("false");
                i += 4;
            }
            else if (i + 4 <= json.Length && json.Substring(i, 4) == "null")
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("null");
                i += 3;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(c);
            }

            i++;
        }
        Console.ResetColor();
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

        string bodyText = null;
        bool isJson = false;

        if (request.HasBody)
        {
            bodyText = await e.GetRequestBodyAsString();

            if (prettyPrintJson && TryParseJson(bodyText, out var jsonDoc))
            {
                using (jsonDoc)
                {
                    isJson = true;
                    bodyText = FormatJson(bodyText);
                }
            }

            sb.AppendLine();
            sb.AppendLine("Body:");
            sb.AppendLine(bodyText);
        }

        // Write to file (no colors)
        lock (fileLock)
        {
            File.AppendAllText(logFile, sb.ToString());
        }

        // Write to console (with colors)
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================================");
        Console.WriteLine($"Timestamp : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("==================================================================");
        Console.WriteLine();
        Console.WriteLine("REQUEST");
        Console.WriteLine("------------------------------------------------------------------");
        Console.WriteLine($"{request.Method} {request.Url}");
        Console.WriteLine();
        Console.WriteLine("Headers:");
        foreach (var h in request.Headers)
            Console.WriteLine($"{h.Name}: {h.Value}");

        if (request.HasBody && bodyText != null)
        {
            Console.WriteLine();
            Console.WriteLine("Body:");

            if (isJson)
            {
                WriteJsonToConsole(bodyText);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(bodyText);
            }
        }

        Console.ResetColor();
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

        string bodyText = null;
        bool isJson = false;

        if (response.HasBody)
        {
            bodyText = await e.GetResponseBodyAsString();

            if (prettyPrintJson && TryParseJson(bodyText, out var jsonDoc))
            {
                using (jsonDoc)
                {
                    isJson = true;
                    bodyText = FormatJson(bodyText);
                }
            }

            sb.AppendLine();
            sb.AppendLine("Body:");
            sb.AppendLine(bodyText);
        }

        sb.AppendLine("==================================================================");
        sb.AppendLine();

        // Write to file (no colors)
        lock (fileLock)
        {
            File.AppendAllText(logFile, sb.ToString());
        }

        // Write to console (with colors)
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine("RESPONSE");
        Console.WriteLine("------------------------------------------------------------------");
        Console.WriteLine($"Status : {(int)response.StatusCode} {response.StatusCode}");
        Console.WriteLine();
        Console.WriteLine("Headers:");
        foreach (var h in response.Headers)
            Console.WriteLine($"{h.Name}: {h.Value}");

        if (response.HasBody && bodyText != null)
        {
            Console.WriteLine();
            Console.WriteLine("Body:");

            if (isJson)
            {
                WriteJsonToConsole(bodyText);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(bodyText);
            }
        }

        Console.WriteLine("==================================================================");
        Console.WriteLine();
        Console.ResetColor();
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
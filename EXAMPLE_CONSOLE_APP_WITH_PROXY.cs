using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine();
            Console.WriteLine("Configuring proxy to 127.0.0.1:8888...");
            Console.WriteLine();

            // Configure HttpClient to use the Network Watcher proxy
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy("http://127.0.0.1:8888"),
                UseProxy = true,
                // Accept all SSL certificates for debugging
                ServerCertificateCustomValidationCallback = 
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);

            try
            {
                Console.WriteLine("Sending request...");
                var response = await client.GetAsync("https://httpbin.org/get");
                
                Console.WriteLine($"Status: {(int)response.StatusCode}");
                Console.WriteLine("Response body:");
                
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Make sure Network Watcher proxy is running!");
            }

            Console.WriteLine();
            Console.WriteLine("Check Network Watcher window for captured traffic!");
            Console.ReadLine();
        }
    }
}

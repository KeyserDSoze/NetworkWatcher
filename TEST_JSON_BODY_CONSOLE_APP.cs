using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("  Network Watcher Test - JSON Body Test");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("Make sure Network Watcher is running!");
            Console.WriteLine("Press ENTER to start tests...");
            Console.ReadLine();

            using var client = new HttpClient();

            // Test 1: Simple GET with JSON response
            Console.WriteLine("\n1. Testing GET with JSON response...");
            await TestGet(client, "https://jsonplaceholder.typicode.com/posts/1");

            // Test 2: POST with JSON request and response
            Console.WriteLine("\n2. Testing POST with JSON request body...");
            await TestPost(client);

            // Test 3: GET httpbin (shows request details)
            Console.WriteLine("\n3. Testing HTTPBin (shows request info)...");
            await TestGet(client, "https://httpbin.org/json");

            // Test 4: Large JSON response
            Console.WriteLine("\n4. Testing with multiple items (array)...");
            await TestGet(client, "https://jsonplaceholder.typicode.com/posts");

            Console.WriteLine("\n==============================================");
            Console.WriteLine("  All tests completed!");
            Console.WriteLine("  Check Network Watcher for:");
            Console.WriteLine("  - Request bodies (for POST)");
            Console.WriteLine("  - Response bodies (for all)");
            Console.WriteLine("  - JSON should be prettified!");
            Console.WriteLine("==============================================");
            Console.ReadLine();
        }

        static async Task TestGet(HttpClient client, string url)
        {
            try
            {
                Console.WriteLine($"   GET {url}");
                var response = await client.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"   Status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"   Body length: {body.Length} chars");
                Console.WriteLine($"   Body preview: {body.Substring(0, Math.Min(80, body.Length))}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ERROR: {ex.Message}");
            }
        }

        static async Task TestPost(HttpClient client)
        {
            try
            {
                var requestData = new
                {
                    title = "Test Post from Network Watcher",
                    body = "This is a test to verify request/response body capture",
                    userId = 999,
                    metadata = new
                    {
                        timestamp = DateTime.Now.ToString("o"),
                        source = "NetworkWatcher Test App"
                    }
                };

                var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                Console.WriteLine($"   POST https://jsonplaceholder.typicode.com/posts");
                Console.WriteLine($"   Request body:\n{json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://jsonplaceholder.typicode.com/posts", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"   Status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"   Response body: {responseBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ERROR: {ex.Message}");
            }
        }
    }
}

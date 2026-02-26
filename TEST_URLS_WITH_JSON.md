# URL di Test per Network Watcher - JSON APIs

Ecco alcune API pubbliche che restituiscono JSON, perfette per testare Network Watcher:

## 🎯 API Semplici e Veloci

### JSONPlaceholder (Fake REST API)
```
https://jsonplaceholder.typicode.com/posts/1
https://jsonplaceholder.typicode.com/users/1
https://jsonplaceholder.typicode.com/comments?postId=1
```
- Risposta: Dati fake per test (post, user, comment)
- Perfetto per: Test rapidi

### HTTPBin (HTTP Request & Response Service)
```
https://httpbin.org/json
https://httpbin.org/get
https://httpbin.org/user-agent
```
- Risposta: Informazioni sulla richiesta HTTP
- Perfetto per: Vedere headers, user-agent, etc.

### DummyJSON (Fake REST API)
```
https://dummyjson.com/products/1
https://dummyjson.com/users/1
https://dummyjson.com/posts/1
```
- Risposta: Dati fake realistici (prodotti, utenti, post)
- Perfetto per: Test con dati strutturati

## 🚀 API Reali

### GitHub API (Public)
```
https://api.github.com/users/github
https://api.github.com/repos/microsoft/vscode
https://api.github.com/emojis
```
- Risposta: Dati reali di GitHub
- Perfetto per: Test con dati reali

### REST Countries
```
https://restcountries.com/v3.1/name/italy
https://restcountries.com/v3.1/all?fields=name,capital
```
- Risposta: Informazioni su paesi
- Perfetto per: JSON complessi

### OpenWeatherMap (richiede API key gratuita)
```
https://api.openweathermap.org/data/2.5/weather?q=London&appid=YOUR_API_KEY
```

### CoinGecko (Crypto prices)
```
https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd
https://api.coingecko.com/api/v3/coins/bitcoin
```
- Risposta: Prezzi crypto in tempo reale
- Perfetto per: Dati dinamici

## 📝 Esempio Console App

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new HttpClient();
        
        Console.WriteLine("Testing Network Watcher with JSON APIs...\n");

        // Test 1: JSONPlaceholder
        Console.WriteLine("1. JSONPlaceholder - Fake Post");
        var post = await client.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");
        Console.WriteLine($"Response: {post.Substring(0, Math.Min(100, post.Length))}...\n");

        // Test 2: HTTPBin
        Console.WriteLine("2. HTTPBin - JSON Response");
        var httpbin = await client.GetStringAsync("https://httpbin.org/json");
        Console.WriteLine($"Response: {httpbin.Substring(0, Math.Min(100, httpbin.Length))}...\n");

        // Test 3: DummyJSON
        Console.WriteLine("3. DummyJSON - Product");
        var product = await client.GetStringAsync("https://dummyjson.com/products/1");
        Console.WriteLine($"Response: {product.Substring(0, Math.Min(100, product.Length))}...\n");

        // Test 4: GitHub API
        Console.WriteLine("4. GitHub API - User Info");
        client.DefaultRequestHeaders.Add("User-Agent", "NetworkWatcher-Test");
        var github = await client.GetStringAsync("https://api.github.com/users/github");
        Console.WriteLine($"Response: {github.Substring(0, Math.Min(100, github.Length))}...\n");

        Console.WriteLine("Done! Check Network Watcher for captured requests.");
        Console.ReadLine();
    }
}
```

## 🧪 Test POST con JSON

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new HttpClient();
        
        // Prepare JSON data
        var newPost = new
        {
            title = "Test from Network Watcher",
            body = "This is a test post to see JSON in Network Watcher",
            userId = 1
        };

        var json = JsonSerializer.Serialize(newPost);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine("Sending POST request with JSON...");
        var response = await client.PostAsync("https://jsonplaceholder.typicode.com/posts", content);
        
        Console.WriteLine($"Status: {response.StatusCode}");
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response: {responseBody}");
        
        Console.WriteLine("\nCheck Network Watcher:");
        Console.WriteLine("- Request Body should show prettified JSON");
        Console.WriteLine("- Response Body should show prettified JSON");
        
        Console.ReadLine();
    }
}
```

## 🎨 Cosa Vedrai in Network Watcher

Quando fai richieste a queste API, vedrai:

1. **Request Headers** - User-Agent, Accept, etc.
2. **Request Body** (per POST) - JSON formattato e indentato
3. **Response Headers** - Content-Type: application/json
4. **Response Body** - JSON formattato automaticamente con:
   - Indentazione
   - Colori (se implementato)
   - Facile lettura

## 💡 Suggerimenti

- Usa **httpbin.org** per vedere cosa invia la tua app
- Usa **jsonplaceholder.typicode.com** per test POST/PUT/DELETE
- Usa **GitHub API** per vedere JSON reali e complessi
- Filtra per dominio: scrivi "httpbin" nel filtro Domain/URL per vedere solo quelle richieste

## ⚠️ Note

- Alcune API hanno rate limiting
- GitHub API richiede User-Agent header
- OpenWeatherMap richiede API key (gratuita)
- Tutte queste API supportano HTTPS

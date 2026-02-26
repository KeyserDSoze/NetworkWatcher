# Fix: Add Newtonsoft.Json Package

## The Problem

You're getting this error:
```
[Error reading body: Could not load file or assembly 'System.Text.Json, Version=10.0.0.3...
```

This happens because the extension project targets **.NET Framework 4.7.2**, but `System.Text.Json` is from .NET 10 and is not compatible.

## The Solution

Use **Newtonsoft.Json** (Json.NET) which is the standard JSON library for .NET Framework.

## Steps to Fix

### 1. Add NuGet Package

**Option A: Using Package Manager Console**
```powershell
Install-Package Newtonsoft.Json -ProjectName NetworkWatcherExtension
```

**Option B: Using .NET CLI**
```bash
cd NetworkWatcherExtension
dotnet add package Newtonsoft.Json
```

**Option C: Using NuGet Package Manager UI**
1. Right-click on **NetworkWatcherExtension** project
2. Select **Manage NuGet Packages...**
3. Click **Browse** tab
4. Search for `Newtonsoft.Json`
5. Click **Install**

### 2. Verify Installation

Check that `NetworkWatcherExtension.csproj` now includes:
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### 3. Rebuild

1. **Clean Solution** (Build → Clean Solution)
2. **Rebuild Solution** (Build → Rebuild Solution)
3. **Test** (Press F5)

## What Changed in Code

### Before (System.Text.Json - .NET 10):
```csharp
using System.Text.Json;

var jsonDoc = JsonDocument.Parse(content);
return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});
```

### After (Newtonsoft.Json - .NET Framework 4.7.2):
```csharp
using Newtonsoft.Json;

var jsonObject = JsonConvert.DeserializeObject(content);
return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
```

## Why Newtonsoft.Json?

✅ **Fully compatible** with .NET Framework 4.7.2  
✅ **Battle-tested** and widely used  
✅ **Rich feature set** for JSON manipulation  
✅ **Excellent performance**  
✅ **De-facto standard** for .NET Framework  

## Compatibility Table

| Library | .NET Framework 4.7.2 | .NET 10 |
|---------|---------------------|---------|
| Newtonsoft.Json | ✅ Yes | ✅ Yes |
| System.Text.Json | ❌ No (limited) | ✅ Yes |

## Testing After Fix

1. Start the extension (F5)
2. Open Network Watcher
3. Start Proxy
4. Make a request with JSON:
   ```csharp
   var client = new HttpClient();
   var response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
   ```
5. Check the Response Body tab
6. You should see **prettified JSON** without errors!

## Expected Result

**Before Fix:**
```
[Error reading body: Could not load file or assembly 'System.Text.Json'...]
```

**After Fix:**
```json
{
  "userId": 1,
  "id": 1,
  "title": "sunt aut facere repellat provident...",
  "body": "quia et suscipit..."
}
```

## Quick Install Command

Run this in **Package Manager Console** (Tools → NuGet Package Manager → Package Manager Console):

```powershell
Install-Package Newtonsoft.Json -ProjectName NetworkWatcherExtension -Version 13.0.3
```

Then rebuild and test! 🚀

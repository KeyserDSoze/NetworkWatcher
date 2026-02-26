# Quick Guide to Add Icon to Extension

## Step 1: Generate PNG Icons

Run this PowerShell script from the `NetworkWatcherExtension\Resources` folder:

```powershell
.\Generate-Icons.ps1
```

Or convert `Icon.svg` manually to PNG (128x128, 32x32, 16x16).

## Step 2: Update the Project File

Add these lines to `NetworkWatcherExtension.csproj` inside an `<ItemGroup>`:

```xml
<ItemGroup>
  <Content Include="Resources\Icon_128.png">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    <IncludeInVSIX>true</IncludeInVSIX>
  </Content>
  <Content Include="Resources\Icon_32.png">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    <IncludeInVSIX>true</IncludeInVSIX>
  </Content>
  <Content Include="Resources\Icon_16.png">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    <IncludeInVSIX>true</IncludeInVSIX>
  </Content>
</ItemGroup>
```

## Step 3: Update source.extension.vsixmanifest

Find the `<Metadata>` section and update:

```xml
<Identity Id="NetworkWatcherExtension.3b271ed4-b286-41eb-98c2-49cd3fa520d1" 
          Version="1.0" 
          Language="en-US" 
          Publisher="Alessandro Rapiti" />
<DisplayName>Network Watcher</DisplayName>
<Description>Monitor HTTP/HTTPS network traffic during debugging with filtering and JSON prettification support.</Description>
<Icon>Resources\Icon_128.png</Icon>
```

## Step 4: Update NetworkWatcherToolWindow.cs

Add icon to the tool window:

```csharp
public NetworkWatcherToolWindow() : base(null)
{
    this.Caption = "Network Watcher";
    
    // Set the tool window icon
    this.BitmapResourceID = 301; // or use BitmapImageMoniker
    
    this.Content = new NetworkWatcherToolWindowControl();
}
```

Or use the modern way with ImageMoniker:

```csharp
using Microsoft.VisualStudio.Imaging;

public NetworkWatcherToolWindow() : base(null)
{
    this.Caption = "Network Watcher";
    
    // Use built-in VS icon or custom one
    this.BitmapImageMoniker = KnownMonikers.StatusInformation; // Temporary
    // TODO: Replace with custom icon moniker
    
    this.Content = new NetworkWatcherToolWindowControl();
}
```

## Step 5: Rebuild and Test

1. Rebuild the extension
2. Press F5 to test
3. The icon should appear in:
   - Extension Manager
   - Tool window tab
   - Menu item (if configured)

## Temporary Solution (Use Built-in Icon)

If you don't have PNG icons yet, use a built-in Visual Studio icon temporarily:

```csharp
this.BitmapImageMoniker = KnownMonikers.StatusInformation;
// or
this.BitmapImageMoniker = KnownMonikers.Connect;
// or
this.BitmapImageMoniker = KnownMonikers.Web;
```

Available in: `Microsoft.VisualStudio.Imaging.KnownMonikers`

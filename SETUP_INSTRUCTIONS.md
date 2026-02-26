# Manual Steps to Complete Network Watcher Extension Setup

Since the solution is currently open in Visual Studio, you'll need to manually update the project file. Follow these steps:

## 1. Close Visual Studio

Close the current Visual Studio instance to allow editing of the project file.

## 2. Update NetworkWatcherExtension.csproj

Open `NetworkWatcherExtension\NetworkWatcherExtension.csproj` in a text editor and replace the `<ItemGroup>` sections with the following:

```xml
  <ItemGroup>
    <Compile Include="NetworkWatcherToolWindow.cs" />
    <Compile Include="NetworkWatcherToolWindowCommand.cs" />
    <Compile Include="NetworkWatcherToolWindowControl.xaml.cs">
      <DependentUpon>NetworkWatcherToolWindowControl.xaml</DependentUpon>
    </Compile>
    <Compile Include="Properties\AssemblyInfo.cs" />
    <Compile Include="NetworkWatcherExtensionPackage.cs" />
  </ItemGroup>
  <ItemGroup>
    <None Include="source.extension.vsixmanifest">
      <SubType>Designer</SubType>
    </None>
  </ItemGroup>
  <ItemGroup>
    <Page Include="NetworkWatcherToolWindowControl.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
  </ItemGroup>
  <ItemGroup>
    <VSCTCompile Include="NetworkWatcherExtensionPackage.vsct">
      <ResourceName>Menus.ctmenu</ResourceName>
    </VSCTCompile>
  </ItemGroup>
  <ItemGroup>
    <Reference Include="PresentationCore" />
    <Reference Include="PresentationFramework" />
    <Reference Include="System" />
    <Reference Include="System.Design" />
    <Reference Include="System.Xaml" />
    <Reference Include="WindowsBase" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.VisualStudio.SDK" Version="17.0.32112.339" ExcludeAssets="runtime" NoWarn="NU1604" />
    <PackageReference Include="Microsoft.VSSDK.BuildTools" Version="17.14.2120" NoWarn="NU1604" />
    <PackageReference Include="Titanium.Web.Proxy" Version="3.1.1417" />
  </ItemGroup>
```

## 3. Reopen Solution

Open the solution in Visual Studio again.

## 4. Restore NuGet Packages

Right-click on the solution and select "Restore NuGet Packages" to download Titanium.Web.Proxy.

## 5. Build the Project

Build the extension project to ensure everything compiles correctly.

## 6. Test the Extension

Press F5 to launch the experimental instance of Visual Studio with your extension loaded.

## 7. Open the Network Watcher Window

In the experimental instance:
- Go to **View → Other Windows → Network Watcher**
- Click **Start Proxy**
- Configure a test application to use proxy `127.0.0.1:8888`
- Watch the traffic appear in the window!

## Files Created

The following files have been created for your extension:

1. **NetworkWatcherToolWindow.cs** - Tool window registration
2. **NetworkWatcherToolWindowControl.xaml** - UI layout with filters and traffic list
3. **NetworkWatcherToolWindowControl.xaml.cs** - Logic for proxy, filtering, and JSON formatting
4. **NetworkWatcherToolWindowCommand.cs** - Command to open the tool window
5. **NetworkWatcherExtensionPackage.vsct** - Visual Studio command table (menu integration)
6. **README.md** - Extension documentation

## Features Implemented

✅ Start/Stop proxy functionality  
✅ Domain/URL filtering (case-insensitive)  
✅ Protocol filtering (HTTP/HTTPS/All)  
✅ Request list with timestamp, method, protocol, URL, and status  
✅ Detailed view with tabs for request/response headers and body  
✅ Automatic JSON prettification based on Content-Type  
✅ Integrated Visual Studio tool window  
✅ Certificate management for HTTPS inspection  

## Notes

- The proxy runs on `localhost:8888`
- Administrator privileges may be required for certificate installation
- JSON is automatically formatted when Content-Type contains "json"
- The tool window can be docked anywhere in Visual Studio

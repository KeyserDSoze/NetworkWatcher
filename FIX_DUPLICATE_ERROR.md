# IMPORTANTE: Correzione Errore RG1000

## Problema
Il file `NetworkWatcherExtension.csproj` contiene **elementi duplicati**, causando l'errore:
```
error RG1000: Unknown build error, 'An item with the same key has already been added.'
```

## Soluzione

**Chiudi Visual Studio completamente**, poi sostituisci il contenuto di `NetworkWatcherExtension\NetworkWatcherExtension.csproj` con il seguente:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <MinimumVisualStudioVersion>17.0</MinimumVisualStudioVersion>
    <VSToolsPath Condition="'$(VSToolsPath)' == ''">$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
  </PropertyGroup>
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectTypeGuids>{82b43b9b-a64c-4715-b499-d71e9ca2bd60};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
    <ProjectGuid>{90959726-4834-4F6B-BB45-F729740C2FFE}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>NetworkWatcherExtension</RootNamespace>
    <AssemblyName>NetworkWatcherExtension</AssemblyName>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    <GeneratePkgDefFile>true</GeneratePkgDefFile>
    <UseCodebase>true</UseCodebase>
    <IncludeAssemblyInVSIXContainer>true</IncludeAssemblyInVSIXContainer>
    <IncludeDebugSymbolsInVSIXContainer>false</IncludeDebugSymbolsInVSIXContainer>
    <IncludeDebugSymbolsInLocalVSIXDeployment>false</IncludeDebugSymbolsInLocalVSIXDeployment>
    <CopyBuildOutputToOutputDirectory>true</CopyBuildOutputToOutputDirectory>
    <CopyOutputSymbolsToOutputDirectory>true</CopyOutputSymbolsToOutputDirectory>
    <StartAction>Program</StartAction>
    <StartProgram Condition="'$(DevEnvDir)' != ''">$(DevEnvDir)devenv.exe</StartProgram>
    <StartArguments>/rootsuffix Exp</StartArguments>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
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
    <None Include="README.md" />
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
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
  <Import Project="$(VSToolsPath)\VSSDK\Microsoft.VsSDK.targets" Condition="'$(VSToolsPath)' != ''" />
</Project>
```

## Passi:

1. **Chiudi Visual Studio completamente**
2. Apri `NetworkWatcherExtension\NetworkWatcherExtension.csproj` con un editor di testo (Notepad, VSCode, etc.)
3. Sostituisci tutto il contenuto con quello sopra
4. Salva il file
5. Riapri Visual Studio
6. Restore NuGet packages (click destro sulla solution → Restore NuGet Packages)
7. Build!

## Cosa è stato corretto:

❌ **Prima** (con duplicati):
```xml
<ItemGroup>
  <Compile Include="NetworkWatcherToolWindow.cs" />
  ...
</ItemGroup>
<ItemGroup>
  <Compile Include="NetworkWatcherToolWindow.cs" />   <!-- DUPLICATO! -->
  ...
</ItemGroup>
```

✅ **Dopo** (senza duplicati):
```xml
<ItemGroup>
  <Compile Include="NetworkWatcherToolWindow.cs" />
  ...
</ItemGroup>
<!-- Solo una volta! -->
```

Il problema era che ogni file era incluso due volte, causando conflitti durante la compilazione XAML.

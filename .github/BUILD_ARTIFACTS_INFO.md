# Build Artifacts Package Documentation

## What's Included

The **NetworkWatcher-Build-Artifacts.zip** contains all compiled outputs from the Release build, including:

### 📦 Contents

- **VSIX** - Complete Visual Studio Extension package
  - `NetworkWatcherExtension.vsix` - Ready to install

- **DLLs** - Compiled assemblies
  - `NetworkWatcherExtension.dll` - Main extension assembly
  - `Titanium.Web.Proxy.dll` - HTTP/HTTPS proxy library
  - `Newtonsoft.Json.dll` - JSON serialization library
  - And all other dependencies

- **PDBs** - Debug symbols
  - `NetworkWatcherExtension.pdb` - Debugging information
  - Useful for debugging and stack traces

- **XML** - Documentation files
  - IntelliSense documentation for libraries

- **JSON** - Configuration files
  - Any JSON configuration or manifest files

### 🎯 Use Cases

#### 1. **Debugging**
If users report issues, you can:
- Use PDB files to debug stack traces
- Step through code with symbols
- Get accurate line numbers in exceptions

#### 2. **Development Reference**
Other developers can:
- Reference the compiled DLLs in their projects
- Study the assembly structure
- Analyze dependencies

#### 3. **Manual Installation**
Advanced users can:
- Manually deploy binaries
- Create custom installation scripts
- Use in non-standard VS configurations

#### 4. **Dependency Analysis**
Developers can:
- See all third-party dependencies
- Check library versions
- Analyze assembly references

### 📂 Expected Structure

```
NetworkWatcher-Build-Artifacts.zip
└── [all files from bin\Release]
    ├── NetworkWatcherExtension.vsix     ← VSIX file
    ├── NetworkWatcherExtension.dll
    ├── NetworkWatcherExtension.pdb
    ├── Titanium.Web.Proxy.dll
    ├── Newtonsoft.Json.dll
    ├── [other dependencies...]
    └── [any XML documentation files...]
```

### 🔍 What's NOT Included

- ❌ **Source code** - Available in the GitHub repository
- ❌ **Intermediate build files** - Only final outputs (no .obj files)
- ❌ **Debug build** - Only Release configuration binaries

### 📥 Where to Download

The build artifacts package is available in two places:

#### 1. **GitHub Releases** (for tagged releases)
- Go to: https://github.com/KeyserDSoze/NetworkWatcher/releases
- Find your version (e.g., `v1.0.0`)
- Under "Assets", download **NetworkWatcher-Build-Artifacts.zip**

#### 2. **GitHub Actions Artifacts** (for all builds)
- Go to: https://github.com/KeyserDSoze/NetworkWatcher/actions
- Click on any workflow run
- Scroll to "Artifacts" section
- Download **NetworkWatcher-Build-[sha].zip**

### 🚀 How to Use

#### Debugging with PDB Files

1. Extract the ZIP
2. Copy PDB files next to your installed extension DLLs
3. Enable symbol loading in Visual Studio
4. Debug with full source-level debugging

#### Referencing DLLs

1. Extract the ZIP
2. Add reference to `NetworkWatcherExtension.dll` in your project
3. Or use for reverse engineering / analysis

#### Manual Deployment

1. Extract the ZIP
2. Copy DLLs to appropriate Visual Studio extension folder
3. Register manually (advanced users only)

### ⚠️ Important Notes

- **Not for end users**: Most users should download the `.vsix` file instead
- **Developer/Debug tool**: This package is for developers and debugging
- **No installation**: These are just compiled files, not an installer
- **Symbols included**: PDB files enable full debugging

### 🔄 Automatic Generation

This package is **automatically created** by GitHub Actions for:
- ✅ Every tagged release (permanent)
- ✅ Every CI build (30-day retention)

No manual steps required!

### 📊 Typical Size

- **VSIX**: ~1-2 MB (compressed extension package)
- **Build Artifacts ZIP**: ~3-5 MB (all binaries + symbols)

### 🛠️ For Developers

If you're debugging issues:

1. Download both files from the same release
2. Install the VSIX normally
3. Copy PDB files from Build Artifacts to the installed location
4. Enable symbol loading in Visual Studio
5. Attach debugger and debug with full symbols

Typical installed location:
```
%LOCALAPPDATA%\Microsoft\VisualStudio\[version]\Extensions\[random-folder]\
```

### 📝 Version Matching

**Always use matching versions!**
- VSIX version `v1.2.0` → Build Artifacts from `v1.2.0`
- Don't mix PDB files from different versions

### 🎓 Advanced Usage

#### Extract Assembly Information
```powershell
# View assembly details
[System.Reflection.Assembly]::LoadFrom("NetworkWatcherExtension.dll").GetName()
```

#### Check Dependencies
```powershell
# List dependencies
[System.Reflection.Assembly]::LoadFrom("NetworkWatcherExtension.dll").GetReferencedAssemblies()
```

#### Analyze with ILSpy/dnSpy
1. Download and extract Build Artifacts
2. Open DLL in ILSpy or dnSpy
3. Decompile and analyze code structure

### ✅ Benefits for Users

- **Transparency**: Users can see exactly what's in the extension
- **Debugging**: Full symbols for troubleshooting
- **Security**: Can scan binaries for malware/analysis
- **Trust**: Open and transparent distribution

---

**Package created by:** GitHub Actions workflow  
**Update frequency:** Every release + every CI build  
**Retention:** 90 days (releases), 30 days (CI builds)

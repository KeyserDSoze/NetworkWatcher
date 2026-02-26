# 🚀 Quick Start: Creating Your First Release

## Overview

This guide walks you through creating your **first automated release** of the Network Watcher extension using GitHub Actions.

---

## Prerequisites

✅ You've committed and pushed all recent changes  
✅ The extension builds successfully locally  
✅ You have push access to the GitHub repository  

---

## Step-by-Step Instructions

### 1️⃣ **Verify Current Version**

Check the current version in `source.extension.vsixmanifest`:

```bash
# Open the file
code NetworkWatcherExtension\source.extension.vsixmanifest
```

Look for the `Version` attribute:
```xml
<Identity ... Version="1.0" ... />
```

Let's bump it to `1.0.0` (or your preferred version):
```xml
<Identity ... Version="1.0.0" ... />
```

### 2️⃣ **Commit the Version Change**

```bash
# Stage all changes
git add .

# Commit with a clear message
git commit -m "Prepare for v1.0.0 release"

# Push to GitHub
git push origin master
```

### 3️⃣ **Create and Push the Git Tag**

```bash
# Create a version tag (must start with 'v')
git tag v1.0.0

# Push the tag to GitHub (this triggers the release workflow!)
git push origin v1.0.0
```

### 4️⃣ **Watch the GitHub Action Run**

1. Open your browser: https://github.com/KeyserDSoze/NetworkWatcher/actions
2. You'll see a workflow run called **"Build and Release VSIX"**
3. Click on it to watch the progress
4. Wait for all steps to complete (usually 2-5 minutes)

Expected steps:
- ✅ Checkout code
- ✅ Setup MSBuild
- ✅ Restore NuGet packages
- ✅ Build VSIX
- ✅ Upload artifact
- ✅ Create GitHub Release

### 5️⃣ **Verify the Release**

1. Go to: https://github.com/KeyserDSoze/NetworkWatcher/releases
2. You should see your new release **v1.0.0**
3. Under "Assets", you'll find the `.vsix` file
4. Click to download and test!

---

## Testing the Release

### Download and Install

1. **Download the `.vsix`** from the release page
2. **Close all Visual Studio instances**
3. **Double-click the `.vsix` file**
4. Follow the installation wizard
5. **Restart Visual Studio**
6. Open **View → Other Windows → Network Watcher**

### Verify Functionality

- ✅ Tool window opens correctly
- ✅ Icon appears in the menu
- ✅ "Start Proxy" button works
- ✅ Traffic is captured
- ✅ Search functionality works
- ✅ JSON prettification works
- ✅ "Stop Proxy" cleans up properly

---

## Troubleshooting

### ❌ GitHub Action Fails

**Check the logs:**
1. Go to Actions tab
2. Click the failed workflow
3. Click on the failed step
4. Read the error message

**Common issues:**
- **NuGet restore fails**: Check package references in `.csproj`
- **MSBuild fails**: Ensure project builds locally with `msbuild`
- **VSIX not found**: Check build output path configuration

**Fix and retry:**
```bash
# Delete the tag locally
git tag -d v1.0.0

# Delete the tag remotely
git push origin :refs/tags/v1.0.0

# Fix the issue, commit, and retry
git add .
git commit -m "Fix build issue"
git push

# Create tag again
git tag v1.0.0
git push origin v1.0.0
```

### ❌ Release Created But VSIX is Missing

Check that the workflow step "Find VSIX file" succeeded. The VSIX should be at:
```
NetworkWatcherExtension\bin\Release\NetworkWatcherExtension.vsix
```

### ❌ Tag Already Exists

```bash
# Delete local tag
git tag -d v1.0.0

# Delete remote tag
git push origin :refs/tags/v1.0.0

# Recreate
git tag v1.0.0
git push origin v1.0.0
```

---

## Future Releases

For subsequent releases:

```bash
# 1. Make your changes
git add .
git commit -m "Add new feature XYZ"
git push

# 2. Update version in source.extension.vsixmanifest
#    Example: 1.0.0 → 1.1.0

# 3. Commit version bump
git add NetworkWatcherExtension\source.extension.vsixmanifest
git commit -m "Bump version to 1.1.0"
git push

# 4. Create and push new tag
git tag v1.1.0
git push origin v1.1.0

# 5. GitHub Actions handles the rest! 🎉
```

---

## Manual Workflow Trigger

If you want to build without creating a release:

1. Go to: https://github.com/KeyserDSoze/NetworkWatcher/actions
2. Select **"Build and Release VSIX"**
3. Click **"Run workflow"**
4. Choose branch: `master`
5. Click **"Run workflow"** button
6. Download the artifact from the workflow run

---

## Version Numbering Guidelines

Follow **Semantic Versioning**:

```
MAJOR.MINOR.PATCH

1.0.0 - Initial release
1.0.1 - Bug fix (patch)
1.1.0 - New feature (minor)
2.0.0 - Breaking change (major)
```

Examples:
- `v1.0.0` - Initial release with basic functionality
- `v1.0.1` - Fixed search highlighting bug
- `v1.1.0` - Added new filter options
- `v1.2.0` - Added export functionality
- `v2.0.0` - Complete UI redesign (breaking)

---

## Release Checklist

Before creating a release, verify:

- [ ] All changes committed and pushed
- [ ] Extension builds successfully locally
- [ ] Version number updated in `source.extension.vsixmanifest`
- [ ] No breaking changes (or version is MAJOR bump)
- [ ] Icon files included (Icon_128.png, Icon_32.png, Icon_16.png)
- [ ] CHANGELOG updated (if you maintain one)
- [ ] All tests passing (if applicable)

---

## Quick Command Reference

```bash
# Check current tags
git tag

# View remote tags
git ls-remote --tags origin

# Create new release
git tag v1.0.0
git push origin v1.0.0

# Delete a tag (if mistake)
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0

# View all releases
https://github.com/KeyserDSoze/NetworkWatcher/releases

# View GitHub Actions
https://github.com/KeyserDSoze/NetworkWatcher/actions
```

---

## Success! 🎉

Once your first release is published:

✅ Users can download the `.vsix` from the Releases page  
✅ GitHub shows version badges on your README  
✅ Future releases are just one `git tag` command away  
✅ CI/CD pipeline validates every push  

**Your extension now has a professional distribution system!**

---

## Next Steps

Consider adding:
- [ ] **CHANGELOG.md** - Document version history
- [ ] **Screenshots** - Add to release description
- [ ] **Video demo** - Show extension in action
- [ ] **VS Marketplace** - Publish to official marketplace
- [ ] **Auto-update check** - Notify users of new versions

See `.github/GITHUB_ACTIONS_GUIDE.md` for more advanced configurations.

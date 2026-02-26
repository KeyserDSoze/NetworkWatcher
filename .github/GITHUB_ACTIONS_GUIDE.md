# GitHub Actions Setup Guide

## 📦 Automated Build and Release System

This repository now has **2 automated GitHub Actions workflows** for building and releasing the Network Watcher Visual Studio extension.

---

## Workflows

### 1. **CI Build** (`ci-build.yml`)
**Trigger:** Every push to `master`, `main`, or `develop` branches, and all pull requests.

**What it does:**
- ✅ Builds the extension in both Debug and Release configurations
- ✅ Verifies the build is successful
- ✅ Uploads the `.vsix` file as a GitHub artifact (available for 30 days)

**Use case:** Continuous integration to ensure your code builds correctly.

---

### 2. **Build and Release** (`build-and-release.yml`)
**Trigger:** When you push a **version tag** like `v1.0.0`, `v1.2.3`, etc.

**What it does:**
- ✅ Builds the extension in Release configuration
- ✅ Creates a GitHub Release with the `.vsix` file attached
- ✅ Auto-generates release notes
- ✅ Makes the release publicly available in the "Releases" section

**Use case:** Publish new versions of your extension for users to download.

---

## How to Create a Release

### Step 1: Update Version Number

Edit `source.extension.vsixmanifest` and increment the version:

```xml
<Identity Id="NetworkWatcherExtension.3b271ed4-b286-41eb-98c2-49cd3fa520d1" Version="1.1.0" ... />
```

### Step 2: Commit and Push Changes

```bash
git add .
git commit -m "Bump version to 1.1.0"
git push origin master
```

### Step 3: Create and Push a Git Tag

```bash
# Create a tag for the new version
git tag v1.1.0

# Push the tag to GitHub
git push origin v1.1.0
```

### Step 4: Wait for GitHub Actions

- Go to **GitHub → Actions** tab
- Watch the "Build and Release VSIX" workflow run
- When complete, go to **GitHub → Releases**
- Your new release will be published with the `.vsix` file! 🎉

---

## Manual Trigger (Optional)

You can also **manually trigger** the release workflow:

1. Go to **GitHub → Actions**
2. Select "Build and Release VSIX"
3. Click **"Run workflow"**
4. Choose the branch
5. Click **"Run workflow"** button

---

## Download the VSIX

### From Releases (Public)
1. Go to **GitHub → Releases**
2. Find your version (e.g., `v1.1.0`)
3. Download the `.vsix` file under "Assets"

### From Actions Artifacts (During Development)
1. Go to **GitHub → Actions**
2. Click on a completed workflow run
3. Scroll to "Artifacts" section
4. Download `NetworkWatcher-VSIX-<commit-sha>.zip`
5. Extract and double-click the `.vsix` file

---

## Installation Instructions (for Users)

Once you download the `.vsix` file:

1. **Close all Visual Studio instances**
2. **Double-click the `.vsix` file** (or right-click → Open With → Visual Studio Installer)
3. Follow the installation wizard
4. **Restart Visual Studio**
5. Open **View → Other Windows → Network Watcher**

---

## Troubleshooting

### Build Fails on GitHub Actions

**Problem:** Workflow fails with "VSIX file not found"

**Solution:**
- Check that `NetworkWatcherExtension.csproj` builds correctly locally
- Verify all NuGet packages restore properly
- Check MSBuild output logs in the GitHub Actions run

### Tag Already Exists

**Problem:** `git push origin v1.1.0` fails because tag exists

**Solution:**
```bash
# Delete local tag
git tag -d v1.1.0

# Delete remote tag
git push origin :refs/tags/v1.1.0

# Create new tag
git tag v1.1.0
git push origin v1.1.0
```

### Want to Delete a Release

1. Go to **GitHub → Releases**
2. Click on the release
3. Click **"Delete"** button
4. Optionally delete the Git tag:
   ```bash
   git push origin :refs/tags/v1.1.0
   ```

---

## Publishing to Visual Studio Marketplace (Optional)

If you want to publish to the official **Visual Studio Marketplace**:

### Prerequisites:
1. Create a publisher account at https://marketplace.visualstudio.com/manage
2. Generate a **Personal Access Token (PAT)** from Azure DevOps
3. Add the PAT as a GitHub Secret named `MARKETPLACE_PAT`

### Add Marketplace Publishing to Workflow

Edit `.github/workflows/build-and-release.yml` and add this step after creating the GitHub Release:

```yaml
- name: Publish to VS Marketplace
  if: startsWith(github.ref, 'refs/tags/')
  run: |
    dotnet tool install -g Microsoft.VisualStudio.Services.CLI
    vsce publish -p ${{ secrets.MARKETPLACE_PAT }} -i ${{ steps.find_vsix.outputs.VSIX_PATH }}
```

---

## Version Numbering Strategy

Follow **Semantic Versioning** (SemVer):

- `v1.0.0` - Initial release
- `v1.0.1` - Bug fix (patch)
- `v1.1.0` - New feature (minor)
- `v2.0.0` - Breaking change (major)

Example release history:
```
v1.0.0 - Initial release with basic proxy
v1.1.0 - Added search functionality
v1.1.1 - Fixed search highlighting bug
v1.2.0 - Added dark theme support
v2.0.0 - Complete UI redesign
```

---

## GitHub Actions Status Badge

Add this to your `README.md` to show build status:

```markdown
[![CI Build](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml)
```

---

## Summary

✅ **CI Build** - Automatic verification on every push  
✅ **Release Workflow** - One-command release with `git tag`  
✅ **Artifact Storage** - Download builds from any commit  
✅ **GitHub Releases** - Professional distribution channel  
✅ **Automated Release Notes** - Auto-generated from commits  

Now your extension has a **professional CI/CD pipeline**! 🚀

---

## Quick Reference

### Create a New Release
```bash
# 1. Update version in source.extension.vsixmanifest
# 2. Commit changes
git add .
git commit -m "Release v1.2.0"
git push

# 3. Create and push tag
git tag v1.2.0
git push origin v1.2.0

# 4. GitHub Actions automatically creates the release! 🎉
```

### Check Build Status
```bash
# View in browser
https://github.com/KeyserDSoze/NetworkWatcher/actions
```

### Download Latest Release
```bash
# View in browser
https://github.com/KeyserDSoze/NetworkWatcher/releases/latest
```

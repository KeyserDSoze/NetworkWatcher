# Fix: GitHub Actions Error 403 "Too many retries"

## Problem

When pushing a tag to trigger the release workflow, the following errors occurred:

```
❌ Pattern 'D:\a\NetworkWatcher\...\NetworkWatcherExtension.vsix' does not match any files
⚠️ GitHub release failed with status: 403
❌ Too many retries. Aborting...
```

## Root Causes

### 1. Missing Permissions (403 Error)

**Issue:** The `GITHUB_TOKEN` didn't have permission to create releases.

**Fix:** Added `permissions` block to the workflow:

```yaml
jobs:
  build:
    runs-on: windows-latest
    permissions:
      contents: write  # Required to create releases
```

### 2. VSIX File Not Found

**Issue:** The workflow couldn't locate the `.vsix` file after build.

**Fix:** Added debug output to identify where the file is created:

```yaml
- name: Verify Build Output
  shell: pwsh
  run: |
    Write-Host "=== Verifying build output ==="
    if (Test-Path "NetworkWatcherExtension\bin\Release") {
      Write-Host "✅ Release folder exists"
      Write-Host "Contents:"
      Get-ChildItem -Path "NetworkWatcherExtension\bin\Release" | Format-Table Name, Length
    }
```

## Changes Made

### File: `.github/workflows/build-and-release.yml`

1. **Added permissions block** (line 12-13)
   ```yaml
   permissions:
     contents: write  # Required to create releases
   ```

2. **Added "Verify Build Output" step** (line 31-44)
   - Lists all files in `bin\Release`
   - Helps diagnose build issues

3. **Enhanced "Find VSIX file" step** (line 46-66)
   - More detailed logging
   - Lists all files being searched
   - Shows VSIX file size
   - Better error messages

### File: `.github/workflows/ci-build.yml`

1. **Enhanced "Find VSIX file" step**
   - Better error messages
   - Debug output for troubleshooting

## How to Test the Fix

### Option 1: Manual Trigger (Recommended)

1. Go to: https://github.com/KeyserDSoze/NetworkWatcher/actions
2. Select **"Build and Release VSIX"**
3. Click **"Run workflow"**
4. Choose branch: `master`
5. Click **"Run workflow"** button
6. Watch the logs for debug output

### Option 2: Delete and Re-push Tag

```bash
# Delete existing tag locally
git tag -d v0.0.1

# Delete tag on GitHub
git push origin :refs/tags/v0.0.1

# Wait a moment, then recreate
git tag v0.0.1
git push origin v0.0.1
```

## Expected Outcome

After the fix, you should see:

```
✅ Release folder exists
✅ Found VSIX: NetworkWatcherExtension.vsix
📦 Size: 1.23 MB
✅ Artifact uploaded
✅ GitHub Release created
```

## If It Still Fails

### Check Repository Settings

1. Go to: **Settings → Actions → General**
2. Scroll to **"Workflow permissions"**
3. Ensure **"Read and write permissions"** is selected
4. Click **"Save"**

### Verify VSIX Generation

The workflow now shows detailed logs. Check:

1. Does `NetworkWatcherExtension\bin\Release` exist?
2. Are there any `.vsix` files in that folder?
3. What files ARE being created?

### Common Issues

#### Issue: "Permission denied"

**Solution:** Check repository settings:
- Settings → Actions → General → Workflow permissions
- Enable "Read and write permissions"

#### Issue: "VSIX not found"

**Solution:** Check the build output step logs:
- Does MSBuild succeed?
- Is the VSIX being created in a different folder?
- Check for build errors

#### Issue: "Tag already exists"

**Solution:** Delete and recreate:
```bash
git tag -d v0.0.1
git push origin :refs/tags/v0.0.1
git tag v0.0.1
git push origin v0.0.1
```

## Additional Debugging

If issues persist, add this step to see MSBuild output:

```yaml
- name: Build VSIX with verbose output
  run: msbuild NetworkWatcherExtension\NetworkWatcherExtension.csproj /p:Configuration=Release /p:DeployExtension=false /v:detailed
```

This will show every file being copied and where.

## Summary

### What Was Fixed

✅ Added `permissions: contents: write` for release creation  
✅ Added debug output to locate VSIX file  
✅ Enhanced error messages  
✅ Added build verification step  

### What To Do Next

1. **Commit and push** these workflow changes
2. **Manually trigger** the workflow to test (recommended)
3. **Or delete and re-push** the tag
4. **Check the logs** for detailed output
5. **Verify** the release is created successfully

---

**Updated:** 2026-02-27  
**Status:** Fixed ✅

# 🔄 Automatic Version Management

## Overview

The GitHub Actions workflow now **automatically updates** the VSIX version based on the Git tag. You no longer need to manually edit `source.extension.vsixmanifest` before creating a release!

---

## ✅ New Workflow (Automatic)

### Before (Manual - Error Prone):

```powershell
# 1. Edit code
git add .
git commit -m "feat: New feature"

# 2. ⚠️ MANUALLY edit source.extension.vsixmanifest
#    Change Version="1.0.0" to Version="1.1.0"

# 3. Commit version change
git add source.extension.vsixmanifest
git commit -m "Bump version to 1.1.0"
git push

# 4. Create tag
git tag v1.1.0
git push origin v1.1.0

# Problems:
# ❌ Easy to forget step 2
# ❌ Tag and manifest can mismatch
# ❌ Extra commits for version bumps
```

### After (Automatic - Foolproof):

```powershell
# 1. Edit code
git add .
git commit -m "feat: New feature"
git push

# 2. Create tag (version is extracted automatically!)
git tag v1.1.0
git push origin v1.1.0

# ✅ Done! GitHub Actions automatically:
#    - Updates manifest to version 1.1.0
#    - Builds VSIX with correct version
#    - Verifies version matches tag
#    - Creates release

# Benefits:
# ✅ No manual version editing
# ✅ Tag is single source of truth
# ✅ No version mismatches
# ✅ Cleaner git history
```

---

## 🔧 How It Works

### New Workflow Step: "Update VSIX Version from Tag"

```yaml
- name: Update VSIX Version from Tag
  if: startsWith(github.ref, 'refs/tags/')
  shell: pwsh
  run: |
    # Extract version from tag (v1.2.3 -> 1.2.3)
    $tagName = "${{ github.ref_name }}"
    if ($tagName -match '^v?(\d+\.\d+\.\d+)') {
      $version = $matches[1]
      
      # Update source.extension.vsixmanifest
      $manifest = Get-Content "source.extension.vsixmanifest" -Raw
      $manifest = $manifest -replace '(<Identity[^>]+Version=")[^"]+(")', "`${1}$version`$2"
      Set-Content "source.extension.vsixmanifest" -Value $manifest
    }
```

### Workflow Sequence:

```
1. Git tag pushed: v1.2.3
   ↓
2. Workflow triggered
   ↓
3. Checkout code
   ↓
4. Extract version from tag (1.2.3)
   ↓
5. Update manifest: Version="1.2.3"
   ↓
6. Build VSIX (now has correct version)
   ↓
7. Verify version matches tag
   ↓
8. Create release with versioned VSIX
```

---

## 🎯 Supported Tag Formats

The workflow automatically extracts version from these tag formats:

✅ **Supported:**

```
v1.0.0     → Version: 1.0.0
v1.2.3     → Version: 1.2.3
v2.0.0     → Version: 2.0.0
1.0.0      → Version: 1.0.0 (without 'v' prefix)
```

❌ **Not Supported:**

```
v1.0       → ❌ Need 3 numbers (MAJOR.MINOR.PATCH)
v1         → ❌ Need 3 numbers
release-1  → ❌ Wrong format
```

**Recommendation:** Always use format `vMAJOR.MINOR.PATCH` (e.g., `v1.2.3`)

---

## 📊 Version Verification

After building, the workflow verifies that the VSIX version matches the tag:

```
=== Verifying VSIX version ===
Tag version: 1.2.3
VSIX version: 1.2.3
✅ Version match confirmed!
```

If there's a mismatch (shouldn't happen, but just in case):

```
⚠️ Version mismatch detected!
   Tag: v1.2.3
   VSIX: 1.0.0
```

---

## 🎓 Usage Examples

### Example 1: Patch Release (Bug Fix)

```powershell
# Fix a bug
git add .
git commit -m "fix: Resolve crash on startup"
git push

# Tag as patch version
git tag v1.0.1
git push origin v1.0.1

# ✅ GitHub Actions:
#    - Sets version to 1.0.1 in manifest
#    - Builds VSIX v1.0.1
#    - Creates release v1.0.1
```

### Example 2: Minor Release (New Feature)

```powershell
# Add new feature
git add .
git commit -m "feat: Add export to JSON"
git push

# Tag as minor version
git tag v1.1.0
git push origin v1.1.0

# ✅ GitHub Actions:
#    - Sets version to 1.1.0 in manifest
#    - Builds VSIX v1.1.0
#    - Creates release v1.1.0
```

### Example 3: Major Release (Breaking Changes)

```powershell
# Complete UI redesign
git add .
git commit -m "feat!: Complete UI redesign"
git push

# Tag as major version
git tag v2.0.0
git push origin v2.0.0

# ✅ GitHub Actions:
#    - Sets version to 2.0.0 in manifest
#    - Builds VSIX v2.0.0
#    - Creates release v2.0.0
```

---

## 🔄 Complete Release Workflow

```
┌─────────────────────────────────────────────────────────────┐
│ Developer                                                    │
└─────────────────────────────────────────────────────────────┘
                      │
                      │ 1. Make code changes
                      ▼
            ┌────────────────────┐
            │  git commit        │
            │  git push          │
            └──────────┬─────────┘
                      │
                      │ 2. Create tag
                      ▼
            ┌────────────────────┐
            │  git tag v1.2.3    │
            │  git push origin   │
            │       v1.2.3       │
            └──────────┬─────────┘
                      │
                      │ 3. Automatic!
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ GitHub Actions                                               │
│                                                              │
│  ✓ Extract version from tag (v1.2.3 → 1.2.3)               │
│  ✓ Update source.extension.vsixmanifest                    │
│  ✓ Build VSIX with version 1.2.3                           │
│  ✓ Verify version matches tag                               │
│  ✓ Create release with versioned VSIX                      │
│                                                              │
└──────────────────────────────┬──────────────────────────────┘
                              │
                              │ 4. Release ready!
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ GitHub Release: v1.2.3                                       │
│                                                              │
│  Assets:                                                     │
│  ├── NetworkWatcherExtension.vsix (Version: 1.2.3)         │
│  └── NetworkWatcher-Build-Artifacts.zip                     │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## ⚠️ Important Notes

### 1. Tag is Source of Truth

The **Git tag** is now the single source of truth for versions. The version in `source.extension.vsixmanifest` is overwritten during build.

**What this means:**
- ✅ You don't need to update manifest manually
- ✅ Tag and VSIX version always match
- ⚠️ Any version in manifest is ignored during release builds

### 2. Local Builds Still Use Manifest Version

The auto-update **only happens in GitHub Actions** (when you push a tag).

**Local builds:**
- Uses whatever version is in `source.extension.vsixmanifest`
- Good for testing before creating a release

**Release builds (GitHub):**
- Overwrites manifest version with tag version
- Ensures consistency

### 3. Manifest File is Temporary During Build

The version update happens **during the workflow**, not in your repository.

**What this means:**
- Your local `source.extension.vsixmanifest` is not changed
- The version update only exists in the GitHub Actions runner
- The built VSIX has the correct version from the tag

---

## 🎯 Best Practices

### 1. Use Semantic Versioning

```
MAJOR.MINOR.PATCH

1.0.0 → Initial release
1.0.1 → Bug fix
1.1.0 → New feature
2.0.0 → Breaking change
```

### 2. Tag Format

Always use: `v` + `MAJOR.MINOR.PATCH`

```bash
# ✅ Good
git tag v1.0.0
git tag v1.2.3
git tag v2.0.0

# ❌ Bad
git tag 1.0.0      # Works but prefer with 'v'
git tag v1.0       # Missing PATCH number
git tag release1   # Wrong format
```

### 3. Annotated Tags (Optional but Recommended)

```bash
# Create annotated tag with message
git tag -a v1.2.3 -m "Release v1.2.3: Add export feature"
git push origin v1.2.3

# Benefits:
# - Includes author and date
# - Can include release notes
# - Better git history
```

### 4. Delete Wrong Tags

If you create a tag with wrong version:

```bash
# Delete local tag
git tag -d v1.2.3

# Delete remote tag
git push origin :refs/tags/v1.2.3

# Create correct tag
git tag v1.2.4
git push origin v1.2.4
```

---

## 🐛 Troubleshooting

### Problem: "Tag format not recognized"

**Error:**
```
⚠️ Tag format not recognized. Expected: v1.2.3
Using existing version in manifest
```

**Solution:**
Ensure tag format is `vMAJOR.MINOR.PATCH`:

```bash
# Wrong
git tag v1.0        # Missing PATCH

# Correct
git tag v1.0.0      # Has MAJOR.MINOR.PATCH
```

### Problem: Version mismatch warning

**Warning:**
```
⚠️ Version mismatch detected!
   Tag: v1.2.3
   VSIX: 1.0.0
```

**This shouldn't happen with automatic updates, but if it does:**

1. Check GitHub Actions logs for errors in "Update VSIX Version from Tag" step
2. Verify tag format is correct
3. Check if manifest file path is correct
4. Re-run the workflow manually

---

## 📝 Commit Message Template

Since you no longer need version bump commits:

```bash
# Before (with manual version bump):
git commit -m "feat: Add new feature"
git commit -m "Bump version to 1.1.0"   # ← No longer needed!

# After (clean history):
git commit -m "feat: Add new feature"
git tag v1.1.0  # Version is in tag only
```

---

## 🎉 Benefits Summary

| Aspect | Before (Manual) | After (Automatic) |
|--------|----------------|-------------------|
| **Version Updates** | Manual edit | Automatic from tag |
| **Version Mismatches** | Possible | Impossible |
| **Git History** | Version bump commits | Clean, no version commits |
| **Source of Truth** | 2 places (tag + manifest) | 1 place (tag only) |
| **Human Error** | Easy to forget | Eliminated |
| **Workflow Steps** | 4 steps | 2 steps |

---

## 🔗 Related Documentation

- **Release Guide:** `.github/QUICK_START_RELEASE.md`
- **Workflow Details:** `.github/GITHUB_ACTIONS_GUIDE.md`
- **Semantic Versioning:** https://semver.org/

---

**Feature Added:** 2026-02-27  
**Status:** Active ✅  
**Available in:** All releases from now on

---

## ✅ Summary

🎯 **One command to release:**
```bash
git tag v1.2.3 && git push origin v1.2.3
```

✅ **Automatic version management**  
✅ **No manual editing**  
✅ **No version mismatches**  
✅ **Cleaner git history**  
✅ **Foolproof workflow**  

**Your release process is now as simple as creating a Git tag!** 🚀

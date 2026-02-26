# GitHub Actions Structure

## 📁 File Organization

```
.github/
├── workflows/
│   ├── ci-build.yml              # Continuous Integration build
│   └── build-and-release.yml     # Release automation
├── GITHUB_ACTIONS_GUIDE.md       # Complete guide
└── QUICK_START_RELEASE.md        # Quick start instructions
```

---

## 🔄 Workflow Overview

### 1. CI Build (`ci-build.yml`)

**Purpose:** Validate every code change

**Triggers:**
- Push to `master`, `main`, `develop`
- Pull requests to `master`, `main`, `develop`

**Actions:**
1. Checkout code
2. Setup MSBuild + NuGet
3. Restore packages
4. Build Debug configuration
5. Build Release configuration
6. Upload VSIX artifact (30-day retention)

**Output:**
- Build validation (pass/fail)
- Downloadable VSIX artifact

**Badge:**
```markdown
[![CI Build](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml)
```

---

### 2. Build and Release (`build-and-release.yml`)

**Purpose:** Create official releases

**Triggers:**
- Version tags (`v*.*.*`)
- Manual workflow dispatch

**Actions:**
1. Checkout code
2. Setup MSBuild + NuGet
3. Restore packages
4. Build Release configuration
5. Find generated VSIX file
6. Upload VSIX artifact (90-day retention)
7. Create GitHub Release with:
   - VSIX file attachment
   - Auto-generated release notes
   - Installation instructions
   - Feature list

**Output:**
- GitHub Release
- Downloadable VSIX in Releases section

**Badge:**
```markdown
[![Release](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/build-and-release.yml)
```

---

## 🎯 Key Features

### Automatic Release Notes

The release workflow uses `generate_release_notes: true` which:
- Lists all commits since last release
- Groups changes by category
- Mentions contributors
- Provides diff links

### Artifact Retention

- **CI Builds:** 30 days
- **Release Builds:** 90 days

### Security

- Uses `GITHUB_TOKEN` (automatically provided)
- No secrets required for basic functionality
- Token scoped to repository only

---

## 🔧 Configuration Options

### Customize Build Configuration

Edit the MSBuild command in workflows:

```yaml
# Current (Release only)
run: msbuild NetworkWatcherExtension\NetworkWatcherExtension.csproj /p:Configuration=Release /p:DeployExtension=false /v:m

# Add parameters
/p:Platform=AnyCPU           # Specify platform
/p:OutputPath=bin\Release    # Custom output path
/v:detailed                  # More verbose logging
```

### Change Trigger Branches

Edit `ci-build.yml`:

```yaml
on:
  push:
    branches: [ master, main, develop, feature/* ]  # Add feature branches
  pull_request:
    branches: [ master, main ]
```

### Pre-release vs Release

Edit `build-and-release.yml` to support pre-releases:

```yaml
- name: Determine if pre-release
  id: prerelease
  run: |
    if [[ ${{ github.ref }} == *"alpha"* ]] || [[ ${{ github.ref }} == *"beta"* ]]; then
      echo "IS_PRERELEASE=true" >> $GITHUB_OUTPUT
    else
      echo "IS_PRERELEASE=false" >> $GITHUB_OUTPUT
    fi

- name: Create GitHub Release
  uses: softprops/action-gh-release@v1
  with:
    prerelease: ${{ steps.prerelease.outputs.IS_PRERELEASE }}
```

Then tag releases as:
- `v1.0.0` → Release
- `v1.1.0-alpha.1` → Pre-release
- `v1.1.0-beta.2` → Pre-release

---

## 📊 Monitoring

### View Build Status

**GitHub UI:**
- https://github.com/KeyserDSoze/NetworkWatcher/actions

**API:**
```bash
# Get workflow runs
curl -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/KeyserDSoze/NetworkWatcher/actions/runs

# Get latest release
curl -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/KeyserDSoze/NetworkWatcher/releases/latest
```

### Status Badges

Add to `README.md`:

```markdown
<!-- Build Status -->
[![CI Build](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml)

<!-- Latest Release -->
[![GitHub release](https://img.shields.io/github/v/release/KeyserDSoze/NetworkWatcher)](https://github.com/KeyserDSoze/NetworkWatcher/releases/latest)

<!-- Total Downloads -->
[![Downloads](https://img.shields.io/github/downloads/KeyserDSoze/NetworkWatcher/total)](https://github.com/KeyserDSoze/NetworkWatcher/releases)

<!-- License -->
[![License](https://img.shields.io/github/license/KeyserDSoze/NetworkWatcher)](LICENSE)
```

---

## 🚨 Notifications

### Slack Integration

Add to workflow (requires `SLACK_WEBHOOK` secret):

```yaml
- name: Notify Slack
  if: always()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### Email Notifications

GitHub sends email notifications automatically to:
- Workflow author
- Repository watchers (if they enabled notifications)

Configure in: **Settings → Notifications → Actions**

---

## 🔐 Secrets Management

Currently, no secrets are required. If you add features that need secrets:

### Add a Secret

1. Go to: **Settings → Secrets and variables → Actions**
2. Click **"New repository secret"**
3. Name: `MARKETPLACE_PAT`
4. Value: Your Personal Access Token
5. Click **"Add secret"**

### Use in Workflow

```yaml
- name: Publish to Marketplace
  run: |
    vsce publish -p ${{ secrets.MARKETPLACE_PAT }}
```

---

## 📈 Analytics

### Workflow Usage

View usage in: **Settings → Actions → General**

- Total workflow runs
- Minutes used (free: 2,000/month for public repos)
- Storage used (free: 500 MB for public repos)

### Release Downloads

GitHub provides download counts for each release asset:
- Go to: **Insights → Traffic**
- View: Release downloads over time

---

## 🛠️ Maintenance

### Update Actions Versions

Periodically update action versions:

```yaml
# Old
uses: actions/checkout@v3

# New
uses: actions/checkout@v4
```

Check for updates:
- https://github.com/actions/checkout/releases
- https://github.com/microsoft/setup-msbuild/releases
- https://github.com/NuGet/setup-nuget/releases

### Dependabot for Actions

Add `.github/dependabot.yml`:

```yaml
version: 2
updates:
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
```

---

## 📚 Related Documentation

- `.github/GITHUB_ACTIONS_GUIDE.md` - Complete usage guide
- `.github/QUICK_START_RELEASE.md` - Step-by-step first release
- `NetworkWatcherExtension/Resources/ADD_ICON_GUIDE.md` - Icon integration
- `IDISPOSABLE_PATTERN.md` - IDisposable implementation
- `FIXES_SUMMARY.md` - Historical changes

---

## 🎓 Learning Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [MSBuild Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild)
- [VSIX Deployment](https://docs.microsoft.com/en-us/visualstudio/extensibility/shipping-visual-studio-extensions)
- [Semantic Versioning](https://semver.org/)

---

## ✅ Best Practices

1. **Always test locally before pushing**
   - Run `msbuild` in Release configuration
   - Verify VSIX installs correctly

2. **Use meaningful commit messages**
   - They appear in auto-generated release notes
   - Follow: "Add X", "Fix Y", "Update Z"

3. **Tag consistently**
   - Always use `v` prefix: `v1.0.0`
   - Follow Semantic Versioning

4. **Keep workflows simple**
   - One workflow = one purpose
   - Don't combine CI and Release

5. **Monitor build times**
   - Optimize slow steps
   - Cache NuGet packages if needed

---

## 🆘 Support

### Workflow Fails?

1. Check logs in Actions tab
2. Read error messages carefully
3. Test MSBuild command locally
4. Verify file paths in workflow

### Need Help?

- GitHub Actions Community: https://github.community/
- Stack Overflow: Tag `github-actions`
- MSBuild Issues: Tag `msbuild`

---

## Summary

✅ **2 Workflows** - CI validation + Release automation  
✅ **Zero Configuration** - Works out of the box  
✅ **Automatic Releases** - Just push a tag  
✅ **Artifact Storage** - Download builds anytime  
✅ **Release Notes** - Auto-generated from commits  
✅ **Status Badges** - Show build status in README  

**Your repository now has enterprise-grade CI/CD!** 🚀

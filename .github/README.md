# GitHub Configuration & Documentation

This directory contains the **GitHub Actions workflows** and related documentation for the Network Watcher Visual Studio Extension.

---

## 📁 Directory Structure

```
.github/
├── workflows/                      # GitHub Actions workflow files
│   ├── ci-build.yml               # Continuous Integration (CI) build
│   └── build-and-release.yml      # Automated release creation
│
├── QUICK_START_RELEASE.md         # 🚀 START HERE for your first release
├── SETUP_COMPLETE.md              # Overview of what was set up
├── GITHUB_ACTIONS_GUIDE.md        # Complete guide to workflows
├── ACTIONS_STRUCTURE.md           # Technical workflow documentation
├── VISUAL_OVERVIEW.md             # Visual diagrams & flowcharts
├── IMPLEMENTATION_SUMMARY.md      # Change log for this feature
└── README.md                      # This file
```

---

## 🎯 Quick Links

### Getting Started
- **New to releases?** → Read [`QUICK_START_RELEASE.md`](QUICK_START_RELEASE.md)
- **Just set up?** → Check [`SETUP_COMPLETE.md`](SETUP_COMPLETE.md)
- **Want visuals?** → See [`VISUAL_OVERVIEW.md`](VISUAL_OVERVIEW.md)

### Reference
- **How workflows work** → [`GITHUB_ACTIONS_GUIDE.md`](GITHUB_ACTIONS_GUIDE.md)
- **Technical details** → [`ACTIONS_STRUCTURE.md`](ACTIONS_STRUCTURE.md)
- **What changed** → [`IMPLEMENTATION_SUMMARY.md`](IMPLEMENTATION_SUMMARY.md)

### External Links
- **GitHub Actions:** https://github.com/KeyserDSoze/NetworkWatcher/actions
- **Releases:** https://github.com/KeyserDSoze/NetworkWatcher/releases
- **GitHub Docs:** https://docs.github.com/en/actions

---

## 🚀 Create a Release (TL;DR)

```bash
# 1. Update version in source.extension.vsixmanifest

# 2. Commit
git add .
git commit -m "Release v1.0.0"
git push

# 3. Tag and push
git tag v1.0.0
git push origin v1.0.0

# 4. Done! Check releases page in ~5 minutes
```

---

## 📋 Workflows

### 1. CI Build (`workflows/ci-build.yml`)

**Purpose:** Validate every code change

**Triggers:**
- Push to `master`, `main`, `develop`
- Pull requests

**What it does:**
- Builds Debug + Release
- Uploads VSIX artifact
- Shows build status badge

**When to use:**
- Automatic on every push
- Ensures code compiles

---

### 2. Build and Release (`workflows/build-and-release.yml`)

**Purpose:** Create public releases

**Triggers:**
- Version tags (`v1.0.0`, `v1.2.3`, etc.)
- Manual trigger from Actions tab

**What it does:**
- Builds Release configuration
- Creates GitHub Release
- Attaches VSIX file
- Auto-generates release notes

**When to use:**
- When ready to publish new version
- Tag format: `v*.*.*`

---

## 🎓 Key Concepts

### Git Tags
A tag marks a specific commit as a release version:

```bash
git tag v1.0.0              # Create tag
git push origin v1.0.0      # Push to GitHub (triggers workflow)
git tag -d v1.0.0           # Delete local tag
git push origin :refs/tags/v1.0.0  # Delete remote tag
```

### Semantic Versioning
Version format: `MAJOR.MINOR.PATCH`

- **MAJOR** (1.0.0 → 2.0.0) - Breaking changes
- **MINOR** (1.0.0 → 1.1.0) - New features (backward compatible)
- **PATCH** (1.0.0 → 1.0.1) - Bug fixes

### Artifacts
Build outputs stored temporarily in GitHub:

- **Where:** Actions tab → Workflow run → Artifacts section
- **CI builds:** 30-day retention
- **Release builds:** 90-day retention
- **Use for:** Testing before official release

---

## 📊 Status Badges

These badges appear in the main `README.md`:

```markdown
[![CI Build](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml)

[![Release](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/build-and-release.yml)

[![GitHub release](https://img.shields.io/github/v/release/KeyserDSoze/NetworkWatcher)](https://github.com/KeyserDSoze/NetworkWatcher/releases/latest)

[![Downloads](https://img.shields.io/github/downloads/KeyserDSoze/NetworkWatcher/total)](https://github.com/KeyserDSoze/NetworkWatcher/releases)
```

They update automatically to show:
- ✅ CI build status (passing/failing)
- ✅ Latest release version
- ✅ Total download count

---

## 🔧 Customization

### Change Trigger Branches

Edit `workflows/ci-build.yml`:

```yaml
on:
  push:
    branches: [ master, main, develop, feature/* ]
  pull_request:
    branches: [ master, main ]
```

### Add Slack Notifications

1. Get webhook URL from Slack
2. Add as repository secret: `SLACK_WEBHOOK`
3. Add to workflow:

```yaml
- name: Notify Slack
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### Change Artifact Retention

Edit workflows:

```yaml
- name: Upload artifact
  uses: actions/upload-artifact@v4
  with:
    retention-days: 60  # Change from 30 to 60 days
```

---

## 🐛 Troubleshooting

### Workflow Fails

1. Go to **Actions** tab
2. Click the failed workflow run
3. Expand the failed step
4. Read the error message

Common issues:
- **NuGet restore fails:** Package not found or network issue
- **MSBuild fails:** Compilation error (test locally first)
- **VSIX not found:** Check build output path

### Tag Already Exists

```bash
# Delete and recreate
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0
git tag v1.0.0
git push origin v1.0.0
```

### Release Not Created

Check that:
- Tag starts with `v` (e.g., `v1.0.0` not `1.0.0`)
- Tag was pushed (`git push origin v1.0.0`)
- Workflow completed successfully

---

## 📈 Monitoring

### View Workflow Runs
https://github.com/KeyserDSoze/NetworkWatcher/actions

### View Releases
https://github.com/KeyserDSoze/NetworkWatcher/releases

### View Build Logs
Actions → Select workflow run → Click on job → Expand steps

### Download Statistics
Releases → Select version → View download counts under Assets

---

## 🎯 Best Practices

1. **Test locally before tagging**
   ```bash
   msbuild NetworkWatcherExtension\NetworkWatcherExtension.csproj /p:Configuration=Release
   ```

2. **Use meaningful commit messages**
   - Good: "Add search highlighting feature"
   - Bad: "update"

3. **Follow semantic versioning**
   - Bug fix: Increment PATCH
   - New feature: Increment MINOR
   - Breaking change: Increment MAJOR

4. **Tag when stable**
   - Don't rush releases
   - Test thoroughly first
   - Tag only production-ready code

5. **Monitor workflows**
   - Check Actions tab regularly
   - Fix failures quickly
   - Keep workflows updated

---

## 📚 Additional Resources

### GitHub Actions
- [Official Documentation](https://docs.github.com/en/actions)
- [Workflow Syntax](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions)
- [Actions Marketplace](https://github.com/marketplace?type=actions)

### MSBuild
- [MSBuild Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild)
- [Command-Line Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference)

### VSIX
- [VSIX Deployment](https://docs.microsoft.com/en-us/visualstudio/extensibility/shipping-visual-studio-extensions)
- [Extension Manifest Schema](https://docs.microsoft.com/en-us/visualstudio/extensibility/vsix-extension-schema-2-0-reference)

### Semantic Versioning
- [SemVer Specification](https://semver.org/)
- [Versioning Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/versioning)

---

## 🆘 Support

### Questions?
- Read the documentation files in this directory
- Check GitHub Actions logs for errors
- Review workflow YAML syntax

### Issues?
- Test build locally first
- Check file paths in workflows
- Verify Git tag format

### Need Help?
- [GitHub Community](https://github.community/)
- [Stack Overflow - github-actions](https://stackoverflow.com/questions/tagged/github-actions)
- [MSBuild Issues](https://stackoverflow.com/questions/tagged/msbuild)

---

## ✅ Checklist: Ready for First Release?

Before creating your first release:

- [ ] Extension builds locally (`msbuild /p:Configuration=Release`)
- [ ] Version updated in `source.extension.vsixmanifest`
- [ ] Icon files added to project (Icon_128.png, Icon_32.png, Icon_16.png)
- [ ] All changes committed and pushed
- [ ] Read [`QUICK_START_RELEASE.md`](QUICK_START_RELEASE.md)
- [ ] Ready to run: `git tag v1.0.0 && git push origin v1.0.0`

---

## 🎉 Summary

This directory contains everything needed for:
- ✅ Automated CI/CD pipeline
- ✅ One-command releases
- ✅ Professional distribution
- ✅ Build validation
- ✅ Version tracking

**Time to first release:** ~5 minutes  
**Maintenance:** Minimal (push tags for new releases)  
**Reliability:** High (automated, consistent)  

**Ready to release?** → See [`QUICK_START_RELEASE.md`](QUICK_START_RELEASE.md) 🚀

---

**Created:** 2026-02-27  
**Version:** 1.0  
**Status:** Complete & Ready

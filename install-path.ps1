<#
install-path.ps1

Interactive PowerShell helper to add a build output folder to the user or machine PATH.
Usage: run from PowerShell (no args) and follow prompts.
#>

function Prompt-YesNo($message, $defaultYes = $true) {
    $default = if ($defaultYes) { 'Y' } else { 'N' }
    $resp = Read-Host "$message ($default/Y/n)"
    if ([string]::IsNullOrWhiteSpace($resp)) { return $defaultYes }
    switch ($resp.ToLower()) {
        'y' { return $true }
        'yes' { return $true }
        'n' { return $false }
        'no' { return $false }
        default { return $defaultYes }
    }
}

Write-Host "NetworkWatcher PATH installer helper" -ForegroundColor Cyan

$pwdPath = (Get-Location).Path
$defaultDebug = Join-Path $pwdPath 'bin\Debug\net10.0'
$defaultRelease = Join-Path $pwdPath 'bin\Release\net10.0'

Write-Host "Suggested build folders:" -ForegroundColor Yellow
Write-Host "  1) $defaultDebug"
Write-Host "  2) $defaultRelease"

$folder = Read-Host "Enter the folder to add to PATH (press Enter to use suggested debug folder)"
if ([string]::IsNullOrWhiteSpace($folder)) { $folder = $defaultDebug }
$folder = Resolve-Path -LiteralPath $folder -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path 2>$null

if (-not $folder) {
    Write-Host "Folder does not exist: using as-is; you can create it later or re-run after building." -ForegroundColor Yellow
    $folder = Read-Host "Confirm the absolute folder path to add to PATH"
}

if (-not (Test-Path $folder)) {
    $create = Prompt-YesNo "Folder '$folder' does not exist. Create it now?" $true
    if ($create) {
        try { New-Item -ItemType Directory -Path $folder -Force | Out-Null }
        catch { Write-Error "Failed to create folder: $_"; exit 1 }
    }
    else {
        Write-Error "Folder does not exist. Exiting."; exit 1
    }
}

$choice = Read-Host "Add to (U)ser PATH or (M)achine PATH? (U/m)"
if ([string]::IsNullOrWhiteSpace($choice)) { $choice = 'U' }
$choice = $choice.Substring(0,1).ToUpper()

function Is-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if ($choice -eq 'M') {
    if (-not (Is-Admin)) {
        Write-Error "Setting the machine PATH requires Administrator privileges. Re-run PowerShell as Administrator and try again."; exit 1
    }
    $scope = 'Machine'
}
else {
    $scope = 'User'
}

$current = [Environment]::GetEnvironmentVariable('Path', $scope)
if ($current -and $current.Split(';') -contains $folder) {
    Write-Host "The folder is already in the $scope PATH." -ForegroundColor Green
    exit 0
}

$new = if ([string]::IsNullOrEmpty($current)) { $folder } else { "$current;$folder" }
try {
    [Environment]::SetEnvironmentVariable('Path', $new, $scope)
    Write-Host "Successfully added to $scope PATH: $folder" -ForegroundColor Green
    Write-Host "Open a new terminal session to pick up the change." -ForegroundColor Yellow
}
catch {
    Write-Error "Failed to update PATH: $_"
    exit 1
}

Write-Host "You can now run 'NetworkWatcher -p' from a new terminal (may require Administrator to run the app)." -ForegroundColor Cyan

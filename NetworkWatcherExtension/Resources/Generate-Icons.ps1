# Network Watcher - Generate Icons from SVG
# This script creates PNG icons in various sizes from the SVG source

$svgPath = Join-Path $PSScriptRoot "Icon.svg"
$outputDir = $PSScriptRoot

Write-Host "Network Watcher - Icon Generator" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $svgPath)) {
    Write-Host "Error: Icon.svg not found at $svgPath" -ForegroundColor Red
    exit 1
}

Write-Host "SVG found: $svgPath" -ForegroundColor Green
Write-Host ""

# Try to find Inkscape
$inkscapePaths = @(
    "C:\Program Files\Inkscape\bin\inkscape.exe",
    "C:\Program Files (x86)\Inkscape\bin\inkscape.exe",
    "$env:LOCALAPPDATA\Programs\Inkscape\bin\inkscape.exe"
)

$inkscape = $null
foreach ($path in $inkscapePaths) {
    if (Test-Path $path) {
        $inkscape = $path
        break
    }
}

if ($inkscape) {
    Write-Host "Using Inkscape: $inkscape" -ForegroundColor Green
    Write-Host ""
    
    # Generate different sizes
    $sizes = @(128, 32, 16)
    
    foreach ($size in $sizes) {
        $outputFile = Join-Path $outputDir "Icon_$size.png"
        Write-Host "Generating ${size}x${size} icon..." -ForegroundColor Yellow
        
        & $inkscape $svgPath `
            --export-type=png `
            --export-filename=$outputFile `
            --export-width=$size `
            --export-height=$size
        
        if (Test-Path $outputFile) {
            Write-Host "  Created: $outputFile" -ForegroundColor Green
        } else {
            Write-Host "  Failed to create $outputFile" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Icon generation complete!" -ForegroundColor Green
} else {
    Write-Host "Inkscape not found. Please install Inkscape or use online converter." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Download Inkscape from: https://inkscape.org/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Or convert online at:" -ForegroundColor Cyan
    Write-Host "  - https://cloudconvert.com/svg-to-png" -ForegroundColor White
    Write-Host "  - https://ezgif.com/svg-to-png" -ForegroundColor White
    Write-Host ""
    Write-Host "Required sizes: 128x128, 32x32, 16x16" -ForegroundColor Yellow
    
    # Open the SVG in default browser so user can save it and convert
    Start-Process $svgPath
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

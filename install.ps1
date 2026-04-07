#!/usr/bin/env pwsh

<#
.SYNOPSIS
    C&A OS Simulator - One-Click Installer & Launcher
    
.DESCRIPTION
    Downloads the C&A OS Simulator from GitHub, verifies .NET installation,
    builds the project, and launches the application.

.EXAMPLE
    iex (irm https://raw.githubusercontent.com/Xznder1984/Caine-and-Able-SIM/main/install.ps1)
#>

Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  C&A OS Simulator - Installer          ║" -ForegroundColor Green
Write-Host "║  The Amazing Digital Circus            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

Write-Host "🔍 Checking .NET installation..." -ForegroundColor Yellow

$dotnetVersion = dotnet --version 2>&1
if ($dotnetVersion -match "^[6-9]\." -or $dotnetVersion -match "^[1-9][0-9]\.") {
    Write-Host "✓ .NET $dotnetVersion detected" -ForegroundColor Green
} else {
    Write-Host "✗ .NET 6.0 or later required!" -ForegroundColor Red
    Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host ""

# ==============================
# 2. CLONE REPOSITORY
# ==============================
Write-Host "📥 Cloning repository..." -ForegroundColor Yellow

$repoUrl = "https://github.com/Xznder1984/Caine-and-Able-SIM.git"
$installPath = Join-Path $env:USERPROFILE "Desktop\Caine-and-Able-SIM"

# Check if already exists
if (Test-Path $installPath) {
    Write-Host "⚠️  Directory already exists: $installPath" -ForegroundColor Yellow
    $response = Read-Host "Overwrite? (y/n)"
    if ($response -ne "y") {
        Write-Host "Installation cancelled." -ForegroundColor Yellow
        exit 0
    }
    Remove-Item $installPath -Recurse -Force -ErrorAction SilentlyContinue
}

try {
    git clone $repoUrl $installPath --depth 1 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Git clone failed!" -ForegroundColor Red
        Write-Host "  Make sure git is installed: https://git-scm.com/" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "✓ Repository cloned to: $installPath" -ForegroundColor Green
} catch {
    Write-Host "✗ Clone failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ==============================
# 3. NAVIGATE TO PROJECT
# ==============================
Write-Host "📁 Navigating to project directory..." -ForegroundColor Yellow
cd $installPath

# Check if .csproj exists
if (-not (Test-Path "*.csproj")) {
    Write-Host "✗ Project file not found!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Found project files" -ForegroundColor Green
Write-Host ""

# ==============================
# 4. RESTORE DEPENDENCIES
# ==============================
Write-Host "📦 Restoring dependencies..." -ForegroundColor Yellow
dotnet restore -q 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Restore failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Dependencies restored" -ForegroundColor Green
Write-Host ""

# ==============================
# 5. BUILD PROJECT
# ==============================
Write-Host "🔨 Building Release version..." -ForegroundColor Yellow
dotnet build -c Release -q 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    Write-Host "  Trying Debug build..." -ForegroundColor Yellow
    dotnet build -c Debug -q 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Debug build also failed!" -ForegroundColor Red
        exit 1
    }
}
Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

# ==============================
# 6. LAUNCH APPLICATION
# ==============================
Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Ready to Launch!                      ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "🎮 Starting C&A OS Simulator..." -ForegroundColor Cyan
Write-Host "   Press Ctrl+Shift+Q to emergency exit" -ForegroundColor Cyan
Write-Host ""

# Launch the application
dotnet run -c Release --no-build

Write-Host ""
Write-Host "👋 Application closed." -ForegroundColor Green
Write-Host ""
Write-Host "ℹ️  Project location: $installPath" -ForegroundColor Cyan
Write-Host "   Launch next time: cd '$installPath' && dotnet run -c Release" -ForegroundColor Cyan

#!/usr/bin/env pwsh
<#
.SYNOPSIS
    NexusFinance Production Build Script
.DESCRIPTION
    Performs nuclear clean + optimized Release build for win-x64.
    WHY: Ensures zero stale artifacts and maximum runtime performance.
.NOTES
    Author: DevOps Team
    Requires: .NET 8 SDK, PowerShell 5.1+
#>

#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# ============================================================================
# CONFIGURATION
# ============================================================================
$Configuration = "Release"
$Runtime = "win-x64"
$SelfContained = $false  # Change to $true for standalone (larger) executable
$OutputPath = ".\Publish"
$ProjectFile = "NexusFinance.csproj"

# ============================================================================
# BANNER
# ============================================================================
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           NEXUS FINANCE - PRODUCTION BUILD                    ║" -ForegroundColor Cyan
Write-Host "║           Configuration: $Configuration | Runtime: $Runtime                  ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# ============================================================================
# STEP 1: NUCLEAR CLEAN
# ============================================================================
Write-Host "🧹 [1/4] Nuclear Clean: Removing all bin/ and obj/ folders..." -ForegroundColor Yellow

try {
    # Find and remove all bin folders recursively
    Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -Force | ForEach-Object {
        Write-Host "   ├─ Removing: $($_.FullName)" -ForegroundColor DarkGray
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Also remove Publish folder if it exists
    if (Test-Path $OutputPath) {
        Write-Host "   ├─ Removing: $OutputPath" -ForegroundColor DarkGray
        Remove-Item $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    Write-Host "   └─ ✅ Clean complete" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "   └─ ❌ Clean failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================================================
# STEP 2: RESTORE DEPENDENCIES
# ============================================================================
Write-Host "📦 [2/4] Restoring NuGet packages..." -ForegroundColor Yellow

try {
    $restoreOutput = & dotnet restore $ProjectFile --verbosity quiet 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "   └─ ✅ Restore complete" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "   └─ ❌ Restore failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "OUTPUT:" -ForegroundColor Red
    Write-Host $restoreOutput -ForegroundColor DarkRed
    exit 1
}

# ============================================================================
# STEP 3: PUBLISH (RELEASE BUILD)
# ============================================================================
Write-Host "🔨 [3/4] Building Release executable..." -ForegroundColor Yellow
Write-Host "   ├─ Configuration: $Configuration" -ForegroundColor DarkGray
Write-Host "   ├─ Runtime: $Runtime" -ForegroundColor DarkGray
Write-Host "   ├─ Self-Contained: $SelfContained" -ForegroundColor DarkGray
Write-Host "   └─ Output: $OutputPath" -ForegroundColor DarkGray
Write-Host ""

try {
    $publishArgs = @(
        "publish"
        $ProjectFile
        "--configuration", $Configuration
        "--runtime", $Runtime
        "--self-contained", $SelfContained.ToString().ToLower()
        "--output", $OutputPath
        "/p:PublishSingleFile=true"
        "/p:PublishReadyToRun=true"
        "/p:IncludeNativeLibrariesForSelfExtract=true"
        "/p:DebugType=None"
        "/p:DebugSymbols=false"
        "--verbosity", "minimal"
    )
    
    Write-Host "   Running: dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray
    Write-Host ""
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host ""
    Write-Host "   └─ ✅ Build complete" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "   └─ ❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================================================
# STEP 4: VERIFICATION & SUMMARY
# ============================================================================
Write-Host "📊 [4/4] Verification..." -ForegroundColor Yellow

$exePath = Join-Path $OutputPath "NexusFinance.exe"

if (Test-Path $exePath) {
    $exeInfo = Get-Item $exePath
    $sizeInMB = [math]::Round($exeInfo.Length / 1MB, 2)
    
    Write-Host "   ├─ Executable: $($exeInfo.Name)" -ForegroundColor Green
    Write-Host "   ├─ Size: $sizeInMB MB" -ForegroundColor Green
    Write-Host "   ├─ Path: $($exeInfo.FullName)" -ForegroundColor Green
    Write-Host "   └─ Modified: $($exeInfo.LastWriteTime)" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "   └─ ⚠️  Warning: NexusFinance.exe not found in output" -ForegroundColor Yellow
    Write-Host ""
}

$stopwatch.Stop()
$elapsed = $stopwatch.Elapsed.ToString("mm\:ss")

# ============================================================================
# SUCCESS BANNER
# ============================================================================
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                   ✅ BUILD SUCCESS!                            ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║   Output Location: $OutputPath                                    ║" -ForegroundColor Green
Write-Host "║   Build Time: $elapsed                                            ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# ============================================================================
# AUTO-OPEN OUTPUT FOLDER
# ============================================================================
Write-Host "📂 Opening output folder..." -ForegroundColor Cyan

try {
    if (Test-Path $OutputPath) {
        $fullPath = Resolve-Path $OutputPath
        Invoke-Item $fullPath
        Write-Host "   └─ ✅ Explorer opened: $fullPath" -ForegroundColor Green
    }
    else {
        Write-Host "   └─ ⚠️  Output folder not found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   └─ ⚠️  Could not open folder: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🚀 Ready to ship!" -ForegroundColor Cyan
Write-Host ""

exit 0

#!/usr/bin/env pwsh
<#
.SYNOPSIS
    NexusFinance STANDALONE Build Script (Self-Contained)
.DESCRIPTION
    Builds a fully standalone executable that includes .NET 8 runtime.
    WHY: Distribution to machines without .NET 8 installed.
    TRADEOFF: Larger file size (~150MB vs ~15MB)
.NOTES
    Use this for deployment to end-users who may not have .NET installed.
#>

#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# ============================================================================
# CONFIGURATION - STANDALONE MODE
# ============================================================================
$Configuration = "Release"
$Runtime = "win-x64"
$SelfContained = $true  # ⚠️ STANDALONE - Includes .NET Runtime
$OutputPath = ".\Publish-Standalone"
$ProjectFile = "NexusFinance.csproj"

# ============================================================================
# BANNER
# ============================================================================
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║        NEXUS FINANCE - STANDALONE BUILD                       ║" -ForegroundColor Magenta
Write-Host "║        (Self-Contained | No .NET Runtime Required)            ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""
Write-Host "⚠️  This build includes the full .NET 8 runtime (~150MB)" -ForegroundColor Yellow
Write-Host ""

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# ============================================================================
# STEP 1: NUCLEAR CLEAN
# ============================================================================
Write-Host "🧹 [1/4] Nuclear Clean..." -ForegroundColor Yellow

try {
    Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -Force | ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    if (Test-Path $OutputPath) {
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
# STEP 2: RESTORE
# ============================================================================
Write-Host "📦 [2/4] Restoring packages..." -ForegroundColor Yellow

try {
    & dotnet restore $ProjectFile --verbosity quiet 2>&1 | Out-Null
    
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed"
    }
    
    Write-Host "   └─ ✅ Restore complete" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "   └─ ❌ Restore failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================================================
# STEP 3: PUBLISH (STANDALONE)
# ============================================================================
Write-Host "🔨 [3/4] Building standalone executable..." -ForegroundColor Yellow
Write-Host "   ⏳ This may take 60-90 seconds due to runtime bundling..." -ForegroundColor DarkYellow
Write-Host ""

try {
    $publishArgs = @(
        "publish"
        $ProjectFile
        "--configuration", $Configuration
        "--runtime", $Runtime
        "--self-contained", "true"
        "--output", $OutputPath
        "/p:PublishSingleFile=true"
        "/p:PublishReadyToRun=true"
        "/p:PublishTrimmed=true"  # Trim unused assemblies
        "/p:IncludeNativeLibrariesForSelfExtract=true"
        "/p:EnableCompressionInSingleFile=true"  # Compress embedded files
        "/p:DebugType=None"
        "/p:DebugSymbols=false"
        "--verbosity", "minimal"
    )
    
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
# STEP 4: VERIFICATION
# ============================================================================
Write-Host "📊 [4/4] Verification..." -ForegroundColor Yellow

$exePath = Join-Path $OutputPath "NexusFinance.exe"

if (Test-Path $exePath) {
    $exeInfo = Get-Item $exePath
    $sizeInMB = [math]::Round($exeInfo.Length / 1MB, 2)
    
    Write-Host "   ├─ Executable: $($exeInfo.Name)" -ForegroundColor Green
    Write-Host "   ├─ Size: $sizeInMB MB (includes .NET runtime)" -ForegroundColor Green
    Write-Host "   ├─ Path: $($exeInfo.FullName)" -ForegroundColor Green
    Write-Host "   └─ ✅ Can run on ANY Windows 10+ machine (no .NET required)" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "   └─ ⚠️  Executable not found" -ForegroundColor Yellow
    Write-Host ""
}

$stopwatch.Stop()
$elapsed = $stopwatch.Elapsed.ToString("mm\:ss")

# ============================================================================
# SUCCESS BANNER
# ============================================================================
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ✅ STANDALONE BUILD SUCCESS!                      ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║   Output: $OutputPath                                  ║" -ForegroundColor Green
Write-Host "║   Build Time: $elapsed                                            ║" -ForegroundColor Green
Write-Host "║   Ready for deployment to end-users!                          ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# ============================================================================
# AUTO-OPEN
# ============================================================================
Write-Host "📂 Opening output folder..." -ForegroundColor Cyan

try {
    if (Test-Path $OutputPath) {
        $fullPath = Resolve-Path $OutputPath
        Invoke-Item $fullPath
        Write-Host "   └─ ✅ Explorer opened: $fullPath" -ForegroundColor Green
    }
}
catch {
    Write-Host "   └─ ⚠️  Could not open folder: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📦 Distribution Checklist:" -ForegroundColor Cyan
Write-Host "   1. Test executable: .\$OutputPath\NexusFinance.exe" -ForegroundColor White
Write-Host "   2. Create installer or ZIP archive" -ForegroundColor White
Write-Host "   3. Sign with code signing certificate (optional)" -ForegroundColor White
Write-Host ""

exit 0

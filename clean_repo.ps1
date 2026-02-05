<#
.SYNOPSIS
    Deep Clean Script for NexusFinance Repository
.DESCRIPTION
    Removes all build artifacts, IDE cache, user files, logs, and temporary files
    before pushing to GitHub. Provides detailed feedback for every deletion.
.NOTES
    Author: DevOps Engineer
    Date: 2026-02-04
    Run this from the repository root directory.
#>

[CmdletBinding()]
param(
    [switch]$WhatIf,
    [switch]$Force
)

# Color-coded output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Deleted { param($Path) Write-Host "🗑️  Deleted: $Path" -ForegroundColor DarkGray }

# Initialize counters
$script:DeletedFolders = 0
$script:DeletedFiles = 0
$script:TotalSize = 0

Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       NEXUSFINANCE - DEEP REPOSITORY CLEANUP             ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Verify we're in the correct directory
if (-not (Test-Path ".\NexusFinance.csproj")) {
    Write-Host "❌ ERROR: Not in NexusFinance repository root!" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    Write-Host "Please navigate to the repository root and try again." -ForegroundColor Yellow
    exit 1
}

Write-Info "Repository root: $(Get-Location)"
Write-Info "WhatIf Mode: $WhatIf"

if (-not $Force) {
    Write-Warning "This will permanently delete build artifacts, cache, and temporary files."
    $confirmation = Read-Host "Continue? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Warning "Operation cancelled by user."
        exit 0
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 1: Build Artifacts (bin, obj)" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete bin and obj folders
$buildFolders = Get-ChildItem -Path . -Include "bin","obj" -Recurse -Directory -Force -ErrorAction SilentlyContinue
foreach ($folder in $buildFolders) {
    try {
        $size = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        $sizeMB = [math]::Round($size / 1MB, 2)
        
        if ($WhatIf) {
            Write-Info "[WHATIF] Would delete: $($folder.FullName) ($sizeMB MB)"
        } else {
            Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
            Write-Deleted "$($folder.FullName) ($sizeMB MB)"
            $script:DeletedFolders++
            $script:TotalSize += $size
        }
    } catch {
        Write-Warning "Failed to delete $($folder.FullName): $($_.Exception.Message)"
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 2: IDE Cache (.vs, .idea, .vscode)" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete IDE cache folders
$ideFolders = Get-ChildItem -Path . -Include ".vs",".idea",".vscode" -Recurse -Directory -Force -ErrorAction SilentlyContinue
foreach ($folder in $ideFolders) {
    try {
        $size = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        $sizeMB = [math]::Round($size / 1MB, 2)
        
        if ($WhatIf) {
            Write-Info "[WHATIF] Would delete: $($folder.FullName) ($sizeMB MB)"
        } else {
            Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
            Write-Deleted "$($folder.FullName) ($sizeMB MB)"
            $script:DeletedFolders++
            $script:TotalSize += $size
        }
    } catch {
        Write-Warning "Failed to delete $($folder.FullName): $($_.Exception.Message)"
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 3: User-Specific Files (*.user, *.suo)" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete user-specific files
$userFilePatterns = @("*.user", "*.suo", "*.userosscache", "*.sln.docstates", "*.rsuser", "*.userprefs")
foreach ($pattern in $userFilePatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        try {
            $sizeKB = [math]::Round($file.Length / 1KB, 2)
            
            if ($WhatIf) {
                Write-Info "[WHATIF] Would delete: $($file.FullName) ($sizeKB KB)"
            } else {
                Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                Write-Deleted "$($file.FullName) ($sizeKB KB)"
                $script:DeletedFiles++
                $script:TotalSize += $file.Length
            }
        } catch {
            Write-Warning "Failed to delete $($file.FullName): $($_.Exception.Message)"
        }
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 4: Logs and Temporary Files" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete logs and temporary files
$tempFilePatterns = @("*.log", "*.tmp", "*.temp", "*_wpftmp.csproj")
foreach ($pattern in $tempFilePatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        try {
            $sizeKB = [math]::Round($file.Length / 1KB, 2)
            
            if ($WhatIf) {
                Write-Info "[WHATIF] Would delete: $($file.FullName) ($sizeKB KB)"
            } else {
                Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                Write-Deleted "$($file.FullName) ($sizeKB KB)"
                $script:DeletedFiles++
                $script:TotalSize += $file.Length
            }
        } catch {
            Write-Warning "Failed to delete $($file.FullName): $($_.Exception.Message)"
        }
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 5: Old Documentation Files" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete old documentation (but PRESERVE main README.md)
$oldDocs = @("README.txt", "OLD_README.md", "README.bak", "BUILD_RELEASE.md", "RELEASE_README.md")
foreach ($doc in $oldDocs) {
    $file = Get-Item -Path ".\$doc" -ErrorAction SilentlyContinue
    if ($file) {
        try {
            $sizeKB = [math]::Round($file.Length / 1KB, 2)
            
            if ($WhatIf) {
                Write-Info "[WHATIF] Would delete: $($file.FullName) ($sizeKB KB)"
            } else {
                Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                Write-Deleted "$($file.FullName) ($sizeKB KB)"
                $script:DeletedFiles++
                $script:TotalSize += $file.Length
            }
        } catch {
            Write-Warning "Failed to delete $($file.FullName): $($_.Exception.Message)"
        }
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 6: NuGet Packages and Artifacts" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete packages folder and artifacts
$packageFolders = Get-ChildItem -Path . -Include "packages","artifacts" -Recurse -Directory -Force -ErrorAction SilentlyContinue
foreach ($folder in $packageFolders) {
    # Skip if it's packages/build (sometimes needed)
    if ($folder.FullName -like "*\packages\build") {
        Write-Info "Preserving: $($folder.FullName)"
        continue
    }
    
    try {
        $size = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        $sizeMB = [math]::Round($size / 1MB, 2)
        
        if ($WhatIf) {
            Write-Info "[WHATIF] Would delete: $($folder.FullName) ($sizeMB MB)"
        } else {
            Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
            Write-Deleted "$($folder.FullName) ($sizeMB MB)"
            $script:DeletedFolders++
            $script:TotalSize += $size
        }
    } catch {
        Write-Warning "Failed to delete $($folder.FullName): $($_.Exception.Message)"
    }
}

# Delete NuGet package files
$nugetFiles = Get-ChildItem -Path . -Filter "*.nupkg" -Recurse -File -ErrorAction SilentlyContinue
foreach ($file in $nugetFiles) {
    try {
        $sizeMB = [math]::Round($file.Length / 1MB, 2)
        
        if ($WhatIf) {
            Write-Info "[WHATIF] Would delete: $($file.FullName) ($sizeMB MB)"
        } else {
            Remove-Item -Path $file.FullName -Force -ErrorAction Stop
            Write-Deleted "$($file.FullName) ($sizeMB MB)"
            $script:DeletedFiles++
            $script:TotalSize += $file.Length
        }
    } catch {
        Write-Warning "Failed to delete $($file.FullName): $($_.Exception.Message)"
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 7: Windows and macOS Junk Files" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete OS-specific junk files
$junkFilePatterns = @("Thumbs.db", "ehthumbs.db", "Desktop.ini", ".DS_Store", ".AppleDouble", ".LSOverride")
foreach ($pattern in $junkFilePatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        try {
            $sizeKB = [math]::Round($file.Length / 1KB, 2)
            
            if ($WhatIf) {
                Write-Info "[WHATIF] Would delete: $($file.FullName) ($sizeKB KB)"
            } else {
                Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                Write-Deleted "$($file.FullName) ($sizeKB KB)"
                $script:DeletedFiles++
                $script:TotalSize += $file.Length
            }
        } catch {
            Write-Warning "Failed to delete $($file.FullName): $($_.Exception.Message)"
        }
    }
}

Write-Host "`n" + "═" * 60 -ForegroundColor Cyan
Write-Host "PHASE 8: Backup Files" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Cyan

# Delete backup files
$backupPatterns = @("*.bak", "*.backup", "*~")
foreach ($pattern in $backupPatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        try {
            $sizeKB = [math]::Round($file.Length / 1KB, 2)
            
            if ($WhatIf) {
                Write-Info "[WHATIF] Would delete: $($file.FullName) ($sizeKB KB)"
            } else {
                Remove-Item -Path $file.FullName -Force -ErrorAction Stop
                Write-Deleted "$($file.FullName) ($sizeKB KB)"
                $script:DeletedFiles++
                $script:TotalSize += $file.Length
            }
        } catch {
            Write-Warning "Failed to delete $($file.FullName): $($_.Exception.Message)"
        }
    }
}

# Summary Report
Write-Host "`n" + "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                    CLEANUP SUMMARY                        ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

$totalSizeMB = [math]::Round($script:TotalSize / 1MB, 2)

if ($WhatIf) {
    Write-Info "WhatIf Mode - No files were actually deleted"
} else {
    Write-Success "Cleanup completed successfully!"
}

Write-Host "`n📊 Statistics:" -ForegroundColor Cyan
Write-Host "   Folders Deleted: $($script:DeletedFolders)" -ForegroundColor White
Write-Host "   Files Deleted:   $($script:DeletedFiles)" -ForegroundColor White
Write-Host "   Space Freed:     $totalSizeMB MB" -ForegroundColor White

Write-Host "`n✅ Preserved Files:" -ForegroundColor Green
Write-Host "   ✓ README.md" -ForegroundColor Gray
Write-Host "   ✓ LICENSE" -ForegroundColor Gray
Write-Host "   ✓ .gitignore" -ForegroundColor Gray
Write-Host "   ✓ Source code (.cs, .xaml)" -ForegroundColor Gray
Write-Host "   ✓ Project files (.csproj, .sln)" -ForegroundColor Gray
Write-Host "   ✓ ARCHITECTURE_AUDIT_REPORT.md" -ForegroundColor Gray

Write-Host "`n💡 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Review the manual checklist below" -ForegroundColor White
Write-Host "   2. Run: git status" -ForegroundColor White
Write-Host "   3. Verify no secrets in tracked files" -ForegroundColor White
Write-Host "   4. Run: git add ." -ForegroundColor White
Write-Host "   5. Run: git commit -m 'Deep clean before initial commit'" -ForegroundColor White

Write-Host "`n" + "═" * 60 + "`n" -ForegroundColor Cyan

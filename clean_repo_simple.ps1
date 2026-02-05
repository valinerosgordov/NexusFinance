#
# NexusFinance Repository Cleanup Script
# Removes all build artifacts, IDE cache, and temporary files
#

[CmdletBinding()]
param(
    [switch]$WhatIf
)

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  NEXUSFINANCE - REPOSITORY CLEANUP" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

# Verify we're in the correct directory
if (-not (Test-Path ".\NexusFinance.csproj")) {
    Write-Host "ERROR: Not in NexusFinance repository root!" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    exit 1
}

Write-Host "Repository root: $(Get-Location)" -ForegroundColor Green
Write-Host "WhatIf Mode: $WhatIf" -ForegroundColor Yellow
Write-Host ""

# Initialize counters
$script:DeletedFolders = 0
$script:DeletedFiles = 0
$script:TotalSize = 0

# Function to delete and report
function Remove-ItemSafely {
    param(
        [string]$Path,
        [string]$Type
    )
    
    try {
        if (Test-Path $Path) {
            $item = Get-Item -Path $Path -Force
            
            if ($item -is [System.IO.DirectoryInfo]) {
                $size = (Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue | 
                         Measure-Object -Property Length -Sum).Sum
                $sizeMB = [math]::Round($size / 1MB, 2)
                
                if ($WhatIf) {
                    Write-Host "[WHATIF] Would delete folder: $Path ($sizeMB MB)" -ForegroundColor DarkGray
                } else {
                    Remove-Item -Path $Path -Recurse -Force -ErrorAction Stop
                    Write-Host "Deleted folder: $Path ($sizeMB MB)" -ForegroundColor DarkGray
                    $script:DeletedFolders++
                    $script:TotalSize += $size
                }
            } else {
                $sizeKB = [math]::Round($item.Length / 1KB, 2)
                
                if ($WhatIf) {
                    Write-Host "[WHATIF] Would delete file: $Path ($sizeKB KB)" -ForegroundColor DarkGray
                } else {
                    Remove-Item -Path $Path -Force -ErrorAction Stop
                    Write-Host "Deleted file: $Path ($sizeKB KB)" -ForegroundColor DarkGray
                    $script:DeletedFiles++
                    $script:TotalSize += $item.Length
                }
            }
        }
    } catch {
        Write-Host "WARNING: Failed to delete $Path - $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "PHASE 1: Build Artifacts (bin, obj)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$buildFolders = Get-ChildItem -Path . -Include "bin","obj" -Recurse -Directory -Force -ErrorAction SilentlyContinue
foreach ($folder in $buildFolders) {
    Remove-ItemSafely -Path $folder.FullName -Type "Folder"
}

Write-Host ""
Write-Host "PHASE 2: IDE Cache (.vs, .idea, .vscode)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$ideFolders = Get-ChildItem -Path . -Include ".vs",".idea",".vscode" -Recurse -Directory -Force -ErrorAction SilentlyContinue
foreach ($folder in $ideFolders) {
    Remove-ItemSafely -Path $folder.FullName -Type "Folder"
}

Write-Host ""
Write-Host "PHASE 3: User Files (*.user, *.suo)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$userPatterns = @("*.user", "*.suo", "*.userosscache", "*.sln.docstates", "*.rsuser")
foreach ($pattern in $userPatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        Remove-ItemSafely -Path $file.FullName -Type "File"
    }
}

Write-Host ""
Write-Host "PHASE 4: Logs and Temporary Files" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$tempPatterns = @("*.log", "*.tmp", "*.temp", "*_wpftmp.csproj")
foreach ($pattern in $tempPatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        Remove-ItemSafely -Path $file.FullName -Type "File"
    }
}

Write-Host ""
Write-Host "PHASE 5: Old Documentation" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$oldDocs = @("README.txt", "OLD_README.md", "BUILD_RELEASE.md", "RELEASE_README.md")
foreach ($doc in $oldDocs) {
    if (Test-Path ".\$doc") {
        Remove-ItemSafely -Path ".\$doc" -Type "File"
    }
}

Write-Host ""
Write-Host "PHASE 6: NuGet Packages" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$packageFolders = Get-ChildItem -Path . -Include "packages","artifacts" -Recurse -Directory -Force -ErrorAction SilentlyContinue
foreach ($folder in $packageFolders) {
    if ($folder.FullName -notlike "*\packages\build") {
        Remove-ItemSafely -Path $folder.FullName -Type "Folder"
    }
}

Write-Host ""
Write-Host "PHASE 7: OS Junk Files" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$junkPatterns = @("Thumbs.db", "ehthumbs.db", "Desktop.ini", ".DS_Store")
foreach ($pattern in $junkPatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        Remove-ItemSafely -Path $file.FullName -Type "File"
    }
}

Write-Host ""
Write-Host "PHASE 8: Backup Files" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$backupPatterns = @("*.bak", "*.backup", "*~")
foreach ($pattern in $backupPatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        Remove-ItemSafely -Path $file.FullName -Type "File"
    }
}

# Summary
Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  CLEANUP SUMMARY" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host ""

$totalSizeMB = [math]::Round($script:TotalSize / 1MB, 2)

if ($WhatIf) {
    Write-Host "WhatIf Mode - No files were actually deleted" -ForegroundColor Yellow
} else {
    Write-Host "Cleanup completed successfully!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Statistics:" -ForegroundColor Cyan
Write-Host "  Folders Deleted: $($script:DeletedFolders)" -ForegroundColor White
Write-Host "  Files Deleted:   $($script:DeletedFiles)" -ForegroundColor White
Write-Host "  Space Freed:     $totalSizeMB MB" -ForegroundColor White
Write-Host ""

Write-Host "Preserved Files:" -ForegroundColor Green
Write-Host "  - README.md" -ForegroundColor Gray
Write-Host "  - LICENSE" -ForegroundColor Gray
Write-Host "  - .gitignore" -ForegroundColor Gray
Write-Host "  - Source code (.cs, .xaml)" -ForegroundColor Gray
Write-Host "  - Project files (.csproj, .sln)" -ForegroundColor Gray
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Run: git status" -ForegroundColor White
Write-Host "  2. Verify no secrets in tracked files" -ForegroundColor White
Write-Host "  3. Run: git add ." -ForegroundColor White
Write-Host "  4. Run: git commit -m 'Deep clean before initial commit'" -ForegroundColor White
Write-Host ""

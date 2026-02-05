<#
.SYNOPSIS
    NexusFinance Operations Manager
.DESCRIPTION
    Master script for all repository operations
.PARAMETER Task
    Clean, Build, BuildStandalone, Sanitize, Run
.EXAMPLE
    .\ops.ps1 -Task Clean
    .\ops.ps1 -Task Build
    .\ops.ps1 -Task BuildStandalone
#>

param(
    [Parameter(Mandatory)]
    [ValidateSet('Clean', 'Build', 'BuildStandalone', 'Sanitize', 'Run')]
    [string]$Task,
    
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

# Configuration
$Config = @{
    ProjectFile = "NexusFinance.csproj"
    Configuration = "Release"
    Runtime = "win-x64"
    OutputStandard = ".\Publish"
    OutputStandalone = ".\Publish-Standalone"
}

# Helper functions
function Write-Banner($Title) {
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ("=" * 70) -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step($Message) {
    Write-Host "[*] $Message" -ForegroundColor Yellow
}

function Write-OK($Message) {
    Write-Host "[+] $Message" -ForegroundColor Green
}

function Write-Warn($Message) {
    Write-Host "[!] $Message" -ForegroundColor Yellow
}

function Get-FolderSizeMB($Path) {
    try {
        $bytes = (Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue | 
                 Measure-Object -Property Length -Sum).Sum
        return [math]::Round($bytes / 1MB, 2)
    } catch { return 0 }
}

# Task: Clean
function Invoke-TaskClean {
    Write-Banner "NUCLEAR CLEAN"
    
    $deletedCount = 0
    
    Write-Step "Removing bin and obj folders"
    Get-ChildItem -Path . -Include "bin","obj" -Recurse -Directory -Force -ErrorAction SilentlyContinue | ForEach-Object {
        if ($WhatIf) {
            Write-Host "  Would delete: $($_.FullName)" -ForegroundColor Gray
        } else {
            Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  Deleted: $($_.FullName)" -ForegroundColor DarkGray
            $deletedCount++
        }
    }
    
    Write-Step "Removing Publish folders"
    $Config.OutputStandard, $Config.OutputStandalone | ForEach-Object {
        if (Test-Path $_) {
            if ($WhatIf) {
                Write-Host "  Would delete: $_" -ForegroundColor Gray
            } else {
                Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "  Deleted: $_" -ForegroundColor DarkGray
                $deletedCount++
            }
        }
    }
    
    Write-Step "Removing IDE cache"
    Get-ChildItem -Path . -Include ".vs",".idea",".vscode" -Recurse -Directory -Force -ErrorAction SilentlyContinue | ForEach-Object {
        if ($WhatIf) {
            Write-Host "  Would delete: $($_.FullName)" -ForegroundColor Gray
        } else {
            Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  Deleted: $($_.FullName)" -ForegroundColor DarkGray
            $deletedCount++
        }
    }
    
    Write-Step "Removing user files"
    "*.user", "*.suo", "*.userosscache" | ForEach-Object {
        Get-ChildItem -Path . -Filter $_ -Recurse -File -Force -ErrorAction SilentlyContinue | ForEach-Object {
            if ($WhatIf) {
                Write-Host "  Would delete: $($_.FullName)" -ForegroundColor Gray
            } else {
                Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
                $deletedCount++
            }
        }
    }
    
    Write-Step "Removing temp files"
    "*.log", "*.tmp", "*_wpftmp.csproj" | ForEach-Object {
        Get-ChildItem -Path . -Filter $_ -Recurse -File -Force -ErrorAction SilentlyContinue | ForEach-Object {
            if ($WhatIf) {
                Write-Host "  Would delete: $($_.FullName)" -ForegroundColor Gray
            } else {
                Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
                $deletedCount++
            }
        }
    }
    
    Write-Host ""
    if ($WhatIf) {
        Write-Warn "WhatIf mode - nothing deleted"
    } else {
        Write-OK "Clean complete - removed $deletedCount items"
    }
}

# Task: Build
function Invoke-TaskBuild {
    param([bool]$Standalone)
    
    $mode = if ($Standalone) { "STANDALONE" } else { "STANDARD" }
    $output = if ($Standalone) { $Config.OutputStandalone } else { $Config.OutputStandard }
    
    Write-Banner "BUILD - $mode"
    
    Write-Step "[1/4] Cleaning"
    Invoke-TaskClean
    
    Write-Step "[2/4] Restoring packages"
    & dotnet restore $Config.ProjectFile --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    Write-OK "Restore complete"
    
    Write-Step "[3/4] Publishing"
    Write-Host "  Config: $($Config.Configuration)" -ForegroundColor Gray
    Write-Host "  Runtime: $($Config.Runtime)" -ForegroundColor Gray
    Write-Host "  Self-contained: $Standalone" -ForegroundColor Gray
    Write-Host ""
    
    $args = @(
        "publish", $Config.ProjectFile
        "--configuration", $Config.Configuration
        "--runtime", $Config.Runtime
        "--self-contained", $Standalone.ToString().ToLower()
        "--output", $output
        "/p:PublishSingleFile=true"
        "/p:PublishReadyToRun=true"
        "/p:IncludeNativeLibrariesForSelfExtract=true"
        "/p:DebugType=None"
        "/p:DebugSymbols=false"
        "--verbosity", "minimal"
    )
    
    if ($Standalone) {
        $args += "/p:PublishTrimmed=true"
    }
    
    & dotnet $args
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    
    Write-Step "[4/4] Verification"
    $exe = Join-Path $output "NexusFinance.exe"
    if (Test-Path $exe) {
        $sizeMB = [math]::Round((Get-Item $exe).Length / 1MB, 2)
        Write-OK "Success - $exe"
        Write-Host "  Size: $sizeMB MB" -ForegroundColor Gray
        Invoke-Item $output
    } else {
        Write-Warn "Executable not found"
    }
    
    Write-Host ""
    Write-Banner "BUILD COMPLETE"
}

# Task: Sanitize
function Invoke-TaskSanitize {
    Write-Banner "SANITIZE"
    
    $count = 0
    
    Write-Step "Removing backup files"
    "*.bak", "*.backup", "*~" | ForEach-Object {
        Get-ChildItem -Path . -Filter $_ -Recurse -File -Force -ErrorAction SilentlyContinue | ForEach-Object {
            if ($WhatIf) {
                Write-Host "  Would delete: $($_.FullName)" -ForegroundColor Gray
            } else {
                Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
                $count++
            }
        }
    }
    
    Write-Host ""
    if ($WhatIf) {
        Write-Warn "WhatIf mode - nothing deleted"
    } else {
        Write-OK "Sanitize complete - removed $count files"
    }
}

# Task: Run
function Invoke-TaskRun {
    Write-Banner "DEVELOPMENT RUN"
    Write-Step "Starting application"
    & dotnet run --project $Config.ProjectFile
}

# Main execution
if (-not (Test-Path $Config.ProjectFile)) {
    Write-Host "ERROR: Not in repository root" -ForegroundColor Red
    exit 1
}

switch ($Task) {
    'Clean' { Invoke-TaskClean }
    'Build' { Invoke-TaskBuild -Standalone $false }
    'BuildStandalone' { Invoke-TaskBuild -Standalone $true }
    'Sanitize' { Invoke-TaskSanitize }
    'Run' { Invoke-TaskRun }
}

Write-Host ""
Write-Host "Done" -ForegroundColor Green
exit 0

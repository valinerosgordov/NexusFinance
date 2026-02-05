@echo off
REM ============================================================================
REM NexusFinance - Build Launcher
REM Double-click this file to build the Release executable
REM ============================================================================

echo.
echo ========================================
echo  NEXUS FINANCE - BUILD LAUNCHER
echo ========================================
echo.
echo Choose build type:
echo   [1] Standard (Requires .NET 8 Runtime - ~15MB)
echo   [2] Standalone (Includes Runtime - ~150MB)
echo.
choice /c 12 /n /m "Enter your choice (1 or 2): "

if errorlevel 2 goto standalone
if errorlevel 1 goto standard

:standard
echo.
echo Building STANDARD version...
echo.
powershell -ExecutionPolicy Bypass -File .\build_release.ps1
goto end

:standalone
echo.
echo Building STANDALONE version...
echo.
powershell -ExecutionPolicy Bypass -File .\build_release_standalone.ps1
goto end

:end
echo.
pause

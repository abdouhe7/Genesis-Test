@echo off
setlocal EnableDelayedExpansion

REM ============================================
REM   Combat Dashboard - Auto Setup & Start
REM ============================================

echo.
echo ╔═══════════════════════════════════════════════════════╗
echo ║   Combat Stats Dashboard - Starting...                ║
echo ╚═══════════════════════════════════════════════════════╝
echo.

REM ============================================
REM   Step 1: Check Node.js Installation
REM ============================================

where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ ERROR: Node.js is not installed!
    echo.
    echo Please download and install Node.js from:
    echo    https://nodejs.org
    echo.
    echo After installation, restart your computer and run this script again.
    echo.
    pause
    exit /b 1
)

echo ✅ Node.js detected:
node --version
echo.

REM ============================================
REM   Step 2: Install Server Dependencies
REM ============================================

echo [1/4] Checking server dependencies...

if not exist "node_modules" (
    echo.
    echo 📦 Installing server dependencies...
    echo    This may take 1-2 minutes on first run...
    echo.
    call npm install --silent
    if %ERRORLEVEL% NEQ 0 (
        echo.
        echo ❌ ERROR: Failed to install server dependencies
        echo.
        pause
        exit /b 1
    )
    echo ✅ Server dependencies installed!
) else (
    echo ✅ Server dependencies already installed
)
echo.

REM ============================================
REM   Step 3: Install Client Dependencies
REM ============================================

echo [2/4] Checking dashboard client dependencies...

if not exist "client\node_modules" (
    echo.
    echo 📦 Installing dashboard dependencies...
    echo    This may take 2-3 minutes on first run...
    echo.
    cd client
    call npm install --silent
    if %ERRORLEVEL% NEQ 0 (
        cd ..
        echo.
        echo ❌ ERROR: Failed to install dashboard dependencies
        echo.
        pause
        exit /b 1
    )
    cd ..
    echo ✅ Dashboard dependencies installed!
) else (
    echo ✅ Dashboard dependencies already installed
)
echo.

REM ============================================
REM   Step 4: Check if concurrently is available
REM ============================================

echo [3/4] Preparing to start services...

REM Check if we have concurrently installed globally or locally
where concurrently >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    set "USE_CONCURRENT=1"
) else (
    if exist "node_modules\.bin\concurrently.cmd" (
        set "USE_CONCURRENT=1"
    ) else (
        set "USE_CONCURRENT=0"
    )
)

REM ============================================
REM   Step 5: Start Server and Dashboard
REM ============================================

echo [4/4] Starting server and dashboard...
echo.
echo ╔═══════════════════════════════════════════════════════╗
echo ║   🎮 Combat Dashboard is starting!                    ║
echo ╠═══════════════════════════════════════════════════════╣
echo ║   Server:    http://localhost:5000                    ║
echo ║   Dashboard: http://localhost:3000                    ║
echo ║                                                       ║
echo ║   The dashboard will open automatically in your       ║
echo ║   browser in a few seconds...                         ║
echo ║                                                       ║
echo ║   Press Ctrl+C to stop both services                  ║
echo ╚═══════════════════════════════════════════════════════╝
echo.

REM Try to use npm's dev:all script which uses concurrently
call npm run dev:all

REM If that failed, fall back to starting server only
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ⚠️  Could not start both services simultaneously
    echo    Starting server only...
    echo    To start dashboard separately, run:
    echo    cd client ^&^& npm start
    echo.
    call npm start
)

pause

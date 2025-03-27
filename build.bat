@echo off
rem Set the project path to the current directory
set projectPath=%CD%\SimpleSnakeGame.csproj

rem Set the output directory to the current directory
set outputDirectory=%CD%\bin\Release

rem Check if MSBuild is installed
where msbuild >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo MSBuild is not installed or not available in PATH. Please ensure Visual Studio is installed.
    pause
    exit /b 1
)

rem Build the project using MSBuild
echo Building project from %projectPath%...
msbuild %projectPath% /p:Configuration=Release /p:OutputPath=%outputDirectory%

rem Check if the build succeeded
if %ERRORLEVEL% EQU 0 (
    echo Build succeeded! The executable can be found in: %outputDirectory%
    start "" %outputDirectory%  rem Open the output folder
) else (
    echo Build failed. Check the errors in your project and try again.
)

pause

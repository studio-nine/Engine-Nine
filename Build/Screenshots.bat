@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..\Samples

echo.
echo.
echo Capturing Screenshots...

for /f "delims=" %%i in ('dir /b /s /a-d "*.exe"') do (
"..\Bin\ScreenshotCapturer.exe" "%%i" -size 900x600 -out "..\Bin\%%~ni.Screenshot.png"
)

popd
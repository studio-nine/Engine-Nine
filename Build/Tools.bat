@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set regasm=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false


pushd ..

call %msbuild% %flags% Tools\EffectCustomTool\EffectCustomTool\EffectCustomTool.csproj
call %msbuild% %flags% Tools\ScreenshotCapturer\ScreenshotCapturer.sln
call %msbuild% %flags% Tools\PathGraphBuilder\PathGraphBuilder.sln
call %msbuild% %flags% Tools\ProcessSamples\ProcessSamples.sln

Bin\Samples.exe "Samples" "Bin"

pushd Bin

echo.
echo.
echo Registering Effect Custom Tool...

call %regasm% Nine.Tools.EffectCustomTool.dll /unregister
call %regasm% Nine.Tools.EffectCustomTool.dll /codebase

if %PROCESSOR_ARCHITECTURE% == x86 goto regx86
if %PROCESSOR_ARCHITECTURE% == AMD64 goto regx64

:regx86

reg query HKLM\SOFTWARE\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC} /ve
if %ERRORLEVEL% == 0 (
reg add "HKLM\SOFTWARE\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /ve /d "Effect Custom Tool for Visual Studio"
reg add "HKLM\SOFTWARE\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v CLSID /d "{72C1F067-C0D6-4263-8279-65A8786C628F}"
reg add "HKLM\SOFTWARE\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v GeneratesDesignTimeSource /t REG_DWORD /d "1"
)

reg query HKLM\SOFTWARE\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC} /ve
if %ERRORLEVEL% == 0 (
reg add "HKLM\SOFTWARE\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /ve /d "Effect Custom Tool for Visual Studio"
reg add "HKLM\SOFTWARE\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v CLSID /d "{72C1F067-C0D6-4263-8279-65A8786C628F}"
reg add "HKLM\SOFTWARE\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v GeneratesDesignTimeSource /t REG_DWORD /d "1"
)

reg query HKLM\SOFTWARE\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC} /ve
if %ERRORLEVEL% == 0 (
reg add "HKLM\SOFTWARE\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /ve /d "Effect Custom Tool for Visual Studio"
reg add "HKLM\SOFTWARE\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v CLSID /d "{72C1F067-C0D6-4263-8279-65A8786C628F}"
reg add "HKLM\SOFTWARE\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v GeneratesDesignTimeSource /t REG_DWORD /d "1"
)

goto endreg

:regx64

reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC} /ve
if %ERRORLEVEL% == 0 (
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /ve /d "Effect Custom Tool for Visual Studio"
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v CLSID /d "{72C1F067-C0D6-4263-8279-65A8786C628F}"
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VCSExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v GeneratesDesignTimeSource /t REG_DWORD /d "1"
)

reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC} /ve
if %ERRORLEVEL% == 0 (
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /ve /d "Effect Custom Tool for Visual Studio"
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v CLSID /d "{72C1F067-C0D6-4263-8279-65A8786C628F}"
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v GeneratesDesignTimeSource /t REG_DWORD /d "1"
)

reg query HKLM\SOFTWARE\Wow6432Node\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC} /ve
if %ERRORLEVEL% == 0 (
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /ve /d "Effect Custom Tool for Visual Studio"
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v CLSID /d "{72C1F067-C0D6-4263-8279-65A8786C628F}"
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\VPDExpress\10.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\EffectCustomTool" /f /v GeneratesDesignTimeSource /t REG_DWORD /d "1"
)

:endreg


popd

popd


endlocal
@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release


pushd ..

call %msbuild% %flags% Tools\EffectCustomTool\EffectCustomTool\EffectCustomTool.csproj
call %msbuild% %flags% Tools\ScreenshotCapturer\ScreenshotCapturer.sln
call %msbuild% %flags% Tools\SamplerBrowser\SamplerBrowser.sln

popd


endlocal
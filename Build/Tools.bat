@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release


pushd ..

call %msbuild% %flags% Tools\EffectCustomTool\EffectCustomTool\EffectCustomTool.csproj

popd


endlocal
@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=SingleImage


pushd ..

call %msbuild% %flags% Installer\Installer.sln

popd


endlocal
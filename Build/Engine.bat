@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release


pushd ..

call %msbuild% %flags% Framework\Nine.sln

popd


endlocal
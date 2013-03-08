@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% Framework\Nine.Serialization.2012.sln
call %msbuild% %flags% Framework\Nine.Windows.2012.sln

REM call %msbuild% %flags% Framework\Nine.WindowsRT.2012.sln
REM call %msbuild% %flags% Framework\Nine.WindowsPhone.2012.sln
REM call %msbuild% %flags% Framework\Nine.Xbox.2012.sln

popd

endlocal

pause
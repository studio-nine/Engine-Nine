@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% Source\Nine.Serialization.sln
call %msbuild% %flags% Source\Nine.sln
call %msbuild% %flags% Source\Nine.Studio.sln

popd

endlocal

pause
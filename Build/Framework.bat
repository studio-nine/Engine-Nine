@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% Framework\Nine.Windows.sln
call %msbuild% %flags% Framework\Nine.WindowsRT.sln
call %msbuild% %flags% Framework\Nine.WindowsPhone.sln
call %msbuild% %flags% Framework\Nine.Xbox.sln
call %msbuild% %flags% Framework\Nine.Silverlight.sln

popd

endlocal
@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% Framework\Nine.Windows.2012.sln
call %msbuild% %flags% Framework\Nine.WindowsRT.2012.sln
call %msbuild% %flags% Framework\Nine.WindowsPhone.2012.sln
call %msbuild% %flags% Framework\Nine.Xbox.2012.sln
call %msbuild% %flags% Framework\Nine.Silverlight.2012.sln

call %msbuild% %flags% Samples\Samples.Windows.2010.sln
call %msbuild% %flags% Samples\Samples.WindowsRT.2010.sln
call %msbuild% %flags% Samples\Samples.WindowsPhone.2010.sln
call %msbuild% %flags% Samples\Samples.Xbox.2010.sln
call %msbuild% %flags% Samples\Samples.Silverlight.2010.sln

call %msbuild% %flags% Studio\Nine.Studio.2012.sln

popd

endlocal

pause
@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% Framework\Nine.Test.sln

popd


pushd ..\Framework\Nine.Test\

"%VS100COMNTOOLS%..\IDE\mstest.exe" /testcontainer:"bin\Release\Nine.Test.dll"

popd


pushd ..\Studio\UnitTest\

"%VS100COMNTOOLS%..\IDE\mstest.exe" /testcontainer:"bin\Release\UnitTest.dll"

popd

endlocal
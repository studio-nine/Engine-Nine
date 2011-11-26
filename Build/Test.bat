@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% Framework\Nine.Test.sln

popd


pushd ..\Framework\

"%VS100COMNTOOLS%..\IDE\mstest.exe" /testcontainer:"Nine.Test\bin\Release\Nine.Test.dll"

popd


pushd ..\Studio\

"%VS100COMNTOOLS%..\IDE\mstest.exe" /testcontainer:"UnitTest\bin\Release\UnitTest.dll"

popd

endlocal
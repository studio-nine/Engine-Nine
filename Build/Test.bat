@echo off


setlocal
set mstest=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo


pushd ..\Framework\Nine.Test\

"%VS100COMNTOOLS%..\IDE\mstest.exe" %flags% /testcontainer:"bin\Release\Nine.Test.dll"

popd


pushd ..\Studio\UnitTest\

"%VS100COMNTOOLS%..\IDE\mstest.exe" %flags% /testcontainer:"bin\Release\UnitTest.dll"

popd

endlocal
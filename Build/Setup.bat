@echo off


setlocal
set heat=%WIX%\bin\heat.exe
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe


pushd ..

call "%heat%" project Framework\Nine\Nine.csproj -pog:Binaries -ag -template:fragment -out project.wxs

popd


endlocal
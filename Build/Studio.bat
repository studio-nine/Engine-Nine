@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false /p:AllowUnsafeBlocks=true

pushd ..

call %msbuild% %flags% Studio\Nine.Studio.sln
call %msbuild% %flags% Studio\Nine.Studio.Extensions.sln

assoc .ix=Nine.Project
ftype "Nine.Project"="%CD%\Bin\Engine Nine.exe" %%1

popd
@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false /p:AllowUnsafeBlocks=true

pushd ..\Bin

"%WIX%bin\heat.exe" file Nine.Tools.EffectCustomTool.dll -cg EffectCustomToolRegister -gg -out ..\Installer\Source\EffectCustomTool.wxs -var var.BinSourceDir -dr ProductDir
"%WIX%bin\heat.exe" dir Samples -cg SampleExecutables -gg -out ..\Installer\Source\SampleExecutables.wxs -var var.SamplesSourceDir -dr ProductDir

call %msbuild% %flags% ..\Installer\Installer.sln

popd
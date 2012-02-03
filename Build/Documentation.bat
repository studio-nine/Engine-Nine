@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/p:Configuration=Release /p:Platform=x86

pushd ..

copy /y "Images\EngineNine.300.png" "C:\Nine.Logo.png"

call %msbuild% %flags% Documentation\Documentation.shfbproj

del "C:\Nine.Logo.png"
copy /y "Documentation\Help\Engine Nine Documentation.chm" "Documentation.chm"

popd

endlocal
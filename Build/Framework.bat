@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

pushd Tools\TextTemplates

for /f "delims=" %%i in ('dir /b /a-d "*.Reader.tt"') do (
"%CommonProgramFiles%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out %%~ni.Generated.cs %%i
"%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out %%~ni.Generated.cs %%i
)

for /f "delims=" %%i in ('dir /b /a-d "*.Writer.tt"') do (
"%CommonProgramFiles%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out %%~ni.Generated.cs %%i
"%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out %%~ni.Generated.cs %%i
)

popd

call %msbuild% %flags% Framework\Nine.Windows.sln
REM call %msbuild% %flags% Framework\Nine.Silverlight.sln

popd

call Register.bat

endlocal
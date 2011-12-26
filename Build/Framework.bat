@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release /p:Optimize=true /p:DebugSymbols=false

pushd ..

call %msbuild% %flags% /p:DefineConstants="WINDOWS;TEXT_TEMPLATE" Framework\Nine\Nine.csproj
call %msbuild% %flags% /p:DefineConstants="WINDOWS;TEXT_TEMPLATE" Framework\Nine.Graphics\Nine.Graphics.csproj

pushd Tools\TextTemplates

for /f "delims=" %%i in ('dir /b /a-d "*.tt"') do (
"%CommonProgramFiles%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out %%~ni.Generated.cs %%i
"%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out %%~ni.Generated.cs %%i
)

popd

call %msbuild% %flags% Framework\Nine\Nine.csproj
call %msbuild% %flags% Framework\Nine.Graphics\Nine.Graphics.csproj

pushd Tools\TextTemplates

for /f "delims=" %%i in ('dir /b /a-d "*.tt"') do (
"%CommonProgramFiles%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out %%~ni.Generated.cs %%i
"%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out %%~ni.Generated.cs %%i
)

popd

call %msbuild% %flags% Framework\Nine.sln
call %msbuild% %flags% Framework\Nine.Silverlight.sln

popd

call Register.bat

endlocal
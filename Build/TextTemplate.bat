@echo off

setlocal

pushd ..\Tools\TextTemplates

for /f "delims=" %%i in ('dir /b /a-d "*.Reader.Generated.cs"') do (echo. > "%%i")
for /f "delims=" %%i in ('dir /b /a-d "*.Writer.Generated.cs"') do (echo. > "%%i")

for /f "delims=" %%i in ('dir /b /a-d "*.Reader.tt"') do (
"%CommonProgramFiles%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out %%~ni.Generated.cs %%i
"%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out %%~ni.Generated.cs %%i
)

for /f "delims=" %%i in ('dir /b /a-d "*.Writer.tt"') do (
"%CommonProgramFiles%\Microsoft Shared\TextTemplating\1.2\texttransform.exe" -out %%~ni.Generated.cs %%i
"%CommonProgramFiles(x86)%\Microsoft Shared\TextTemplating\10.0\texttransform.exe" -out %%~ni.Generated.cs %%i
)

popd

endlocal
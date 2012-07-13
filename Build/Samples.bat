@echo off


setlocal
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set flags=/nologo /p:Configuration=Release


pushd "..\Samples"

echo Replacing all logos and icons...

for /f "delims=" %%i in ('dir /b /a-d /s Game.ico') do (
	copy /y ..\Images\Game.ico "%%i"
)

for /f "delims=" %%i in ('dir /b /a-d /s Background.png') do (
	copy /y ..\Images\Background.png "%%i"
)

for /f "delims=" %%i in ('dir /b /a-d /s GameThumbnail.png') do (
	copy /y ..\Images\GameThumbnail.png "%%i"
)

for /f "delims=" %%i in ('dir /b /a-d /s PhoneGameThumb.png') do (
	copy /y ..\Images\PhoneGameThumb.png "%%i"
)


call %msbuild% %flags% Samples.Windows.sln


..\Build\samples.exe "." "..\Bin"

popd


endlocal
@echo off

pushd ..


echo Deleting all *.user, *.cachefile, *.suo, *.exe, *.msi, *.chm files...

for /f "delims=" %%i in ('dir /b /a-d /s "*.user"') do (del /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.user"') do (del /ah /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.cachefile"') do (del /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.cachefile"') do (del /ah /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.suo"') do (del /ah /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.suo"') do (del /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.exe"') do (del /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.msi"') do (del /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.chm"') do (del /s "%%i")
for /f "delims=" %%i in ('dir /b /a-d /s "*.vsp"') do (del /s "%%i")


echo Deleting all bin/obj/testresults folders...

rmdir /s /q "bin"

for /f "delims=" %%i in ('dir /b /ad /s bin obj testresults') do (
	echo %%i
	rmdir /s /q "%%i"
)

rmdir /s /q "Documentation\Help"
rmdir /s /q "References"

popd

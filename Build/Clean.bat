@echo off

pushd ..


echo Deleting all *.user files...

for /f "delims=" %%i in ('dir /b /a-d /s "*.user"') do (del /s "%%i")


echo Deleting all bin/obj/testresults folders...

rmdir /s /q "bin"

for /f "delims=" %%i in ('dir /b /ad /s bin obj testresults') do (
	echo %%i
	rmdir /s /q "%%i"
)

rmdir /s /q "Installer\Installer"
rmdir /s /q "Documentation\Help"
rmdir /s /q "Publish"
rmdir /s /q "References"

popd

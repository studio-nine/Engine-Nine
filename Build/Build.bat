@echo off

setlocal

call Engine
call Test
call Documentation
call Samples
call Tools
call Installer

mkdir ..\Publish\v1.0
move "..\Installer\Installer\Express\SingleImage\DiskImages\DISK1\Engine Nine.msi" "..\Publish\v1.0\Engine.Nine.1.0.msi"


endlocal
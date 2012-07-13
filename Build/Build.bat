@echo off

for /F %%I IN ("%0") do set BATDIR=%%~dpI
cd /D %BATDIR%

setlocal

call Framework
call Test
call Editor
call Samples
REM call Tools
REM call Documentation
REM call Installer

endlocal
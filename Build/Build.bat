@echo off

for /F %%I IN ("%0") do set BATDIR=%%~dpI
cd /D %BATDIR%

setlocal

call Framework
call Samples
call Tools
call Test
call Documentation

endlocal
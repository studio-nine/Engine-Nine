@echo off

for /F %%I IN ("%0") do set BATDIR=%%~dpI
cd /D %BATDIR%

setlocal

call Framework
call Test
call Samples
call Tools

endlocal
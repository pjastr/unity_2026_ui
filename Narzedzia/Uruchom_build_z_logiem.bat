@echo off
rem Uruchamia zbudowana wersje Windows i zapisuje log gracza do Logs\player.log
for %%I in ("%~dp0..") do set "PROJ=%%~fI"
if not exist "%PROJ%\Builds\Windows\UIDemo.exe" (
  echo Brak buildu - najpierw uruchom Generuj_sceny_i_zbuduj_Windows.bat
  pause
  exit /b 1
)
if not exist "%PROJ%\Logs" mkdir "%PROJ%\Logs"
start "" "%PROJ%\Builds\Windows\UIDemo.exe" -screen-fullscreen 0 -screen-width 1600 -screen-height 900 -logFile "%PROJ%\Logs\player.log"

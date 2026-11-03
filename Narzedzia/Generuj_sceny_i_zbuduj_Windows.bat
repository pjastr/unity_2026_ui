@echo off
rem Generuje wszystkie sceny (01..18) i buduje wersje Windows x64 w trybie wsadowym (bez otwierania edytora).
rem Zamknij edytor Unity z tym projektem przed uruchomieniem!
setlocal
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe"
for %%I in ("%~dp0..") do set "PROJ=%%~fI"
if not exist "%PROJ%\Logs" mkdir "%PROJ%\Logs"
echo Projekt: %PROJ%
echo Trwa generowanie scen i build Windows... (pierwsze uruchomienie moze trwac kilka minut)
"%UNITY%" -batchmode -projectPath "%PROJ%" -buildTarget Win64 -executeMethod UIDemo.EditorTools.SceneBuilder.BatchGenerateAndBuildWindows -logFile "%PROJ%\Logs\batch_build.log"
set "RC=%ERRORLEVEL%"
>"%PROJ%\Logs\batch_exitcode.txt" echo %RC%
if "%RC%"=="0" (
  echo OK - build: %PROJ%\Builds\Windows\UIDemo.exe
) else (
  echo BLAD %RC% - zobacz %PROJ%\Logs\batch_build.log
)
if /I not "%~1"=="nopause" pause
endlocal

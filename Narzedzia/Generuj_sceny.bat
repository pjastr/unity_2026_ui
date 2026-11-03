@echo off
rem Tylko generuje sceny 01..18 (bez buildu). Zamknij edytor Unity z tym projektem przed uruchomieniem!
setlocal
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe"
for %%I in ("%~dp0..") do set "PROJ=%%~fI"
if not exist "%PROJ%\Logs" mkdir "%PROJ%\Logs"
"%UNITY%" -batchmode -projectPath "%PROJ%" -executeMethod UIDemo.EditorTools.SceneBuilder.BatchGenerate -logFile "%PROJ%\Logs\batch_generate.log"
set "RC=%ERRORLEVEL%"
if "%RC%"=="0" (echo OK) else (echo BLAD %RC% - zobacz %PROJ%\Logs\batch_generate.log)
if /I not "%~1"=="nopause" pause
endlocal

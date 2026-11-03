@echo off
rem Otwiera projekt w edytorze Unity 6000.3.24f1 (bez Unity Hub)
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe"
for %%I in ("%~dp0..") do set "PROJ=%%~fI"
start "" "%UNITY%" -projectPath "%PROJ%"

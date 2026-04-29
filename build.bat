@echo off
title Launcher Phantom Build
cls

echo ========================================
echo  Launcher Phantom - Build ^& Run
echo ========================================
echo.

REM Si es la primera vez, restaurar
if not exist "bin\" (
    echo Primera compilacion - restaurando paquetes...
    dotnet restore
)

echo Compilando...
dotnet build -c Debug --no-restore

if errorlevel 1 (
    echo.
    echo [ERROR] Fallo en compilacion
    pause
    exit /b 1
)

echo.
echo Compilacion exitosa
echo.
echo Ejecutando aplicacion...
echo.

dotnet run --no-build

pause
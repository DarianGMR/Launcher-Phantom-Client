@echo off
title Launcher Phantom - Release Build
cls

echo ========================================
echo  Launcher Phantom - Release Build
echo ========================================
echo.

echo Compilando Release...
dotnet publish -c Release -r win-x64 --self-contained

if errorlevel 1 (
    echo.
    echo [ERROR] Fallo en compilacion
    pause
    exit /b 1
)

echo.
echo Release compilado exitosamente
echo.
echo Output: bin\Release\net8.0-windows\win-x64\publish\
pause
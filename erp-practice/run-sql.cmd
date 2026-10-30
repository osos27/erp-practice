@echo off
rem ===========================================================================
rem  run-sql.cmd - one-click wrapper for run-sql.ps1
rem
rem  Why this exists:
rem    Running a .ps1 directly can be blocked by the PowerShell execution
rem    policy, and the "-ExecutionPolicy Bypass" flag only works when it is
rem    passed to a *new* powershell.exe process. Calling this .cmd file
rem    sidesteps both problems.
rem
rem  Usage:
rem    run-sql.cmd
rem    run-sql.cmd -Server ".\SQLEXPRESS"
rem ===========================================================================
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-sql.ps1" %*
set EXITCODE=%errorlevel%
endlocal & exit /b %EXITCODE%

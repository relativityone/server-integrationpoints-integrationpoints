@echo off

SET LDAPSyncPath=%LDAPSync%
if not "%root%" == "" (
	Set "LDAPSyncPath=%root%"
)

SET dllBuilder=%LDAPSyncPath%\Dependencies\AssemblyExtractor\AssemblyExtractor.exe
SET App=LDAPSync.xml
SET AppPath=%LDAPSyncPath%\Application
SET BUILDPATH=%LDAPSyncPath%\bin
SET BUILDCONFIG=Debug

::List of Dll's we want to drop into the Application
SET Files=kCura.IntegrationPoints.dll

if "%1" == "/?" goto help
if "%1" == "/-?" goto help

if not "%1" == "" (
	Set "BUILDCONFIG=%1"
)

SET BUILDCONFIGPATH=%BUILDPATH%\%BUILDCONFIG%

::Copy Application to Release Folder
echo. Copying %AppPath%\%APP% to %BUILDCONFIGPATH%
copy "%AppPath%\%APP%" "%BUILDCONFIGPATH%" > /null


pushd "%BUILDCONFIGPATH%"
call "%dllBuilder%" "%BUILDCONFIGPATH%\%APP%" "%BUILDCONFIGPATH%\%Files%"
goto end

:help
echo      Build the specified Application
echo.
echo      usage: %0 [CONFIG]
echo.
echo.           CONFIG    The configuration for the build. Possibilities are 'debug' and 'release'. (default: %BUILDCONFIG%)
echo.
echo.
echo.		Output Application will be in %BUILDPATH%\%%Config%% 
echo.			(default: %BUILDPATH%\%BUILDCONFIG%)
:end
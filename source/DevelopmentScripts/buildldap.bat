@echo off

SET BUILDCONFIG=Debug
SET BUILDACTION=build
SET BUILDPROJECT=Build.build

if "%1" == "/?" goto help
if "%1" == "-?" goto help

if not "%1" == "" (
	SET "BUILDCONFIG=%1"
)

if not "%2" == "" (
	SET "BUILDACTION=%2"
)

SET LDAPSyncRoot=%LDAPSync%\source
pushd %LDAPSync%\source\developmentscripts
nant build -buildfile:"%BUILDPROJECT%" "-D:root=%LDAPSyncRoot%" "-D:buildconfig=%BUILDCONFIG%" "-D:action=%BUILDACTION%"
popd

::DropDLL
goto end


:help
echo      Build the specified branch
echo.
echo      usage: %0 [config] [action]
echo.
echo.           config    The configuration for the build. Possibilities are 'debug' and 'release'. (default: %BUILDCONFIG%)
echo.
echo.		action	  Preforms an action on the build. Possibilities are 'build' and clean. (default: %BUILDACTION%)
:end
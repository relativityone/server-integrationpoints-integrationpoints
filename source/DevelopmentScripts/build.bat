@echo off

SET BUILDCONFIG=Debug
SET BUILDACTION=build
SET BUILDPROJECT=Build.build
SET SIGNOUTPUT=false
SET CASEID=1014823

if "%1" == "/?" goto help
if "%1" == "-?" goto help

if not "%1" == "" (
	SET "CASEID=%1"
)
if not "%2" == "" (
	SET "BUILDCONFIG=%2"
)
if not "%3" == "" (
	SET "BUILDACTION=%3"
)
if not "%3" == "" (
	SET "buildType=%4"
)

SET LDAPSyncRoot=%LDAPSync%
pushd %LDAPSync%\developmentscripts
nant build_all -buildfile:"%BUILDPROJECT%" "-D:root=%LDAPSyncRoot%" "-D:buildconfig=%BUILDCONFIG%" "-D:action=%BUILDACTION%" "-D:DevEnv=DEVENV" "-D:signOutput=SIGNOUTPUT"
popd
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
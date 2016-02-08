@echo off

SET BUILDCONFIG=Debug
SET BUILDACTION=build
SET BUILDPROJECT=Build.build

if "%1" == "/?" goto help
if "%1" == "-?" goto help


SET LDAPSyncRoot=%LDAPSync%
pushd %LDAPSync%\developmentscripts
nant start_tests -buildfile:"%BUILDPROJECT%" "-D:root=%LDAPSyncRoot%" "-D:buildconfig=%BUILDCONFIG%"
popd
goto end


:help
echo      Run unit tests
echo.
:end
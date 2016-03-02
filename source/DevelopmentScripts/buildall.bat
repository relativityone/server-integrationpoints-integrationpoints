@echo off

SET LDAPSync=C:\SourceCode\IntegrationPoints\source
SET BUILDPROJECT=Build.build
SET BUILDCONFIG=Debug
SET BUILDACTION=build
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

SET LDAPSyncRoot=%LDAPSync%
pushd %LDAPSync%\developmentscripts
nant build_and_deploy -buildfile:"%BUILDPROJECT%" "-D:root=%LDAPSyncRoot%" "-D:buildconfig=%BUILDCONFIG%" "-D:action=%BUILDACTION%" "-D:signOutput=%SIGNOUTPUT%" "-D:caseId=%CASEID%"
popd
goto end

:help
echo      Build the specified branch
echo.
echo      usage: %0 [caseId] [config] [action]
echo.
echo.		caseId 	  Workspace ID for which we are deploying to. (default: %CASEID%)
echo.           config    The configuration for the build. Possibilities are 'debug' and 'release'. (default: %BUILDCONFIG%)
echo.		action	  Preforms an action on the build. Possibilities are 'build' and clean. (default: %BUILDACTION%)
:end
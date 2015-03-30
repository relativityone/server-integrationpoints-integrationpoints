@echo off

SET LDAPSyncPath=%LDAPSync%
if not "%root%" == "" (
	Set "LDAPSyncPath=%root%"
)

SET dllUploader=%LDAPSyncPath%\DevelopmentScripts\ResourceFileUploader\ResourceFileUploader.exe
SET BUILDPATH=%LDAPSyncPath%\bin
SET BUILDCONFIG=Debug

if "%1" == "/?" goto help
if "%1" == "/-?" goto help

if not "%1" == "" (
	Set "BUILDCONFIG=%1"
)

SET BUILDCONFIGPATH=%BUILDPATH%\%BUILDCONFIG%


pushd "%BUILDCONFIGPATH%"
call "%dllUploader%" /updatedlls /masterurl:http://localhost/Relativity.Services/ /caseusername:relativity.admin@kcura.com /caseuserpassword:Test1234! /caseid:1014823 /applicationguid:DCF6E9D1-22B6-4DA3-98F6-41381E93C30C /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Contracts.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.dll /assembly:%BUILDCONFIGPATH%\kCura.LDAPProvider.dll
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
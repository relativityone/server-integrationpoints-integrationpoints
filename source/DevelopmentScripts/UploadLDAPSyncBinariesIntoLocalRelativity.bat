@echo off

SET LDAPSyncPath=%LDAPSync%
if not "%root%" == "" (
	Set "LDAPSyncPath=%root%"
)

SET dllUploader=%LDAPSyncPath%\DevelopmentScripts\ResourceFileUploader\ResourceFileUploader.exe
SET BUILDPATH=%LDAPSyncPath%\bin

SET CASEID=1014823

if "%1" == "/?" goto help
if "%1" == "/-?" goto help

if not "%1" == "" (
	Set "CASEID=%1"
)

SET BUILDCONFIGPATH=%BUILDPATH%

pushd "%BUILDCONFIGPATH%"
call "%dllUploader%" /updatedlls /masterurl:http://localhost/Relativity.Services/ /caseusername:relativity.admin@kcura.com /caseuserpassword:Test1234! /caseid:%CASEID% /applicationguid:DCF6E9D1-22B6-4DA3-98F6-41381E93C30C /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Contracts.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.dll /assembly:%BUILDCONFIGPATH%\kCura.LDAPProvider.dll /assembly:%BUILDCONFIGPATH%\kCura.DocumentTransferProvider.dll
goto end

:help
echo      Build the specified Application
echo.
echo      usage: %~n0 [WorkspaceId]
echo.
echo.           WorkspaceId    The local workspaceId of where the dlls will be updated. Invalid workspaceId has no effect.
echo.
:end
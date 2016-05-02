@echo off

SET LDAPSyncPath=%LDAPSync%
if not "%root%" == "" (
	Set "LDAPSyncPath=%root%"
)

SET dllUploader=%LDAPSyncPath%\DevelopmentScripts\ResourceFileUploader\ResourceFileUploader.exe
SET BUILDPATH=%LDAPSyncPath%\bin

if "%1" == "/?" goto help
if "%1" == "/-?" goto help
if "%1" == "" goto help

if not "%1" == "" (
	SET "CASEID=%1"
)

if not "%2" == "" (
	SET "MASTERURL=http://%2/Relativity.Services/"
)

SET BUILDCONFIGPATH=%BUILDPATH%

pushd "%BUILDCONFIGPATH%"
call "%dllUploader%" /updatedlls /masterurl:%MASTERURL% /caseusername:relativity.admin@kcura.com /caseuserpassword:Test1234! /caseid:%CASEID% /applicationguid:DCF6E9D1-22B6-4DA3-98F6-41381E93C30C /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Contracts.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.dll /assembly:%BUILDCONFIGPATH%\kCura.LDAPProvider.dll /assembly:%BUILDCONFIGPATH%\kCura.DocumentTransferProvider.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Services.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Services.Interfaces.Private.dll /assembly:%BUILDCONFIGPATH%\kCura.Image.Viewer.dll /assembly:%BUILDCONFIGPATH%\kCura.Print.dll
goto end

:help
echo      Build the specified Application
echo.
echo      usage: %~n0 [WorkspaceId] [IPAddress]
echo.
echo.           WorkspaceId    The local workspaceId of where the dlls will be updated.
echo.           IPAdress       The IP address of the relativity instance.
echo.
:end
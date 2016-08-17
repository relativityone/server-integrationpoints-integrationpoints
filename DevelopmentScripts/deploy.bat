@echo off

SET IntegrationPointsPath=%IntegrationPoints%
if not "%root%" == "" (
	Set "IntegrationPointsPath=%root%"
)

SET dllUploader=%IntegrationPointsPath%\ResourceFileUploader\ResourceFileUploader.exe
SET BUILDPATH=%IntegrationPointsPath%\lib

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
call "%dllUploader%" /updatedlls /masterurl:%MASTERURL% /caseusername:relativity.admin@kcura.com /caseuserpassword:Test1234! /caseid:%CASEID% /applicationguid:DCF6E9D1-22B6-4DA3-98F6-41381E93C30C /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Contracts.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Domain.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.dll /assembly:%BUILDCONFIGPATH%\kCura.LDAPProvider.dll /assembly:%BUILDCONFIGPATH%\kCura.DocumentTransferProvider.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Services.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Services.Interfaces.Private.dll /assembly:%BUILDCONFIGPATH%\kCura.Image.Viewer.dll /assembly:%BUILDCONFIGPATH%\kCura.Print.dll /assembly:%BUILDCONFIGPATH%\Castle.Windsor.dll /assembly:%BUILDCONFIGPATH%\Newtonsoft.Json.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.FtpProvider.Connection.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.FtpProvider.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.FtpProvider.Helpers.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.FtpProvider.Parser.dll /assembly:%BUILDCONFIGPATH%\kCura.IntegrationPoints.Security.dll  /assembly:%BUILDCONFIGPATH%\Renci.SshNet.dll
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
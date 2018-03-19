@echo off

SET DLLUPLOADER=%COMMONTOOLS%\ResourceFileUploader\ResourceFileUploader\bin\Debug\ResourceFileUploader.exe
SET LIBDIRECTORY=%~dp0..\lib

echo %DLLUPLOADER%
echo %LIBDIRECTORY%

if "%1" == "/?" goto help
if "%1" == "/-?" goto help
if "%1" == "" goto help
if "%2" == "" goto help

if not "%1" == "" (
    SET "CASEID=%1"
)

if not "%2" == "" (
    SET "MASTERURL=http://%2/Relativity.Services/"
)

SET UPDATEDLLS=/updatedlls
SET UPDATECUSTOMPAGE=

if "%3" == "custompage" (
    SET UPDATEDLLS=
    SET UPDATECUSTOMPAGE=/custompage:C2C93191-7B15-4A9B-AA56-6A8C2DAC5494=C:\SourceCode\IntegrationPoints\source\kCura.IntegrationPoints.Web\
)


call "%DLLUPLOADER%" %UPDATEDLLS% %UPDATECUSTOMPAGE% /masterurl:%MASTERURL% /casedbserver:%2  /casedbusername:eddsdbo /casedbuserpassword:edds /caseusername:relativity.admin@kcura.com /caseuserpassword:Test1234! /caseid:%CASEID% /applicationguid:DCF6E9D1-22B6-4DA3-98F6-41381E93C30C /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.Contracts.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.Domain.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.dll /assembly:%LIBDIRECTORY%\kCura.LDAPProvider.dll /assembly:%LIBDIRECTORY%\kCura.DocumentTransferProvider.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.Services.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.Services.Interfaces.Private.dll /assembly:%LIBDIRECTORY%\kCura.Image.Viewer.dll /assembly:%LIBDIRECTORY%\kCura.Print.dll /assembly:%LIBDIRECTORY%\Castle.Windsor.dll /assembly:%LIBDIRECTORY%\Newtonsoft.Json.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.FtpProvider.Connection.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.FtpProvider.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.FtpProvider.Helpers.dll /assembly:%LIBDIRECTORY%\kCura.IntegrationPoints.FtpProvider.Parser.dll /assembly:%LIBDIRECTORY%\Renci.SshNet.dll
goto end

:help
echo      Build the specified Application
echo.
echo      usage: %~n0 [WorkspaceId] [IPAddress] [custompage]
echo.
echo.           WorkspaceId    The local workspaceId of where the dlls will be updated.
echo.           IPAdress       The IP address of the relativity instance.
echo.           custompage     If this parameter is custompage then custom page is also updated, otherwise only resource dlls are uploaded
echo.
:end
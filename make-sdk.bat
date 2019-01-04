@echo off
setlocal enabledelayedexpansion

call :show-help-if-requested %1

::input parameters
if "%1"=="" (
 set @version=empty
) else (
 set @version=%1
)
if "%2"=="" (
 set @packages-dir=empty
) else (
 set @packages-dir=%2
)
if "%3"=="" (
 set @source-dir=empty
) else (
 set @source-dir=%3
)

set @root-dir=.

::set defaults
if "%@source-dir%"=="empty" set @source-dir=%@root-dir%\Source

call :validate-parameters %@version% %@packages-dir% %@source-dir%

echo Preparing RIP SDK %@version% ...

::variables
set @sdk-main-dir-name=Relativity Integration Points SDK %@version%
set @sdk-main-sub-dir-name=Integration Points SDK
set @sdk-examples-dir-name=Examples
set @sdk-provider-dir-name=Provider
set @sdk-web-dir-name=Web

set @sdk-main-dir=%@root-dir%\%@sdk-main-dir-name%
set @sdk-main-sub-dir=%@sdk-main-dir%\%@sdk-main-sub-dir-name%
set @sdk-examples-dir=%@sdk-main-sub-dir%\%@sdk-examples-dir-name%
set @sdk-provider-dir=%@sdk-main-sub-dir%\%@sdk-provider-dir-name%
set @sdk-web-dir=%@sdk-main-sub-dir%\%@sdk-web-dir-name%

set @source-rap-dir=%@packages-dir%\RAP
set @source-bin-dir=%@packages-dir%\bin
set @source-web-scripts=%@source-dir%\Web\Scripts

set @sdk-dlls-count=13
set @sdk-dlls[0]=kCura.Apps.Common.Config.dll
set @sdk-dlls[1]=kCura.Apps.Common.Data.dll
set @sdk-dlls[2]=kCura.Apps.Common.Utils.dll
set @sdk-dlls[3]=kCura.IntegrationPoints.Config.dll
set @sdk-dlls[4]=kCura.IntegrationPoints.Contracts.dll
set @sdk-dlls[5]=kCura.IntegrationPoints.Core.Contracts.dll
set @sdk-dlls[6]=kCura.IntegrationPoints.Core.dll
set @sdk-dlls[7]=kCura.IntegrationPoints.Data.dll
set @sdk-dlls[8]=kCura.IntegrationPoints.Domain.dll
set @sdk-dlls[9]=kCura.IntegrationPoints.SourceProviderInstaller.dll
set @sdk-dlls[10]=kCura.IntegrationPoints.Synchronizers.RDO.dll
set @sdk-dlls[11]=kCura.ScheduleQueue.Core.dll
set @sdk-dlls[12]=Relativity.DataTransfer.MessageService.dll
set @sdk-dlls[13]=SystemInterface.dll

set @sdk-web-scripts-count=2
set @sdk-web-scripts[0]=frame-messaging.js
set @sdk-web-scripts[1]=jquery-3.3.1.js
set @sdk-web-scripts[2]=jquery-postMessage.js

::jsonloader
set @sdk-jsonloader-dir=%@sdk-examples-dir%\JsonLoader
set @sdk-jsonloader-provider-dir=%@sdk-jsonloader-dir%\JsonLoader
set @sdk-jsonloader-web-dir=%@sdk-jsonloader-dir%\JsonWeb
set @source-jsonloader-dir=%@source-dir%\JsonLoader
set @source-jsonweb-dir=%@source-dir%\JsonWeb
set @source-jsonloader-sln=JsonLoader.sln
set @source-jsonloader-rap=JsonLoader.rap

::myfirstprovider
set @sdk-myfirstprovider-dir=%@sdk-examples-dir%\MyFirstProvider
set @sdk-myfirstprovider-provider-dir=%@sdk-myfirstprovider-dir%\Provider
set @sdk-myfirstprovider-web-dir=%@sdk-myfirstprovider-dir%\Web
set @source-myfirstprovider-dir=%@source-dir%\Provider
set @source-myfirstproviderweb-dir=%@source-dir%\Web
set @source-myfirstproviderweb-sln=MyFirstProvider.sln
set @source-myfirstprovider-rap=MyFirstProvider.rap

md "%@sdk-main-dir%"
md "%@sdk-main-sub-dir%"
md "%@sdk-provider-dir%"
md "%@sdk-web-dir%"
md "%@sdk-jsonloader-dir%"
md "%@sdk-myfirstprovider-dir%"

call :stop-with-message-if-any-error "Error while creating folder structure"

::pack examples
robocopy /e "%@source-jsonloader-dir%" "%@sdk-jsonloader-provider-dir%" /is /xd "bin" "obj"
robocopy /e "%@source-jsonweb-dir%" "%@sdk-jsonloader-web-dir%" /is /xd "bin" "obj"
robocopy "%@source-dir%" "%@sdk-jsonloader-dir%" "%@source-jsonloader-sln%" /is
robocopy "%@source-rap-dir%" "%@sdk-examples-dir%" "%@source-jsonloader-rap%" /is

call :stop-with-message-if-any-robocopy-error "Error while copying JsonLoader"

robocopy /e "%@source-myfirstprovider-dir%" "%@sdk-myfirstprovider-provider-dir%" /is /xd "bin" "obj"
robocopy /e "%@source-myfirstproviderweb-dir%" "%@sdk-myfirstprovider-web-dir%" /is /xd "bin" "obj"
robocopy "%@source-dir%" "%@sdk-myfirstprovider-dir%" "%@source-myfirstproviderweb-sln%" /is
robocopy "%@source-rap-dir%" "%@sdk-examples-dir%" "%@source-myfirstprovider-rap%" /is

call :stop-with-message-if-any-robocopy-error "Error while copying MyFirstProvider"

::pack provider - dlls
for /l %%i in (0,1,%@sdk-dlls-count%) do (
 robocopy /is "%@source-bin-dir%" "%@sdk-provider-dir%" "!@sdk-dlls[%%i]!"
)

call :stop-with-message-if-any-robocopy-error "Error while copying DLLs"

::pack web files
for /l %%i in (0,1,%@sdk-web-scripts-count%) do (
 robocopy /is "%@source-web-scripts%" "%@sdk-web-dir%" "!@sdk-web-scripts[%%i]!"
)

call :stop-with-message-if-any-robocopy-error "Error while copying web scripts"

::zip SDK
C:\"Program Files"\7-Zip\7z.exe a -tzip "%@sdk-main-dir-name%.zip" -r "%@sdk-main-dir-name%"

call :stop-with-message-if-any-error "Error while zipping"

rmdir /s /q "%@sdk-main-dir-name%"

::eof
call :echo-green "%@sdk-main-dir-name% generated successfully!"
exit /b %ERRORLEVEL%

::functions
:stop-with-message-if-any-robocopy-error
if %ERRORLEVEL% GTR 1 (
 call :echo-red "%~1 ERRORCODE: %ERRORLEVEL%"
 call :halt 2> nul
)
exit /b 0

:stop-with-message-if-any-error
if %ERRORLEVEL% NEQ 0 (
 call :echo-red "%~1 ERRORCODE: %ERRORLEVEL%"
 call :halt 2> nul
)
exit /b 0

:validate-parameters
set valid=true

if "%1"=="empty" (
 set valid=false
 call :echo-red "Version is not provided."
)
if "%2"=="empty" (
 set valid=false
 call :echo-red "Packages path is not provided."
)
if not exist "%2" (
 set valid=false
 call :echo-red "Packages path does not exist."
)
if "%3"=="empty" ( 
 set valid=false
 call :echo-red "Source path is not provided."
)
if not exist "%3" (
  set valid=false
  call :echo-red "Source path does not exist."
)

if "%valid%"=="false" (
 call :echo-red "Invalid parameters passed. Type --help for more info."
 call :halt 2> nul
)
exit /b 0

:show-help
echo NAME
echo    RIP SDK Maker 0.1
echo.
echo SYNOPSIS
echo    Prepares RIP SDK for specified version.
echo.
echo REQUIREMENTS 
echo    7-zip                           https://www.7-zip.org/a/7z1806-x64.exe
echo.
echo OPTIONS
echo    -h, --help                      display help and exit
echo.
echo SYNTAX
echo    .\make-sdk.bat [Options]
echo    .\make-sdk.bat [Options] [Version] [BuildPackagesPath]
echo    .\make-sdk.bat [Options] [Version] [BuildPackagesPath] [SourcePath]
echo.
echo SAMPLE USAGE
echo    .\make-sdk.bat 10.0.16.8 \\bld-pkgs.kcura.corp\Packages\IntegrationPoints\release-10.0-larkspur1\10.0.16.8
echo    .\make-sdk.bat 10.0.16.8 \\bld-pkgs.kcura.corp\Packages\IntegrationPoints\release-10.0-larkspur1\10.0.16.8 S:\Source
call :halt 2> nul
exit /b 0

:show-help-if-requested
set arg=%1
if defined arg (
 if "%arg%"=="-h" call :show-help
 if "%arg%"=="--help" call :show-help
)
exit /b 0

:echo-red
echo [31m%~1[0m
exit /b 0

:echo-green
echo [32m%~1[0m
exit /b 0

:halt
()
exit /b
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
 set @jsonloader-packages-dir=empty
) else (
 set @jsonloader-packages-dir=%2
)
if "%3"=="" (
 set @jsonloader-source-dir=empty
) else (
 set @jsonloader-source-dir=%3
)
if "%4"=="" (
 set @myfirstprovider-packages-dir=empty
) else (
 set @myfirstprovider-packages-dir=%4
)
if "%5"=="" (
 set @myfirstprovider-source-dir=empty
) else (
 set @myfirstprovider-source-dir=%5
)

set @root-dir=.

call :validate-parameters %@version% %@jsonloader-packages-dir% %@jsonloader-source-dir% %@myfirstprovider-packages-dir% %@myfirstprovider-source-dir%

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

set @unzipped-rap-folder-name=UnzippedRap
set @source-bin-dir=%@root-dir%\%@unzipped-rap-folder-name%\assemblies
set @source-web-scripts-dir=%@jsonloader-source-dir%\Relativity.IntegrationPoints.JsonLoader.Web\Scripts

set @sdk-dlls-count=2
set @sdk-dlls[0]=Relativity.IntegrationPoints.Contracts.dll
set @sdk-dlls[1]=Relativity.IntegrationPoints.SourceProviderInstaller.dll
set @sdk-dlls[2]=Relativity.IntegrationPoints.Services.Interfaces.Private.dll

set @sdk-web-scripts-count=2
set @sdk-web-scripts[0]=frame-messaging.js
set @sdk-web-scripts[1]=jquery-3.4.1.js
set @sdk-web-scripts[2]=jquery-postMessage.js

::jsonloader
set @sdk-jsonloader-dir=%@sdk-examples-dir%\JsonLoader
set @sdk-jsonloader-provider-dir=%@sdk-jsonloader-dir%\Relativity.IntegrationPoints.JsonLoader
set @sdk-jsonloader-web-dir=%@sdk-jsonloader-dir%\Relativity.IntegrationPoints.JsonLoader.Web

set @source-jsonloader-dir=%@jsonloader-source-dir%\Relativity.IntegrationPoints.JsonLoader
set @source-jsonweb-dir=%@jsonloader-source-dir%\Relativity.IntegrationPoints.JsonLoader.Web
set @source-jsonloader-sln=Master.sln
set @source-jsonloader-rap=JsonLoader.rap

::myfirstprovider
set @sdk-myfirstprovider-dir=%@sdk-examples-dir%\MyFirstProvider
set @sdk-myfirstprovider-provider-dir=%@sdk-myfirstprovider-dir%\Relativity.IntegrationPoints.MyFirstProvider.Provider
set @sdk-myfirstprovider-web-dir=%@sdk-myfirstprovider-dir%\Relativity.IntegrationPoints.MyFirstProvider.Provider.Web

set @source-myfirstprovider-dir=%@myfirstprovider-source-dir%\Relativity.IntegrationPoints.MyFirstProvider.Provider
set @source-myfirstproviderweb-dir=%@myfirstprovider-source-dir%\Relativity.IntegrationPoints.MyFirstProvider.Provider.Web
set @source-myfirstproviderweb-sln=Master.sln
set @source-myfirstprovider-rap=MyFirstProvider.rap

call :initialize

::create sdk folder structure
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
robocopy "%@jsonloader-source-dir%" "%@sdk-jsonloader-dir%" "%@source-jsonloader-sln%" /is
robocopy "%@jsonloader-packages-dir%" "%@sdk-examples-dir%" "%@source-jsonloader-rap%" /is

call :stop-with-message-if-any-robocopy-error "Error while copying JsonLoader"

robocopy /e "%@source-myfirstprovider-dir%" "%@sdk-myfirstprovider-provider-dir%" /is /xd "bin" "obj"
robocopy /e "%@source-myfirstproviderweb-dir%" "%@sdk-myfirstprovider-web-dir%" /is /xd "bin" "obj"
robocopy "%@myfirstprovider-source-dir%" "%@sdk-myfirstprovider-dir%" "%@source-myfirstproviderweb-sln%" /is
robocopy "%@myfirstprovider-packages-dir%" "%@sdk-examples-dir%" "%@source-myfirstprovider-rap%" /is

call :stop-with-message-if-any-robocopy-error "Error while copying MyFirstProvider"

::pack provider - dlls
for /l %%i in (0,1,%@sdk-dlls-count%) do (
 robocopy /is "%@source-bin-dir%" "%@sdk-provider-dir%" "!@sdk-dlls[%%i]!"
)

call :stop-with-message-if-any-robocopy-error "Error while copying DLLs"

::pack web files
for /l %%i in (0,1,%@sdk-web-scripts-count%) do (
 robocopy /is "%@source-web-scripts-dir%" "%@sdk-web-dir%" "!@sdk-web-scripts[%%i]!"
)

call :stop-with-message-if-any-robocopy-error "Error while copying web scripts"

::zip SDK
C:\"Program Files"\7-Zip\7z.exe a -tzip "%@sdk-main-dir-name%.zip" -r "%@sdk-main-dir-name%"

call :stop-with-message-if-any-error "Error while zipping"

call :cleanup

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
 call :echo-red "Json Loader packages path is not provided."
)
if not exist "%2" (
 set valid=false
 call :echo-red "Json Loader packages path does not exist."
)
if "%3"=="empty" ( 
 set valid=false
 call :echo-red "Json Loader source code path is not provided."
)
if not exist "%3" (
  set valid=false
  call :echo-red "Json Loader source code path does not exist."
)
if "%4"=="empty" (
 set valid=false
 call :echo-red "My First Provider packages path is not provided."
)
if not exist "%4" (
 set valid=false
 call :echo-red "My First Provider packages path does not exist."
)
if "%5"=="empty" ( 
 set valid=false
 call :echo-red "My First Provider source code path is not provided."
)
if not exist "%5" (
  set valid=false
  call :echo-red "My First Provider source code path does not exist."
)

if "%valid%"=="false" (
 call :echo-red "Invalid parameters passed. Type --help for more info."
 call :halt 2> nul
)
exit /b 0

:initialize
C:\"Program Files"\7-Zip\7z.exe x "%@jsonloader-packages-dir%/%@source-jsonloader-rap%" -o"./%@unzipped-rap-folder-name%"
exit /b 0

:cleanup
rmdir /s /q "%@unzipped-rap-folder-name%"
rmdir /s /q "%@sdk-main-dir-name%"
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
echo    .\make-sdk.bat [Options] [Version] [JsonLoaderBuildPackagesPath] [JsonLoaderSourcePath] [MyFirstProviderBuildPackagesPath] [MyFirstProviderSourcePath]
echo.
echo SAMPLE USAGE
echo    .\make-sdk.bat 11.0.16.8 \\bld-pkgs.kcura.corp\Packages\integrationpoints-jsonloader\master\1.0.0 S:\integrationpoints-jsonloader\Source \\bld-pkgs.kcura.corp\Packages\integrationpoints-myfirstprovider\master\1.0.0 S:\integrationpoints-myfirstprovider\Source
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
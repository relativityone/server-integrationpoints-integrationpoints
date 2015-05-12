@echo off

SET BUILDCONFIG=Debug
SET BUILDTYPE=DEV
SET VERSION=1.0.0.0
SET COMPANY = 'kCura LLC'
SET PRODUCT = 'Template'
SET PRODUCTDESCRIPTION = 'Template repo for kCura'
SET BUILD=True
SET APPS=True
SET TEST=False
SET NUGET=False
SET PACKAGE=False

:setArgs
 if "%1"=="" goto doneSetArgs
 
 if "%1" == "/?" goto help
 if "%1" == "-?" goto help
 if "%1" == "help" goto help
 
 if /i %1==debug SET BUILDCONFIG=Debug
 if /i %1==release SET BUILDCONFIG=Release

 if /i %1==/v (
	shift /1
	SET VERSION=%2
	)
	
 if /i %1==/b (
	shift /1
	SET BUILDTYPE=%2
	)
	
 if /i %1==/apps SET BUILD=False
 if /i %1==/noapps SET APPS=False
 if /i %1==/test SET TEST=True
 if /i %1==/nuget SET NUGET=True
 if /i %1==/package SET PACKAGE=True
 
 shift /1
 goto setArgs
:doneSetArgs

echo.
echo buildconfig is %BUILDCONFIG%
echo buildtype is %BUILDTYPE%
echo version is %VERSION%
echo build   step is set to %BUILD%
echo apps    step is set to %APPS%
echo test    step is set to %TEST%
echo nuget   step is set to %NUGET%
echo package step is set to %PACKAGE%



for /f "delims=" %%A in ('hg root') do @set SourceRoot=%%A
pushd %SourceRoot%\DevelopmentScripts

if NOT %VERSION%==1.0.0.0 goto version
goto build

:version
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\psake-version.ps1 -properties @{'version'='%VERSION%';'company'='%COMPANY%';'product'='%PRODUCT%';'product_description'='%PRODUCTDESCRIPTION%';}; exit !$psake.build_success;}"
if NOT %errorlevel%==0 goto end

:build
if %BUILD%==False goto apps
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\psake-build.ps1 -properties @{'version'='%VERSION%';'server_type'='local';'build_config'='%BUILDCONFIG%';'build_type'='%BUILDTYPE%';}; exit !$psake.build_success;}"
if NOT %errorlevel%==0 goto end

:apps
if %APPS%==False goto test
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\psake-application.ps1 -properties @{'version'='%VERSION%';'server_type'='local';'build_config'='%BUILDCONFIG%';'build_type'='%BUILDTYPE%';}; exit !$psake.build_success;}"
if NOT %errorlevel%==0 goto end

:test
if %TEST%==False goto nuget
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\psake-test.ps1; exit !$psake.build_success;}"
if NOT %errorlevel%==0 goto end

:nuget
if %NUGET%==False goto package
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\psake-nuget.ps1 -properties @{'version'='%VERSION%';'server_type'='local';'build_config'='%BUILDCONFIG%';'build_type'='%BUILDTYPE%';}; exit !$psake.build_success;}"
if NOT %errorlevel%==0 goto end

:package
if %PACKAGE%==False goto end
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\psake-package.ps1 -properties @{'version'='%VERSION%';'server_type'='local';'build_config'='%BUILDCONFIG%';'build_type'='%BUILDTYPE%';}; exit !$psake.build_success;}"

goto end

:help
echo.
echo Use this batch file to peform a full build of all projects.
echo This build is the same as the build that happens on the TeamCity server. 
echo. 
echo usage: %0 [debug^|release] [/v VERSION] [/b BUIDLTYPE] [/apps] [/noapps] [/test] [/nuget] [/package]
echo.
echo options:
echo.
echo /v             sets the version # for the build, default is 1.0.0.0 (example: 1.3.3.7)
echo /b             sets the build type for the build, default is DEV. Available options are
echo                       DEV, ALPHA, BETA, RC, GOLD             
echo /apps          skips the build step, continues to only build apps
echo /noapps        does not build apps
echo /test          runs nunit test with the build
echo /nuget         runs the nuget pack step
echo /package       runs the package step
echo.
goto end

:end
popd

@echo off

SET BUILDCONFIG=Debug
SET BUILDTYPE=DEV
SET VERSION=1.0.0.0
SET BUILD=True
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
	
 if /i %1==/nobuild SET BUILD=False
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
echo test    step is set to %TEST%
echo nuget   step is set to %NUGET%
echo package step is set to %PACKAGE%



for /f "delims=" %%A in ('hg root') do @set SourceRoot=%%A
pushd %SourceRoot%\DevelopmentScripts
echo root is %SourceRoot%

SET BUILDPROJECT=%SourceRoot%\DevelopmentScripts\build.build

if NOT %VERSION%==1.0.0.0 goto version
goto build

:version
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\build.ps1 -properties @{'STEP'='version';version=%VERSION%"}}"
if NOT %errorlevel%==0 goto end

:build
if %BUILD%==False goto test
powershell -Command "& { Import-Module ..\Vendor\psake\tools\psake.psm1; Invoke-psake .\build.ps1 -properties @{'STEP'='build';'version'='%VERSION%';'server_type'='local';'build_config'='%BUILDCONFIG%';'build_type'='%BUILDTYPE%';}}"
if NOT %errorlevel%==0 goto end

:test
if %TEST%==False goto nuget
nant start_tests -buildfile:"%BUILDPROJECT%" "-D:root=%SourceRoot%"
if NOT %errorlevel%==0 goto end

:nuget
if %NUGET%==False goto package
nant build_NuGet -buildfile:"%BUILDPROJECT%" "-D:root=%SourceRoot%" "-D:buildconfig=%BUILDCONFIG%" "-D:buildType=%BUILDTYPE%" "-D:serverType=local" "-D:version=%VERSION%"
if NOT %errorlevel%==0 goto end

:package
if %PACKAGE%==False goto end
nant package -buildfile:"%BUILDPROJECT%" "-D:root=%SourceRoot%" "-D:buildconfig=%BUILDCONFIG%" "-D:buildType=%BUILDTYPE%" "-D:serverType=local" "-D:version=%VERSION%"

goto end

:help
echo.
echo Use this batch file to peform a full build of all BuildTools projects.
echo This build is the same as the build that happens on the TeamCity server. 
echo. 
echo usage: %0 [debug^|release] [/v VERSION] [/b BUIDLTYPE] [/nobuild] [/test] [/nuget] [/package]
echo.
echo options:
echo.
echo /v             sets the version # for the build, default is 1.0.0.0 (example: 1.3.3.7)
echo /b             sets the build type for the build, default is DEV. Available options are
echo                       DEV, ALPHA, BETA, RC, GOLD             
echo /nobuild       skips the build step 
echo /test          runs nunit test with the build
echo /nuget         runs the nuget pack step
echo /package       runs the package step
echo.
goto end

:end
popd

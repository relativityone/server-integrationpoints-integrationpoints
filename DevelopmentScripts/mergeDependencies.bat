@ECHO OFF

SET BUILDCONFIG=%1
SET DEPENDENCYVERSION=%2
SET ROOT=%~dp0..
SET BUILDPROJECT=%~dp0build.build

ECHO %BUILDCONFIG%
ECHO %VERSION%
ECHO %ROOT%
ECHO %BUILDPROJECT%

nant merge_main -buildfile:"%BUILDPROJECT%" "-D:root=%Root%" "-D:buildconfig=%BUILDCONFIG%" "-D:relativityDependencyVersion=%DEPENDENCYVERSION%" -nologo
@echo off
SET Root=%LDAP%
echo.Dropping kCura.IntegrationPoints.dll into localhost
echo.

	pushd %LDAP%\Dependencies
	RelativityDLLDropper.exe localhost ..\bin\Debug\kCura.IntegrationPoints.dll eddsdbo edds DCF6E9D1-22B6-4DA3-98F6-41381E93C30C"
	popd

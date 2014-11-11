@echo off
 
echo PACKAGECLIENT: BEGIN

echo PACKAGECLIENT: Signing all exe and dll files under: %sourceLDAPDirectory%
echo PACKAGECLIENT: Signing Start Time: %time%

pushd %sourceLDAPDirectory%
	for /R %%i in ("*.dll","*.exe") do (
		echo Checking if we need to sign file: "%%i"
		"%SIGNTOOL%" verify /pa "%%i"
		if ERRORLEVEL 1 (
			echo Attempting to sign file: "%%i"
			"%SIGNTOOL%" sign /a /t http://timestamp.verisign.com/scripts/timstamp.dll /d Relativity /du http://www.kcura.com "%%i"
			rem ERRORLEVEL 1 means that there was an ERROR, while 0 means NO ERROR
			if ERRORLEVEL 1 (
				echo SIGNTOOL ERROR occured, retrying to sign file %%i
				"%SIGNTOOL%" sign /a /t http://timestamp.verisign.com/scripts/timstamp.dll /d Relativity /du http://www.kcura.com "%%i"
				if ERRORLEVEL 1 (
					echo SIGNTOOL ERROR occured, retrying to sign file %%i
					"%SIGNTOOL%" sign /a /t http://timestamp.verisign.com/scripts/timstamp.dll /d Relativity /du http://www.kcura.com "%%i"
				)
			)
		) ELSE (
			echo No need to sign already signed file: "%%i"
		)
	)
popd

echo PACKAGECLIENT: Signing End Time: %time%

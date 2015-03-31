@echo off

set files=%~1
set SIGNTOOL=%~2

SET RETURN=0

echo "filepath is %files%"
echo "signtool is %SIGNTOOL%"

setlocal enabledelayedexpansion
call :parse "%files%"
if not !RETURN! == 0 (EXIT /B !RETURN!)

goto eos

:parse

set list=%1
set list=%list:"=%

FOR /f "tokens=1* delims=;" %%a IN ("%list%") DO (
  if not "%%a" == "" call :sub %%a  
  if not !RETURN! == 0 (EXIT /B !RETURN!)  
  if not "%%b" == "" call :parse "%%b"  
)
goto eos

:sub 

SET filePath=%1

FOR /L %%A IN (1,1,3) DO (
	echo SIGNTOOL: Checking if we need to sign file: "%filePath%"
	"%SIGNTOOL%" verify /pa /q "%filePath%"
	IF ERRORLEVEL 1 (	
		echo SIGNTOOL: Attempting to sign file: "%filePath%" using http://timestamp.verisign.com/scripts/timstamp.dll 
		"%SIGNTOOL%" sign /a /t http://timestamp.verisign.com/scripts/timstamp.dll /d Relativity /du http://www.kcura.com "%filePath%"
		rem ERRORLEVEL 1 means that there was an ERROR, while 0 means NO ERROR
		IF ERRORLEVEL 1 (
			echo SIGNTOOL: retrying to sign file "%filePath%" using http://timestamp.comodoca.com/authenticode
			"%SIGNTOOL%" sign /a /t http://timestamp.comodoca.com/authenticode /d Relativity /du http://www.kcura.com "%filePath%"
			IF ERRORLEVEL 1 (
				echo SIGNTOOL: retrying to sign file "%filePath%" using http://www2.trustcenter.de/codesigning/timestamp
				"%SIGNTOOL%" sign /a /t http://www2.trustcenter.de/codesigning/timestamp /d Relativity /du http://www.kcura.com "%filePath%"				
				IF ERRORLEVEL 1 (
					SET RETURN=!ERRORLEVEL!
				) ELSE (SET RETURN=0)
			) ELSE (SET RETURN=0)				
		) ELSE (SET RETURN=0)
	) ELSE (
		SET RETURN=0
		echo SIGNTOOL: No need to sign an already signed file: "%filePath%"
		GOTO END)
	rem This is a ping command which allows the script to sleep for 1 second and will work on all versions of windows
	ping -n 1 127.0.0.1 > nul	
)

goto eos
:eos

endlocal

goto END

:END
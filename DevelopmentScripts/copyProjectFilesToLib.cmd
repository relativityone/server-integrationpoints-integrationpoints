SET solutionDir=%1
SET projectTargetDir=%2
SET filesToCopy=%3

SET result=0

ECHO robocopy %projectTargetDir% %solutionDir%..\lib /ndl /njh /njs /np /r:10 /w:10 /mt:1 /if %filesToCopy%.dll %filesToCopy%.pdb

robocopy %projectTargetDir% %solutionDir%..\lib /ndl /njh /njs /np /r:10 /w:10 /mt:1 /if %filesToCopy%.dll %filesToCopy%.pdb

REM ERRORLEVEL > 7 means error
IF %ERRORLEVEL% GTR 7 SET result=%ERRORLEVEL%

EXIT /B %result%
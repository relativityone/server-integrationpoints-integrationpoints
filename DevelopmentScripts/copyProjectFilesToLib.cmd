SET solutionDir=%1
SET projectTargetDir=%2
SET filesToCopy=%3

SET result=0

ECHO robocopy %projectTargetDir% %solutionDir%..\lib %filesToCopy%.dll %filesToCopy%.pdb /NDL /NJH /NJS /NP /R:10 /W:10

robocopy %projectTargetDir% %solutionDir%..\lib %filesToCopy%.dll %filesToCopy%.pdb /NDL /NJH /NJS /NP /R:10 /W:10

REM ERRORLEVEL > 7 means error
IF %ERRORLEVEL% GTR 7 SET result=%ERRORLEVEL%

EXIT /B %result%
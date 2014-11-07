@echo off

echo PACKAGEMASTER: BEGIN
 
echo PACKAGEMASTER: Calling package scripts
mkdir "%revisionDirectory%"

echo f | xcopy "%basePath%\bin\Release\*" "%revisionDirectory%\" /EY /EXCLUDE:packageCreationExcludeList.txt

:END
echo PACKAGEMASTER: END
@echo off

echo PACKAGEMASTER: BEGIN
 
echo PACKAGEMASTER: Calling package scripts
mkdir "%revisionDirectory%"
mkdir "%revisionDirectory%\web"

echo f | xcopy "%basePath%\bin\Release\*" "%revisionDirectory%\" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\publish\bin\web\Release\*" "%revisionDirectory%\web\" /EY /EXCLUDE:packageCreationExcludeList.txt

:END
echo PACKAGEMASTER: END
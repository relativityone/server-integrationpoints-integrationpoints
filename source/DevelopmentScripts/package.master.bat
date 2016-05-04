@echo off

echo PACKAGEMASTER: BEGIN

echo PACKAGEMASTER: Calling package scripts

mkdir "%revisionDirectory%"
mkdir "%revisionDirectory%\web"

echo f | xcopy "%basePath%\bin\Application\*" "%revisionDirectory%\Application\" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\bin\Release\*.dll;*.xml*" "%revisionDirectory%\dlls" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\bin\*.pdb" "%revisionDirectory%\pdbs" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\publish\bin\web\Release\*" "%revisionDirectory%\web\" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\nugspec\*.nupkg" "%revisionDirectory%\%buildType% %version% NuGet Packages - INTERNAL USE ONLY" /E /I

:END
echo PACKAGEMASTER: END
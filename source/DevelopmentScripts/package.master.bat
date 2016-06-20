@echo off

echo PACKAGEMASTER: BEGIN

echo PACKAGEMASTER: Calling package scripts

mkdir "%revisionDirectory%"
mkdir "%revisionDirectory%\web"
mkdir "%revisionDirectory%\dlls"
mkdir "%revisionDirectory%\pdbs"
mkdir "%revisionDirectory%\SDK"

echo f | xcopy "%basePath%\bin\Application\*" "%revisionDirectory%\Application\" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\bin\*.dll" "%revisionDirectory%\dlls" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\bin\*.config" "%revisionDirectory%\dlls" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\bin\*.pdb" "%revisionDirectory%\pdbs" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\publish\bin\web\*" "%revisionDirectory%\web\" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\nugspec\*.nupkg" "%revisionDirectory%\%buildType% %version% NuGet Packages - INTERNAL USE ONLY" /E /I
echo f | xcopy "%basePath%\bin\sdk\*" "%revisionDirectory%\dlls\sdk\" /EY /EXCLUDE:packageCreationExcludeList.txt
echo f | xcopy "%basePath%\bin\IntegrationPoints.SDK.zip" "%revisionDirectory%\sdk\" /Y /EXCLUDE:packageCreationExcludeList.txt

:END
echo PACKAGEMASTER: END
.\packages\OpenCover.4.5.3522\OpenCover.Console.exe -register:user "-filter:+[Bom]* -[*Test]*" "-target:.\packages\NUnit.Runners.2.6.3\tools\nunit-console-x86.exe" "-targetargs:/noshadow .\BomTest\bin\Debug\BomTest.dll"

.\packages\ReportGenerator.1.9.1.0\ReportGenerator.exe "-reports:results.xml" "-targetdir:.\coverage"

pause
SET LDAPSyncRoot=%LDAPSync%
SET OpenCoverRoot=%LDAPSyncRoot%\packages\OpenCover.4.5.3522
SET ReportGeneratorRoot=%LDAPSyncRoot%\packages\ReportGenerator.2.1.1.0
SET NUnitAppRoot=%LDAPSyncRoot%\packages\NUnit.Runners.2.6.4\tools
SET NUnitTestsRoot=%LDAPSyncRoot%\bin\UnitTests

"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.Agent.Tests\bin\kCura.IntegrationPoints.Agent.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage1.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.Core.Tests\bin\kCura.IntegrationPoints.Core.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage2.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.Data.Tests\bin\kCura.IntegrationPoints.Data.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage3.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.EventHandlers.Tests\bin\kCura.IntegrationPoints.EventHandlers.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage4.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.LDAPProvider.Tests\bin\kCura.IntegrationPoints.LDAPProvider.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage5.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.Synchronizers.RDO.Tests\bin\kCura.IntegrationPoints.Synchronizers.RDO.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage6.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.IntegrationPoints.Web.Tests\bin\kCura.IntegrationPoints.Web.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage7.xml"
"%OpenCoverRoot%\OpenCover.Console.exe" -register:user -target:"%NUnitAppRoot%\nunit-console.exe" -targetargs:"/noshadow %LDAPSync%\kCura.ScheduleQueue.Core.Tests\bin\kCura.ScheduleQueue.Core.Tests.dll" -filter:"+[*]* -[*]*.Tests.*" -output:"%NUnitTestsRoot%\coverage8.xml"

"%ReportGeneratorRoot%\ReportGenerator.exe" -reports:"%NUnitTestsRoot%\coverage1.xml;%NUnitTestsRoot%\coverage2.xml;%NUnitTestsRoot%\coverage3.xml;%NUnitTestsRoot%\coverage4.xml;%NUnitTestsRoot%\coverage5.xml;%NUnitTestsRoot%\coverage6.xml;%NUnitTestsRoot%\coverage7.xml;%NUnitTestsRoot%\coverage8.xml;" -targetdir:"%LDAPSyncRoot%\TestReports\"
FormatTaskName "------- Executing Task: {0} -------"

properties {
    $SourceDir = Join-Path $PSScriptRoot "source"
    $Solution = ((Get-ChildItem -Path $SourceDir -Filter *.sln -File)[0].FullName)
    $ArtifactsDir = Join-Path $PSScriptRoot "Artifacts"
    $LogsDir = Join-Path $ArtifactsDir "Logs"
    $LogFilePath = Join-Path $LogsDir "buildsummary.log"
    $ErrorLogFilePath = Join-Path $LogsDir "builderrors.log"
    $PaketExe = Join-Path $PSScriptRoot ".paket\paket.exe"
}

Task default -Depends Analyze, Compile, Test, Package -Description "Build and run unit tests. All the steps for a local build.";

Task Analyze -Description "Run build analysis" {
    # Leaving this blank until we are ready to add in analyzers later
}

Task NugetRestore -Description "Restore the packages needed for this build" {
    exec { & $PaketExe restore }
    exec { dotnet restore $Solution }
}

Task Compile -Depends NugetRestore -Description "Compile code for this repo" {
    Initialize-Folder $ArtifactsDir -Safe
    Initialize-Folder $LogsDir -Safe

    dotnet --info
    exec { msbuild @($Solution,
        ("/target:Build"),
        ("/property:Configuration=$BuildConfig"),
        ("/consoleloggerparameters:Summary"),
        ("/property:PublishWebProjects=True"),
        ("/nodeReuse:False"),
        ("/maxcpucount"),
        ("/nologo"),
        ("/fileloggerparameters1:LogFile=`"$LogFilePath`""),
        ("/fileloggerparameters2:errorsonly;LogFile=`"$ErrorLogFilePath`""))
    }
}

Task Test -Description "Run tests that don't require a deployed environment." {
    $LogPath = Join-Path $LogsDir "UnitTestResults.xml"
    Invoke-Tests -WhereClause "cat == Unit || namespace =~ Relativity.IntegrationPoints.Tests.Unit" -OutputFile $LogPath -WithCoverage

    $LogPath = Join-Path $LogsDir "CI_IntegrationTestResults.xml"
    Invoke-Tests -WhereClause "namespace =~ Relativity.IntegrationPoints.Tests.Integration" -OutputFile $LogPath -WithCoverage
}

Task FunctionalTest -Depends OneTimeTestsSetup -Description "Run tests that require a deployed environment." {
    $LogPath = Join-Path $LogsDir "FunctionalTestResults.xml"
    Invoke-Tests -WhereClause "(namespace =~ FunctionalTests || namespace =~ Tests\.Integration$ || namespace =~ Tests\.Integration[\.] || namespace =~ E2ETests) && cat != NotWorkingOnTrident" -OutputFile $LogPath

    $LogPath = Join-Path $LogsDir "CI_FunctionalTestResults.xml"
    Invoke-Tests -WhereClause "namespace =~ Relativity.IntegrationPoints.Tests.Functional.CI" -OutputFile $LogPath -WithCoverage
}

Task Sign -Description "Sign all files" {
    Get-ChildItem $SourceDir -recurse | Where-Object {$_.Directory.Name -eq "bin" -and @(".dll",".msi",".exe") -contains $_.Extension} | Select-Object -expand FullName | Set-DigitalSignature -ErrorAction Stop
}

Task Package -Description "Package up the build artifacts" {
    $buildTools = Join-Path $PSScriptRoot "buildtools"
    $developmentScripts = Join-Path $PSScriptRoot "DevelopmentScripts"
    $RAPBuilder = Join-Path $buildTools "Relativity.RAPBuilder\tools\Relativity.RAPBuilder.exe"
    $BuildXML = Join-Path $developmentScripts "build.xml"

     exec { & $NuGetEXE install "Relativity.RAPBuilder" "-ExcludeVersion" -o $buildTools }

    exec { & $RAPBuilder `
        "--source" "$PSScriptRoot" `
        "--input" "$BuildXML" `
        "--version" "$RAPVersion"
    }

    Get-ChildItem -Path $ArtifactsDir -Filter *.nuspec |
    ForEach-Object {
        exec { & $NugetExe pack $_.FullName -OutputDirectory (Join-Path $ArtifactsDir "NuGet") -Version $PackageVersion }
    }

    Save-PDBs -SourceDir $SourceDir -ArtifactsDir $ArtifactsDir
}

Task Clean -Description "Delete build artifacts" {
    Initialize-Folder $ArtifactsDir

    Write-Verbose "Running Clean target on $Solution"
    exec { msbuild @($Solution,
        ("/target:Clean"),
        ("/property:Configuration=$BuildConfig"),
        ("/consoleloggerparameters:Summary"),
        ("/nodeReuse:False"),
        ("/maxcpucount"),
        ("/nologo"))
    }
}

Task Rebuild -Description "Do a rebuild" {
    Initialize-Folder $ArtifactsDir

    Write-Verbose "Running Rebuild target on $Solution"
    exec { msbuild @($Solution,
        ("/target:Rebuild"),
        ("/property:Configuration=$BuildConfig"),
        ("/consoleloggerparameters:Summary"),
        ("/property:PublishWebProjects=True"),
        ("/nodeReuse:False"),
        ("/maxcpucount"),
        ("/nologo"),
        ("/fileloggerparameters1:LogFile=`"$LogFilePath`""),
        ("/fileloggerparameters2:errorsonly;LogFile=`"$ErrorLogFilePath`""))
    }
}

Task Help -Alias ? -Description "Display task information" {
    WriteDocumentation
}

Task OneTimeTestsSetup -Description "Should be run always before running tests that require setup in deployed environment." {
    Move-TestSettings $LogsDir

    $LogPath = Join-Path $LogsDir "OneTimeSetupTestResults.xml"
    Invoke-Tests -WhereClause "cat == OneTimeTestsSetup" -OutputFile $LogPath
}

Task RegTest -Description "Run custom tests based on specified filter on regression environment" {
    Invoke-MyTest
}

Task MyTest -Depends OneTimeTestsSetup -Description "Run custom tests based on specified filter" {
    Invoke-MyTest
}

function Move-TestSettings
{
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [String] $Destination
    )

    $TestSettings = Join-Path $PSScriptRoot FunctionalTestSettings
    if(Test-Path $TestSettings) { Copy-Item -Path $TestSettings -Destination $Destination -Force }

    $VS_TestSettings = Join-Path $PSScriptRoot FunctionalTest.runsettings
    if(Test-Path $VS_TestSettings) { Copy-Item -Path $VS_TestSettings -Destination $Destination -Force }
}

function Invoke-MyTest
{
    $LogPath = Join-Path $LogsDir "MyTestResults.xml"
    Invoke-Tests -WhereClause $TestFilter -OutputFile $LogPath
}

function Invoke-Tests
{
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [String] $WhereClause,
        [Parameter()]
        [String] $OutputFile,
        [Parameter()]
        [String] $TestSettings,
        [Parameter()]
        [Switch]$WithCoverage
    )

    $NUnit = Resolve-Path (Join-Path $BuildToolsDir "NUnit.ConsoleRunner\tools\nunit3-console.exe")

    if(!$TestSettings) { $TestSettings = (Join-Path $PSScriptRoot FunctionalTestSettings) }
    $settings = if(Test-Path $TestSettings) { "@$TestSettings" }

    Initialize-Folder $ArtifactsDir -Safe
    Initialize-Folder $LogsDir -Safe
    
    if($WithCoverage)
    {
        $OpenCover = Join-Path $BuildToolsDir "opencover\tools\OpenCover.Console.exe"
        $ReportGenerator = Join-Path $BuildToolsDir "reportgenerator\tools\net47\ReportGenerator.exe"
        $CoveragePath = Join-Path $LogsDir "Coverage.xml"

        exec { & $OpenCover -target:$NUnit -targetargs:"$Solution --where=\`"$WhereClause\`" --noheader --labels=On --skipnontestassemblies --result=$OutputFile $settings" -register:path64 -filter:"+[kCura*]* +[Relativity*]* -[*Tests*]*" -hideskipped:All -output:"$LogsDir\OpenCover.xml" -returntargetcode }
        exec { & $ReportGenerator -reports:"$LogsDir\OpenCover.xml" -targetdir:$LogsDir -reporttypes:Cobertura }
        Move-Item (Join-Path $LogsDir Cobertura.xml) $CoveragePath -Force
    }
    else
    {
        exec { & $NUnit $Solution `
            "--where=`"$WhereClause`"" `
            "--noheader" `
            "--labels=On" `
            "--skipnontestassemblies" `
            "--result=$OutputFile" `
            $settings
        }
    }
}
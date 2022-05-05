FormatTaskName "------- Executing Task: {0} -------"

properties {
    $SourceDir = Join-Path $PSScriptRoot "source"
    $Solution = ((Get-ChildItem -Path $SourceDir -Filter *.sln -File)[0].FullName)
    $ArtifactsDir = Join-Path $PSScriptRoot "Artifacts"
    $LogsDir = Join-Path $ArtifactsDir "Logs"
    $LogFilePath = Join-Path $LogsDir "buildsummary.log"
    $ErrorLogFilePath = Join-Path $LogsDir "builderrors.log"
}

Task default -Depends Analyze, Compile, Test, Package -Description "Build and run unit tests. All the steps for a local build.";

Task Analyze -Description "Run build analysis" {
    # Leaving this blank until we are ready to add in analyzers later
}

Task Compile -Description "Compile code for this repo" {
    Initialize-Folder $ArtifactsDir -Safe
    Initialize-Folder $LogsDir -Safe

    exec { dotnet @("build", $Solution,
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

Task Test -Description "Run Unit and Integration Tests with coverage" {
    $LogPath = Join-Path $LogsDir "TestResults.xml"
    Invoke-Tests -WhereClause "namespace =~ Tests.Unit || namespace =~ Tests.Integration" -OutputFile $LogPath -WithCoverage
}

Task FunctionalTest -Description "Run tests that require a deployed environment." {
    $LogPath = Join-Path $LogsDir "SystemTestResults.xml"
    Invoke-Tests -WhereClause "namespace =~ Tests.System" -OutputFile $LogPath
}

Task Sign -Description "Sign all files" {
    Write-Host "This task will always fail on local environment (by design)`r`n" -ForegroundColor Red

    Get-ChildItem $PSScriptRoot -recurse `
    | Where-Object { $_.Directory.FullName -notmatch "Vendor" -and $_.Directory.FullName -notmatch "packages" -and $_.Directory.FullName -notmatch "buildtools" -and $_.Directory.FullName -notmatch "obj" -and @(".dll", ".msi", ".exe") -contains $_.Extension } `
	| Select-Object -expand FullName `
	| Set-DigitalSignature -ErrorAction Stop
}

Task Package -Description "Package up the build artifacts" {
	Initialize-Folder $ArtifactsDir -Safe
	
	exec { dotnet @("pack", $Solution,
	("--no-build"),
	("/property:Configuration=$BuildConfig"),
	("/consoleloggerparameters:Summary"),
	("/maxcpucount"),
	("/nologo"))
	
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
}

Task Clean -Description "Delete build artifacts" {
    Initialize-Folder $ArtifactsDir

    Write-Verbose "Running Clean target on $Solution"
    exec { dotnet @("msbuild", $Solution,
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
    exec { dotnet @("msbuild", $Solution,
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

Task PerformanceTest -Description "Run performance tests" {
    $LogPath = Join-Path $LogsDir "PerformanceTestResults.xml"
    Invoke-Tests -WhereClause "cat == ReferencePerformance" -OutputFile $LogPath
}

Task MyTest -Description "Run custom tests based on specified filter" {
    Invoke-MyTest
}

Task Help -Alias ? -Description "Display task information" {
    WriteDocumentation
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

        exec { & $OpenCover -target:$NUnit -targetargs:"$Solution --where=\`"$WhereClause\`" --noheader --labels=On --skipnontestassemblies --result=$OutputFile $settings" -register:path64 -filter:"+[Relativity.Sync*]* -[Relativity.Sync.Tests*]**" -hideskipped:All -output:"$LogsDir\OpenCover.xml" -returntargetcode }
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
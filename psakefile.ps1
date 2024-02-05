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

Task NugetRestore -Description "Restore the packages needed for this build" {
   exec { & $NugetExe @('restore', $Solution) }
}

Task BuildNodePackagesJS{
    Set-NodePath
    $JSDir = Join-Path $PSScriptRoot "Source\kCura.IntegrationPoints.Web"

    Invoke-NpmCommand {
        npx @('npm', 'install')
    } -workingDirectory $JSDir
}

Task BuildLiquidFormsJS {   
    Set-NodePath
    $liquidFormsJSDir = Join-Path $PSScriptRoot "Source\kCura.IntegrationPoints.Web\Scripts\RelativityForms"

    Invoke-NpmCommand {
        npx @('npm', '-v')
    } -workingDirectory $liquidFormsJSDir

    Invoke-NpmCommand {
        npx @('npm', 'install')
    } -workingDirectory $liquidFormsJSDir
   
    Invoke-NpmCommand {
        npm @('run', 'build')
    } -workingDirectory $liquidFormsJSDir
}

Task Compile -Depends NugetRestore,BuildNodePackagesJS,BuildLiquidFormsJS -Description "Compile code for this repo" {
    Initialize-Folder $ArtifactsDir -Safe
    Initialize-Folder $LogsDir -Safe

    Get-ChildItem Env:

    dotnet --info
    exec { msbuild @($Solution,
        ("/target:Build"),
        ("/property:Configuration=$BuildConfig"),
        ("/consoleloggerparameters:Summary"),
        ("/property:PublishWebProjects=True"),
        ("/nodeReuse:False"),
        ("/maxcpucount"),
        ("/nologo"),
        ("/fileloggerparameters1:LogFile= $LogFilePath"),
        ("/fileloggerparameters2:errorsonly;LogFile= $ErrorLogFilePath"))
    }

    $publishPath = "$SourceDir\CustomPages\IntegrationPoints"
    if(Test-Path $publishPath) {
        Write-Host "Update web.config file"
        Copy-Item -Path "$SourceDir\kCura.IntegrationPoints.Web\Web.Config" -Destination "$publishPath\Web.Config" 
    }
}

Task Test -Description "Run tests that don't require a deployed environment." {
    $LogPath = Join-Path $LogsDir "TestResults.xml"
    Invoke-Tests -WhereClause "cat == Unit || namespace =~ Relativity.IntegrationPoints.Tests.Unit || namespace =~ Relativity.IntegrationPoints.Tests.Integration" -OutputFile $LogPath -WithCoverage
}

Task FunctionalTest -Description "Run tests that require a deployed environment." {
    $LogPath = Join-Path $LogsDir "FunctionalTestResults.xml"
    
    # REL-865787 : Will be re enable these functional test runs once isolation and release completed.
    # $OneTimeSetupLogPath = Join-Path $LogsDir "OneTimeSetupTestResults.xml"    
    # Invoke-Tests -WhereClause "cat == OneTimeTestsSetup" -OutputFile $OneTimeSetupLogPath
    # Invoke-Tests -WhereClause "(namespace =~ Relativity.IntegrationPoints.FunctionalTests || namespace =~ Tests\.Integration$ || namespace =~ Tests\.Integration[\.] || namespace =~ E2ETests || namespace =~ Relativity.IntegrationPoints.Tests.Functional.CI) && cat != NotWorkingOnTrident" -OutputFile $LogPath

Invoke-Tests -WhereClause "TestType == Critical" -OutputFile $LogPath

}

Task NightlyTest -Depends OneTimeTestsSetup -Description "Run Nightly tests that require a deployed environment." {
    $LogPath = Join-Path $LogsDir "NightlyTestResults.xml"
    Invoke-Tests -WhereClause "(namespace =~ Relativity.IntegrationPoints.FunctionalTests || namespace =~ Tests\.Integration$ || namespace =~ Tests\.Integration[\.] || namespace =~ E2ETests || namespace =~ Relativity.IntegrationPoints.Tests.Functional.CI) && cat != NotWorkingOnTrident" -OutputFile $LogPath
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

    Write-Host "Running Clean target on $Solution"
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

    Write-Host "Running Rebuild target on $Solution"
    exec { msbuild @($Solution,
        ("/target:Rebuild"),
        ("/property:Configuration=$BuildConfig"),
        ("/consoleloggerparameters:Summary"),
        ("/property:PublishWebProjects=True"),
        ("/nodeReuse:False"),
        ("/maxcpucount"),
        ("/nologo"),
        ("/fileloggerparameters1:LogFile= $LogFilePath"),
        ("/fileloggerparameters2:errorsonly;LogFile= $ErrorLogFilePath"))
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

Task MyTest -Depends OneTimeTestsSetup -Description "Run custom tests based on specified filter" {
    Invoke-MyTest
}

Task RegressionTest -Description "Regression Tests against one of Ring-0 Environments" {
    Move-TestSettings $LogsDir

    $LogPath = Join-Path $LogsDir "RegressionTestsResults.xml"
    Invoke-Tests -WhereClause "namespace =~ Relativity.IntegrationPoints.Tests.Functional.CI_REG" -OutputFile $LogPath
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

        exec { & $OpenCover -target:$NUnit -targetargs:"$Solution --where=`"$WhereClause`" --noheader --labels=On --skipnontestassemblies --result=$OutputFile $settings" -register:path64 -filter:"+[kCura*]* +[Relativity*]* -[*Tests*]*" -hideskipped:All -output:"$LogsDir\OpenCover.xml" -returntargetcode }
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
function Set-NodePath {
    $pathToNode = [System.IO.Path]::Combine("buildtools", 'Portable.NodeJS', 'tools', 'win-x64')
    $resolvedPathToNode = Resolve-Path "$pathToNode" | Select-Object -ExpandProperty Path
    
    if (-Not ($env:Path -like "*$resolvedPathToNode*")) {
        Write-Output "Append PATH with $resolvedPathToNode"
        exec {
            $env:Path += ';' + $resolvedPathToNode
        }
    }
    else
    {
        Write-Output "Path to node already set"
    }
}
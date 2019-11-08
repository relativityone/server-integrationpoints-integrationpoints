FormatTaskName "------- Executing Task: {0} -------"

properties {
    $SourceDir = Join-Path $PSScriptRoot "source"
    $Solution = ((Get-ChildItem -Path $SourceDir -Filter *.sln -File)[0].FullName)
    $ArtifactsDir = Join-Path $PSScriptRoot "Artifacts"
    $LogsDir = Join-Path $ArtifactsDir "Logs"
    $LogFilePath = Join-Path $LogsDir "buildsummary.log"
    $ErrorLogFilePath = Join-Path $LogsDir "builderrors.log"
    $ScriptsDir = Join-Path $PSScriptRoot "scripts"
    $ToolsDir = Join-Path $PSScriptRoot "buildtools"
    $PaketExec = Join-Path $PSScriptRoot ".paket/paket.exe"
}

Task default -Depends Analyze, Compile, Test, Package -Description "Build and run unit tests. All the steps for a local build.";

Task RestoreAnaylyzeTools -Description "Download tools for ConfigureAwaitChecker" {
    Write-Host "ScriptsDir: " $ScriptsDir
    $nugetExe = Join-Path $ToolsDir nuget.exe

    Write-Host "nuget: " $nugetExe
    & (Join-Path $ScriptsDir "restore-buildtools.ps1") -toolsDir $ToolsDir -nugetExe $nugetExe
}

Task Analyze -Description "Run build analysis" -depends RestoreAnaylyzeTools {
    
}

Task Compile -Description "Compile code for this repo" {
    Initialize-Folder $ArtifactsDir -Safe
    Initialize-Folder $LogsDir -Safe
    
	dotnet --info
    exec { dotnet @("build", $Solution,
            ("/property:Configuration=$BuildConfig"),
            ("/consoleloggerparameters:Summary"),
            ("/nodeReuse:False"),
            ("/maxcpucount"),
            ("/nologo"),
            ("/fileloggerparameters1:LogFile=`"$LogFilePath`""),
            ("/fileloggerparameters2:errorsonly;LogFile=`"$ErrorLogFilePath`"")) 
    }
}

Task Test -Description "Run unit tests that don't require a deployed environment." {
    $TestResultsPath = Join-Path $LogsDir "{assembly}.{framework}.TestResults.xml"
    $CoveragePath = Join-Path $LogsDir "Coverage.xml"
    exec { & dotnet @("test", $Solution,
            ("/p:collectcoverage=true"),
            ("/p:CoverletOutputFormat=cobertura"),
            ("--logger:nunit;LogFilePath=$TestResultsPath"))
    }

    $coverageReports = [string]::Empty
    Get-ChildItem $PSScriptRoot -Filter "coverage.cobertura.xml" -Recurse | ForEach-Object { $coverageReports += "$($_.FullName);" }
    exec { & $ReportGenerator @(
            ("-reports:$coverageReports"),
            ("-targetdir:$LogsDir"),
            ("-reporttypes:Cobertura"))
    }
    Move-Item (Join-Path $LogsDir Cobertura.xml) $CoveragePath -Force
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
    }
    Save-PDBs -SourceDir $SourceDir -ArtifactsDir $ArtifactsDir
}

Task PackagePaket -Description "Package up the build artifacts" {
    Initialize-Folder $ArtifactsDir -Safe

    & $PaketExec pack (Join-Path $ArtifactsDir "NuGet") --include-referenced-projects --symbols

    Save-PDBs -SourceDir $SourceDir -ArtifactsDir $ArtifactsDir
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
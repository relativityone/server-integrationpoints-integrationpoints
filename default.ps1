#Requires -Version 5.0

FormatTaskName "------- Executing Task: {0} -------"

properties {
    $root
    $toolsDir
    $scriptsDir
    $buildConfig
    $buildType
    $version
    $packageVersion
    $nugetExe
    $progetApiKey
    $branchName
    $databaseUser
    $databasePassword
    $sourceDir = Join-Path $root "Source"
    $logsDir = Join-Path $root "buildlogs"
    $paketExe = Join-Path $root -ChildPath ".paket" | Join-Path -ChildPath "paket.exe"
    $nugetOutput = Join-Path $root "nuget"
    $certName = "Relativity ODA LLC"
    $progetUrl = "https://proget.kcura.corp/nuget/NuGet"
    $coverageFileName = "coverage.html"
    $projectName = "Relativity.Sync"
}

task default -depends restorePackages, checkConfigureAwait, build, runUnitTests, packNuget

task sign {
    & (Join-Path $scriptsDir "get-certificate-thumbprint.ps1") -certName $certName
    & (Join-Path $scriptsDir "get-signtool.ps1")
}

task getVersion {
    & (Join-Path $scriptsDir "get-version.ps1") -projectName $projectName -buildType $buildType -scriptsDir $scriptsDir -branchName $branchName `
        -databaseUser $databaseUser -databasePassword $databasePassword
    Write-Output "!!!VERSION=$global:version"
    Write-Output "!!!PACKAGE_VERSION=$global:packageVersion"
}

task restorePackages {
    & (Join-Path $scriptsDir "restore-packages.ps1") -paketExe $paketExe
}

task checkConfigureAwait -depends restorePackages {
    & (Join-Path $scriptsDir "check-configureawait.ps1") -sourceDir $sourceDir -toolsDir $toolsDir -logsDir $logsDir
}

task findMsbuild {
    & (Join-Path $scriptsDir "find-msbuild.ps1")
    Write-Output $global:msbuild_exe
}

task build -depends restorePackages, findMsbuild {
    & (Join-Path $scriptsDir "build-solution.ps1") -buildConf $buildConfig -version $version -packageVersion $packageVersion -sourceDir $sourceDir -certThumbprint $global:certThumbprint -signToolpath $global:signToolPath
}

task buildAndSign -depends sign, build {    
}

task packNuget {
    & (Join-Path $scriptsDir "pack-nuget.ps1") -packageVersion $packageVersion -paketExe $paketExe -nugetOutput $nugetOutput
}

task publishNuget {
    & (Join-Path $scriptsDir "publish-nuget.ps1") -nugetExe $nugetExe -nugetOutput $nugetOutput -certName $certName -url $progetUrl -apiKey $progetApiKey
}

task runUnitTests {
    & (Join-Path $scriptsDir "run-unit-tests.ps1") -projectName $projectName -sourceDir $sourceDir -toolsDir $toolsDir -logsDir $logsDir -coverageFileName $coverageFileName
}

task runIntegrationTests {
    & (Join-Path $scriptsDir "run-tests.ps1") -projectName $projectName -testsType "Integration" -sourceDir $sourceDir -toolsDir $toolsDir -logsDir $logsDir
}

task runPerformanceTests {
    & (Join-Path $scriptsDir "run-tests.ps1") -projectName $projectName -testsType "Performance" -sourceDir $sourceDir -toolsDir $toolsDir -logsDir $logsDir
}

task runSonarScanner -depends restorePackages, findMsbuild {
    & (Join-Path $scriptsDir "run-sonar-scanner.ps1") -projectName $projectName -sourceDir $sourceDir -toolsDir $toolsDir -logsDir $logsDir -version $version -coverageFileName $coverageFileName
}
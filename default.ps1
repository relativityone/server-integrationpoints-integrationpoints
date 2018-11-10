#Requires -Version 5.0

FormatTaskName "------- Executing Task: {0} -------"

properties {
    $root
    $toolsDir
    $scriptsDir
    $buildConfig
    $buildType
    $version
    $nugetExe
    $progetApiKey
    $sourceDir = Join-Path $root "Source"
    $logsDir = Join-Path $root "buildlogs"
    $paketExe = Join-Path $root -ChildPath ".paket" | Join-Path -ChildPath "paket.exe"
    $nugetOutput = Join-Path $root "nuget"
    $certName = "Relativity ODA LLC"
    $progetUrl = "https://proget.kcura.corp/nuget/NuGet"
}

task default -depends build, packNuget

task sign {
    $global:certThumbprint = & (Join-Path $scriptsDir "get-certificate-thumbprint.ps1") -certName $certName
}

task restorePackages {
    & (Join-Path $scriptsDir "restore-packages.ps1") -paketExe $paketExe
}

task checkConfigureAwait {
    & (Join-Path $scriptsDir "check-configureawait") -sourceDir $sourceDir -toolsDir $toolsDir -logsDir $logsDir
}

task build -depends restorePackages, checkConfigureAwait {
    & (Join-Path $scriptsDir "build-solution.ps1") -buildConf $buildConfig -version $version -sourceDir $sourceDir -certThumbprint $global:certThumbprint
}

task packNuget {
    & (Join-Path $scriptsDir "pack-nuget.ps1") -version $version -paketExe $paketExe -nugetOutput $nugetOutput
}

task publishNuget {
    & (Join-Path $scriptDir "publish-nuget.ps1") -nugetExe $nugetExe -nugetOutput $nugetOutput -certName $certName -url $progetUrl -apiKey $progetApiKey
}
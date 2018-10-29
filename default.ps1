#Requires -Version 5.0

FormatTaskName "------- Executing Task: {0} -------"

properties {
    $root
    $toolsDir
    $scriptsDir
    $sourceDir    
    $buildConfig
    $buildType
    $version
    $nugetExe
    $paketExe
}

task default -depends build

task sign {
    $global:certThumbprint = & (Join-Path $scriptsDir "get-certificate-thumbprint.ps1")
}

task restorePackages {
    & (Join-Path $scriptsDir "restore-packages.ps1") -paketExe $paketExe
}

task build -depends restorePackages {
    & (Join-Path $scriptsDir "build-solution.ps1") -buildConf $buildConfig -version $version -sourceDir $sourceDir -certThumbprint $global:certThumbprint
}
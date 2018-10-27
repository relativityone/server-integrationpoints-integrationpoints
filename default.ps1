#Requires -Version 5.0

properties {
    $root
    $toolsDir
    $scriptsDir
    $sourceDir    
    $buildConfig
    $buildType
    $version
}

task default -Depends build

task buildAndSign {
    $certThumbprint = & (Join-Path $scriptsDir "get-certificate-thumbprint.ps1")
    & (Join-Path $scriptsDir "build-solution.ps1") -buildConf $buildConfig -version $version -sourceDir $sourceDir
}

task build {
    & (Join-Path $scriptsDir "build-solution.ps1") -buildConf $buildConfig -version $version -sourceDir $sourceDir
}
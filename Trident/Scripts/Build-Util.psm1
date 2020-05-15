function Set-RegressionSettings ($RegEnv) {
    $TaskRunner = Resolve-Path -Path .\DevelopmentScripts\Reg-TestSettings.ps1
    &($TaskRunner) -RegEnv $RegEnv
}

function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

function Invoke-Test ($TestFilter) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) CustomTest -Configuration Release -TestFilter $TestFilter
}
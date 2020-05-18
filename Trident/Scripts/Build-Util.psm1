function Set-RegressionSettings ($RegEnv) {
    $TaskRunner = Resolve-Path -Path .\DevelopmentScripts\Reg-TestSettings.ps1
    &($TaskRunner) -RegEnv $RegEnv
}
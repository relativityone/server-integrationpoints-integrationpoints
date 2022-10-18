function Set-RegressionSettings ($RegEnv) {
    $TaskRunner = Resolve-Path -Path .\DevelopmentScripts\New-TestSettings.ps1
    &($TaskRunner) -AdditionalRunSettingsFilePath ".\DevelopmentScripts\Regression\$RegEnv.runsettings"
}
function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

Invoke-Task Compile
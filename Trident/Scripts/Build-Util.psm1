function Set-TestSetting {
    param (
        [Parameter]
        [String] $TestSettings,
        [Parameter(Mandatory=$true)]
        [String] $Name,
        [Parameter(Mandatory=$true)]
        [String] $Value
    )

    if(-not $TestSettings) {
        [string]$TestSettings = Join-Path $PSScriptRoot ..\..\FunctionalTestSettings
    }

    if(-not (Test-Path $TestSettings)) {
        Write-Host "FunctionalTestSettings file does not exist."
        return
    }

    $regex = "--params `"$Name=(.*?)`"";
    $param = "--params `"$Name=$Value`""

    $settings = Get-Content $TestSettings
    if($settings -match $regex) {
        $settings -replace $regex, $param | Out-File $TestSettings -Encoding utf8;
    }
    else {
        Add-Content $TestSettings $param;
    }
}


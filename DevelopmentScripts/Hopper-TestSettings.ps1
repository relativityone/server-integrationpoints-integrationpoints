[CmdletBinding()]
param (
    [Parameter()]
    [String]
    $HopperServiceName
)

if(-not $HopperServiceName)
{
    Throw "Hopper Service Name was not specified."
}

$HopperServiceUrl = "$HopperServiceName.relativityhopper.com"

Powershell "$PSScriptRoot\New-TestSettings.ps1" `
    -ServerBindingType https `
    -SqlServer "$HopperServiceUrl\EDDSINSTANCE001" `
    -SqlUsername eddsdbo `
    -SqlPassword "M@y0rQu1mby2!F0r3v3r" `
    -RelativityHostAddress $HopperServiceUrl `
    -AdminUsername "relativity.admin@kcura.com" `
    -AdminPassword "Test1234!"
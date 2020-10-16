[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $TestFilter
)

$TaskRunner = Resolve-Path -Path build.ps1

# Custom
&($TaskRunner) MyTest -Configuration Release -TestFilter $TestFilter
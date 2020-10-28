[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $TestFilter
)

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) Compile, Package, MyTest -Configuration Release -TestFilter $TestFilter
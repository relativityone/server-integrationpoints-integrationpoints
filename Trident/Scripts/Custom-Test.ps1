[CmdletBinding()]
param(
    [Parameter()]
    [string] $TestFilter,
    [Parameter()]
    [Switch] $EmptySUT
)

if($EmptySUT)
{
    throw "Hopper has been saved"
}

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) Compile, Package, MyTest -Configuration Release -TestFilter $TestFilter
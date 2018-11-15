#Requires -Version 5.0

<#
.SYNOPSIS
    Looks for and imports powershell modules required for build process

.PARAMETER toolsDir
    buildtools directory
#>

[CmdletBinding()]
param(
    [string]$toolsDir
)

$modules = "psake.psm1", "PSBuildTools.psm1"

Write-Verbose "Importing modules..."
$modules | ForEach-Object {
    Write-Verbose "Looking for $_ module..."
    $moduleFile = Get-ChildItem -Path $toolsDir -Filter $_ -Recurse -ErrorAction SilentlyContinue -Force
    if ($moduleFile) {
        Write-Verbose "Importing module from $moduleFile..."
        Import-Module $moduleFile.FullName -ErrorAction Stop

        if ($LASTEXITCODE -ne 0) {
            Throw "Unable to load module $_"
        }
    }
    else {
        Throw "Unable to find module $_"
    }
}

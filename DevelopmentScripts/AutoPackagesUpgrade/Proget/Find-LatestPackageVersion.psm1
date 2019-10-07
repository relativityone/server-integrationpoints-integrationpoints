<#  
.SYNOPSIS  
    Finds latest version of specified package

.PARAMETER PackageName
    Name of package
#>

function Find-LatestPackageVersion
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $PackageName
    )

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"
    
    $package = Find-Package $PackageName -Source ProGet
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $package.Version
}

Export-ModuleMember -Function Find-LatestPackageVersion
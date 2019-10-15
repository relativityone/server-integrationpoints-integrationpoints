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

    Install-PackagesSourceIfNotExist
    
    $package = Find-Package $PackageName -Source kCuraArtifactoryNuGet
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $package.Version | Select-Object -First 1
}

function Install-PackagesSourceIfNotExist
{
    $sourceLocation = "https://relativity.jfrog.io/relativity/api/nuget/proget-nuget-remote"
    $repository = Get-PSRepository | Where-Object {$_.SourceLocation -eq $sourceLocation }

    if (-not $repository) 
    {
        Register-PSRepository -Name kCuraArtifactoryNuGet -InstallationPolicy Trusted -SourceLocation $sourceLocation -PublishLocation $sourceLocation
    }
}

Export-ModuleMember -Function Find-LatestPackageVersion
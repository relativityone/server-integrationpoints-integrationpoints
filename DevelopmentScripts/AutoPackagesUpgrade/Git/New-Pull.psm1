<#  
.SYNOPSIS  
    Pulls changes from specified branch

.PARAMETER Path
    Execution path
#>  

function New-Pull
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$False)]
        [string]
        $Path
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils

    if(!$Path)
    {
        $Path = $PSScriptRoot
    }

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"
    
    git -C $Path pull
    
    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-Pull
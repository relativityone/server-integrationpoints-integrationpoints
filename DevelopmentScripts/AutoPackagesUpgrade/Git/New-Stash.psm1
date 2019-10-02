<#  
.SYNOPSIS  
    Creates new stash

.PARAMETER StashName
    New stash name

.PARAMETER Path
    Execution path
#>  

function New-Stash
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $StashName,

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

    git -C $Path stash save $StashName

    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-Stash
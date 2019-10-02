<#  
.SYNOPSIS  
    Finds name of currently checked out branch

.PARAMETER Path
    Execution path
#>

function Find-CurrentBranchName
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

    $currentBranch = git -C $Path rev-parse --abbrev-ref HEAD
    
    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name

    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $currentBranch
}

Export-ModuleMember -Function Find-CurrentBranchName
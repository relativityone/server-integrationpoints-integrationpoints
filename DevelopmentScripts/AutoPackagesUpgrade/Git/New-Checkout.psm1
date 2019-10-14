<#  
.SYNOPSIS  
    Checks out to specified branch

.PARAMETER BranchName
    Branch name to be checkouted

.PARAMETER Path
    Execution path
#>  

function New-Checkout
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $BranchName,

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
    
    git -C $Path rev-parse --verify --quiet $BranchName
    git -C $Path checkout -- .

    if($? -eq $true)
    {
        git -C $Path checkout $BranchName
    }
    else
    {
        git -C $Path checkout -b $BranchName "origin/$BranchName" 
    }
    
    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name

    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-Checkout
<#  
.SYNOPSIS  
    Pushes changes to origin

.PARAMETER BranchName
    Destination branch name

.PARAMETER Path
    Execution path
#>  

function New-Push
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
    
    git -C $Path push origin $BranchName

    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-Push
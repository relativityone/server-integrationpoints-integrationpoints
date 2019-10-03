<#  
.SYNOPSIS  
    Commits changes to checked out branch

.PARAMETER JiraNumber
    Jira number to be included in commit

.PARAMETER Message
    Commit message

.PARAMETER Path
    Execution path
#>  

function New-Commit
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $JiraNumber,

        [Parameter(Mandatory=$True)]
        [string]
        $Message,

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

    $commitMessage = "$JiraNumber $Message"
    git -C $Path commit -m $commitMessage
    
    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name

    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-Commit
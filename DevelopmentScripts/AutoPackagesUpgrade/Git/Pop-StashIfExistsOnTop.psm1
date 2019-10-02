<#  
.SYNOPSIS  
    Pops stash changes if it exists

.PARAMETER StashName
    Stash name

.PARAMETER Path
    Execution path
#>  

function Pop-StashIfExistsOnTop
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

    $stashes = @(git -C $Path stash list)

    if($stashes.Length -gt 0 -and $stashes[0].Contains($StashName))
    {
        Write-Host "Executing stash pop"
        git -C $Path stash pop 'stash@{0}'
    }

    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function Pop-StashIfExistsOnTop
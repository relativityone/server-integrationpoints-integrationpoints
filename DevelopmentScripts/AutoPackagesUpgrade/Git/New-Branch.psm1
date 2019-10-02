<#  
.SYNOPSIS  
    Creates new branch

.PARAMETER ParentBranch
    Source branch name

.PARAMETER BranchName
    New branch name

.PARAMETER Path
    Execution path
#>  

function New-Branch
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $ParentBranch,

        [Parameter(Mandatory=$True)]
        [string]
        $BranchName,

        [Parameter(Mandatory=$False)]
        [string]
        $Path
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config 
    Import-Module -Name $PSScriptRoot\..\Import-Utils
    Import-Module -Name $PSScriptRoot\..\Git\New-Checkout
    Import-Module -Name $PSScriptRoot\..\Git\New-Pull
    Import-Module -Name $PSScriptRoot\..\Git\New-Push
    
    if(!$Path)
    {
        $Path = $PSScriptRoot
    }

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"
    
    try
    {
        Write-Host "Clean"
        git -C $Path clean -dfx 
        Write-Host "GC Auto"
        git -C $Path gc --auto
        Write-Host "Checkout parent"
        New-Checkout -BranchName $ParentBranch -Path $Path
        Write-Host "Pull"
        New-Pull -Path $Path
        Write-Host "Checkout branch"
        git -C $Path checkout -b $BranchName
        Write-Host "Push"
        New-Push -BranchName $BranchName -Path $Path
    }
    catch
    {
        Write-Error "Creating branch failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    Exit-OnAnyErrors -CommandName $MyInvocation.MyCommand.Name
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-Branch
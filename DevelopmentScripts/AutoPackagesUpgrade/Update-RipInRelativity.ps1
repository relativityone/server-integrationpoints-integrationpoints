<#
.SYNOPSIS
    Updates RIP packages in Relativity

.DESCRIPTION
    Execute it from whatever branch you want without need to commit/stash your current changes.
    This script stashes all your changes and restores them when the update is finished.
    By default it will create a PR and put all BVCC members on it.

.PARAMETER ToVersion
    Version of RIP to be updated

.PARAMETER RelativitySourceCodePath
    Path to Relativity's source code

.PARAMETER JiraNumber
    JIRA ticket number that update refers to

.PARAMETER OnBranch
    Branch name that needs to be updated

.PARAMETER SkipPullRequest
    Skips creating a PR to $OnBranch

.EXAMPLE
    .\Update-RipInRelativity.ps1 -ToVersion 10.3.90.11-DEV -OnBranch develop -RelativitySourceCodePath S:\relativity -JiraNumber REL-331932

.EXAMPLE
    .\Update-RipInRelativity.ps1 -OnBranch develop -RelativitySourceCodePath S:\relativity 
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$False)]
    [string]
    $ToVersion,

    [Parameter(Mandatory=$True)]
    [string]
    $OnBranch,

    [Parameter(Mandatory=$True)]
    [string]
    $RelativitySourceCodePath,

    [Parameter(Mandatory=$False)]
    [string]
    $JiraNumber,

    [Parameter(Mandatory=$False)]
    [switch]
    $SkipStashing,

    [Parameter(Mandatory=$False)]
    [switch]
    $SkipPullRequest,

    [Parameter(Mandatory=$False)]
    [pscredential]
    $Credential
)
Begin
{
    Write-Host "Starting..."

    Import-Module -Name $PSScriptRoot\Import-Config
    Import-Module -Name $PSScriptRoot\Import-Utils
    Import-Module -Name $PSScriptRoot\Bitbucket\Find-AlreadyExistingPullRequest
    Import-Module -Name $PSScriptRoot\Bitbucket\New-PullRequest
    Import-Module -Name $PSScriptRoot\Git\Find-CurrentBranchName
    Import-Module -Name $PSScriptRoot\Git\New-Branch
    Import-Module -Name $PSScriptRoot\Git\New-Checkout
    Import-Module -Name $PSScriptRoot\Git\New-Commit
    Import-Module -Name $PSScriptRoot\Git\New-Push
    Import-Module -Name $PSScriptRoot\Git\New-Stash
    Import-Module -Name $PSScriptRoot\Git\Pop-StashIfExistsOnTop
    Import-Module -Name $PSScriptRoot\Jenkins\Find-FirstSuccessfulBuildVersionContainingPrChanges
    Import-Module -Name $PSScriptRoot\Jira\Find-LastJiraWithMergedPrToBranchByLabel
    Import-Module -Name $PSScriptRoot\Jira\Find-OrCreateIfNotExistJiraSubtask
    Import-Module -Name $PSScriptRoot\Jira\Update-JiraStatus
    Import-Module -Name $PSScriptRoot\PackageUpdate\Test-IsRipPackageInRelativityUpToDate
    Import-Module -Name $PSScriptRoot\PackageUpdate\Update-RipPackagesInRelativity

    Write-Host "Beginning of Begin block in $($MyInvocation.MyCommand.Name)"

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    if(!$Credential)
    {
        $Credential = Get-Credential
    }

    $changeIssueStatus = $false
    if(!$ToVersion)
    {
        $lastRipUpdatePackageJira = Find-LastJiraWithMergedPrToBranchByLabel -Credential $Credential -LabelName $RipUpdateJiraLabel -BranchName $OnBranch
        if($null -eq $lastRipUpdatePackageJira)
        {
            Write-Host "Jira issue with updated RIP packages not found!" -ForegroundColor Cyan
            break
        }
        $changeIssueStatus = $true
        $buildVersion = Find-FirstSuccessfulBuildVersionContainingPrChanges -Credential $Credential -JiraKey $lastRipUpdatePackageJira.key -Category DataTransfer -Pipeline IntegrationPoints -Branch $OnBranch
        $ToVersion = Format-JenkinsBuildVersionToRipPackageVersion -BuildVersion $buildVersion
        $JiraNumber = Find-OrCreateIfNotExistJiraSubtask -Credential $Credential -Project REL -ParentIssueKey $lastRipUpdatePackageJira.fields.parent.key -Summary "Update RIP in Relativity on $OnBranch" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RelativityUpdateJiraLabel -Assignee $JiraAssignee
    }

    $commitMessage = "RIP packages updated to $ToVersion"

    $pullRequestAuthor = $JiraAssignee
    $updatePullRequest = Find-AlreadyExistingPullRequest -Credential $Credential -Title $commitMessage -Author $pullRequestAuthor -Project REL -Repository Relativity
    if($updatePullRequest.Count -gt 0)
    {
        Write-Host "Pull request containing updated RIP packages to $ToVersion on $OnBranch is already created!" -ForegroundColor Cyan
        break
    }

    $stashName = "Saved changes before checking packages"
    $initialBranchName = Find-CurrentBranchName -Path $RelativitySourceCodePath
    New-Stash -StashName $stashName -Path $RelativitySourceCodePath
    New-Checkout -BranchName $OnBranch -Path $RelativitySourceCodePath
    $isPackageUpToDate = Test-IsRipPackageInRelativityUpToDate -NewVersion $ToVersion -RelativitySourceCodePath $RelativitySourceCodePath
    New-Checkout -BranchName $initialBranchName -Path $RelativitySourceCodePath
    Pop-StashIfExistsOnTop -StashName $stashName -Path $RelativitySourceCodePath
    if($isPackageUpToDate -eq $true)
    {
        Write-Host "RIP package in Relativity - $ToVersion - is same or higher!" -ForegroundColor Cyan
        break
    }

    if(!$JiraNumber)
    {
        Write-Error "Jira issue key not specified!" -ErrorAction Stop
    }

    Write-Host "End of Begin block in $($MyInvocation.MyCommand.Name)"
}
Process
{
    Write-Host "Beginning of Process block in $($MyInvocation.MyCommand.Name)"

    $updateBranchName = "$JiraNumber-update-rip-in-relativity"
    $stashName = "Saved changes before auto package upgrade"
    
    New-Stash -StashName $stashName -Path $RelativitySourceCodePath
    if($changeIssueStatus -eq $true)
    {
        Update-JiraStatus -Credential $Credential -JiraNumber $JiraNumber -StatusName "In Progress"
    }
    New-Branch -ParentBranch $OnBranch -BranchName $updateBranchName -Path $RelativitySourceCodePath
    Update-RipPackagesInRelativity -ToVersion $ToVersion -RelativitySourceCodePath $RelativitySourceCodePath
    New-Commit -JiraNumber $JiraNumber -Message $commitMessage -Path $RelativitySourceCodePath
    New-Push -BranchName $updateBranchName -Path $RelativitySourceCodePath
    if(!$SkipPullRequest)
    {
        New-PullRequest -Credential $Credential -FromBranch $updateBranchName -ToBranch $OnBranch -Reviewers $PRreviewers -Project REL -Repository Relativity -Title "$JiraNumber $commitMessage"
    }
    if($changeIssueStatus -eq $true)
    {
        Update-JiraStatus -Credential $Credential -JiraNumber $JiraNumber -StatusName Review
    }
    New-Checkout -BranchName $initialBranchName -Path $RelativitySourceCodePath
    Pop-StashIfExistsOnTop -StashName $stashName -Path $RelativitySourceCodePath

    Write-Host "RIP version in Relativity updated successfully to $ToVersion!" -ForegroundColor Green
    
    Write-Host "End of Process block in $($MyInvocation.MyCommand.Name)"
}
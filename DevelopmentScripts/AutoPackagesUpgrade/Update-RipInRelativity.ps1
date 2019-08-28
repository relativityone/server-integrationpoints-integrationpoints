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
	[string]$ToVersion,
	[Parameter(Mandatory=$True)]
	[string]$OnBranch,
	[Parameter(Mandatory=$True)]
	[string]$RelativitySourceCodePath,
	[Parameter(Mandatory=$True)]
	[string]$JiraNumber,
	[Parameter(Mandatory=$False)]
	[switch]$SkipStashing,
	[Parameter(Mandatory=$False)]
	[switch]$SkipPullRequest,
	[Parameter(Mandatory=$False)]
	[pscredential]$Credential
)
Begin
{
	. ".\Config.ps1" 

	Write-Verbose "Beginning of Begin block in Update-RipInRelativity.ps1"

	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

	if(!$Credential)
	{
		$Credential = Get-Credential
	}

	$changeIssueStatus = $false
	if(!$ToVersion)
	{
		$lastRipUpdatePackageJira = .\Jira\Get-LastIssueWithMergedPrToBranchByLabel.ps1 -Credential $Credential -LabelName $RipUpdateJiraLabel -BranchName $OnBranch
		if($null -eq $lastRipUpdatePackageJira)
		{
			Write-Host "Jira issue with updated RIP packages not found!" -ForegroundColor Cyan
			break
		}
		$changeIssueStatus = $true
		$buildVersion = .\Jenkins\Get-FirstSuccessfulBuildVersionContainingPrChanges.ps1 -Credential $Credential -JiraKey $lastRipUpdatePackageJira.key -Category DataTransfer -Pipeline IntegrationPoints -Branch $OnBranch
		$ToVersion = Map-JenkinsBuildVersionToRipPackageVersion -BuildVersion $buildVersion
		$JiraNumber = .\Jira\Create-OrGetIfExistsJiraSubtask.ps1 -Credential $Credential -Project REL -ParentIssueKey $lastRipUpdatePackageJira.parent -Summary "Update RIP in Relativity on $OnBranch" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RelativityUpdateJiraLabel -Assignee $JiraAssignee
	}

	$commitMessage = "RIP packages updated to $ToVersion"

	$pullRequestAuthor = $JiraAssignee
	$updatePullRequestExists = .\Bitbucket\Check-IfPullRequestAlreadyExists.ps1 -Credential $Credential -Title $commitMessage -Author $pullRequestAuthor -Project REL -Repository Relativity
	if($updatePullRequestExists -eq $true)
	{
		Write-Host "Pull request containing updated RIP packages to $ToVersion on $OnBranch is already created!" -ForegroundColor Cyan
		break
	}

	$stashName = "Saved changes before checking packages"
    $initialBranchName = .\Git\Get-CurrentBranchName.ps1 -Path $RelativitySourceCodePath
    .\Git\Stash.ps1 -StashName $stashName -Path $RelativitySourceCodePath
    .\Git\Checkout.ps1 -BranchName $OnBranch -Path $RelativitySourceCodePath
	$isPackageUpToDate = .\PackageUpdate\Is-RipPackageInRelativityUpToDate.ps1 -NewVersion $ToVersion -RelativitySourceCodePath $RelativitySourceCodePath
	.\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RelativitySourceCodePath
	.\Git\Pop-StashIfExistsOnTop.ps1 -StashName $stashName -Path $RelativitySourceCodePath
	if($isPackageUpToDate -eq $true)
	{
		Write-Host "RIP package in Relativity - $ToVersion - is same or higher!" -ForegroundColor Cyan
		break
	}

	if(!$JiraNumber)
	{
		Write-Error "Jira issue key not specified!" -ErrorAction Stop
	}

	Write-Verbose "End of Begin block in Update-RipInRelativity.ps1"
}
Process
{
	Write-Verbose "Beginning of Process block in Update-RipInRelativity.ps1"

	$updateBranchName = "$JiraNumber-update-rip-in-relativity"
	$stashName = "Saved changes before auto package upgrade"
	
	.\Git\Stash.ps1 -StashName $stashName -Path $RelativitySourceCodePath
	if($changeIssueStatus -eq $true)
	{
		.\Jira\Change-IssueStatus.ps1 -Credential $Credential -JiraNumber $JiraNumber -StatusName "In Progress"
	}
	.\Git\Create-Branch.ps1 -ParentBranch $OnBranch -BranchName $updateBranchName -Path $RelativitySourceCodePath
	.\PackageUpdate\Update-RipPackagesInRelativity.ps1 -ToVersion $ToVersion -RelativitySourceCodePath $RelativitySourceCodePath
	.\Git\Commit.ps1 -JiraNumber $JiraNumber -Message $commitMessage -Path $RelativitySourceCodePath
	.\Git\Push.ps1 -BranchName $updateBranchName -Path $RelativitySourceCodePath
	if(!$SkipPullRequest)
	{
		.\Bitbucket\Create-PullRequest.ps1 -Credential $Credential -FromBranch $updateBranchName -ToBranch $OnBranch -Reviewers $PRreviewers -Project REL -Repository Relativity -Title "$JiraNumber $commitMessage"
	}
	if($changeIssueStatus -eq $true)
	{
		.\Jira\Change-IssueStatus.ps1 -Credential $Credential -JiraNumber $JiraNumber -StatusName Review
	}
	.\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RelativitySourceCodePath
	.\Git\Pop-StashIfExistsOnTop.ps1 -StashName $stashName -Path $RelativitySourceCodePath

	Write-Host "RIP version in Relativity updated successfully to $ToVersion!" -ForegroundColor Green
	
	Write-Verbose "End of Process block in Update-RipInRelativity.ps1"
}
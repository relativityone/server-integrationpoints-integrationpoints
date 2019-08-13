<#
.SYNOPSIS
	Updates Relativity packages in RIP

.DESCRIPTION
	Execute it from whatever branch you want without need to commit/stash your current changes.
	This script stashes all your changes and restores them when the update is finished.
	By default it will create a PR and put all BVCC members on it.

.PARAMETER ToVersion
	Version of Relativity to be updated

.PARAMETER OnBranch
	Branch name that needs to be updated

.PARAMETER RipSourceCodePath
	Path to Rip's source code

.PARAMETER JiraNumber
	JIRA ticket number that update refers to

.PARAMETER SkipStashing
	Skips stashing not commited changes

.PARAMETER SkipPullRequest
	Skips creating a PR to $OnBranch

.PARAMETER Credential
	Domain user credentials

.EXAMPLE
	.\UpdateRelativityInRip.ps1 -ToVersion 10.3.90.11-DEV -OnBranch develop -RipSourceCodePath S:\integrationpoints -JiraNumber REL-331932

.EXAMPLE
	.\UpdateRelativityInRip.ps1 -OnBranch develop -RipSourceCodePath S:\integrationpoints
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$False)]
	[string]$ToVersion,
	[Parameter(Mandatory=$True)]
	[string]$OnBranch,
	[Parameter(Mandatory=$True)]
	[string]$RipSourceCodePath,
	[Parameter(Mandatory=$False)]
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
	. ".\Utils.ps1"

	Write-Verbose "Beginning of Begin block in UpdateRelativityInRip.ps1"

	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

	if(!$Credential)
	{
		$Credential = Get-Credential
	}

	if(!$ToVersion)
	{
		$buildVersion = .\Jenkins\GetLastSuccessfulBuildVersion.ps1 -Credential $Credential -Category Relativity -Pipeline RelativityBuild -Branch $OnBranch
		$ToVersion = MapJenkinsBuildVersionToRelativityPackageVersion -BuildVersion $buildVersion
	}

	$commitMessage = "Relativity packages updated to $ToVersion on $OnBranch"

	$updatePullRequestExists = .\Bitbucket\CheckIfPullRequestAlreadyExists.ps1 -Credential $Credential -Title $commitMessage -Author $JiraAssignee -Project "IN" -Repository IntegrationPoints
	if($updatePullRequestExists -eq $true)
	{
		Write-Host "Pull request containing updated Relativity packages to $ToVersion on $OnBranch is already created!" -ForegroundColor Cyan
		break
	}

	$arePackagesUpToDate = .\PackageUpdate\AreRelativityPackagesInRipUpToDate.ps1 -NewVersion $ToVersion -OnBranch $OnBranch -RipSourceCodePath $RipSourceCodePath
	if($arePackagesUpToDate -eq $true)
	{
		Write-Host "Relativity packages in RIP - $ToVersion - is same or higher!" -ForegroundColor Cyan
		break
	}

	$changeIssueStatusEnabled = $false
	if(!$JiraNumber)
	{
		$changeIssueStatusEnabled = $true
		$currentDate = Get-Date -Uformat "%D"
		$parentJiraSummary = "Update RIP <-> Relativity on $OnBranch $currentDate"
		$parentJiraDescription = "$parentJiraSummary. $AutoPackageUpgradeAdnotation"
		$parentJiraKey = .\Jira\CreateJira.ps1 -Credential $Credential -Project REL -Summary $parentJiraSummary -Description $parentJiraDescription -Product "Data Transfer" -Feature "Integration Points" -Team $TeamName -IssueType Maintenance -Label $MainJiraLabel -Assignee $JiraAssignee
		$JiraNumber = .\Jira\CreateOrGetIfExistsJiraSubtask.ps1 -Credential $Credential -Project REL -ParentIssueKey $parentJiraKey -Summary "Update Relativity in RIP on $OnBranch" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RipUpdateJiraLabel -Assignee $JiraAssignee
		.\Jira\CreateOrGetIfExistsJiraSubtask.ps1 -Credential $Credential -Project REL -ParentIssueKey $parentJiraKey -Summary "Update RIP in Relativity on $OnBranch" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RelativityUpdateJiraLabel -Assignee $JiraAssignee
	}

	Write-Verbose "End of Begin block in UpdateRelativityInRip.ps1"
}
Process
{
	Write-Verbose "Beginning of Process block in UpdateRelativityInRip.ps1"

	$initialBranchName = .\Git\GetCurrentBranchName.ps1 -Path $RipSourceCodePath
	$updateBranchName = "$JiraNumber-update-relativity-in-rip"
	$stashName = "Saved changes before auto package upgrade"

	if(!$SkipStashing)
	{
		.\Git\Stash.ps1 -StashName $stashName -Path $RipSourceCodePath
	}
	if($changeIssueStatusEnabled -eq $true)
	{
		.\Jira\ChangeIssueStatus.ps1 -Credential $Credential -JiraNumber $JiraNumber -StatusName "In Progress"
	}
	.\Git\CreateBranch.ps1 -ParentBranch $OnBranch -BranchName $updateBranchName -Path $RipSourceCodePath
	$upgradeLogs = .\PackageUpdate\UpdateRelativityPackagesInRip.ps1 -ToVersion $ToVersion -RipSourceCodePath $RipSourceCodePath -WithLatestkCura -WithLatestRelativityApi -WithLatestRelativityDataExchange
	.\Git\Commit.ps1 -JiraNumber $JiraNumber -Message $commitMessage -Path $RipSourceCodePath
	.\Git\Push.ps1 -BranchName $updateBranchName -Path $RipSourceCodePath
	if(!$SkipPullRequest)
	{
		$pullRequestDescription = 'This PR was created automatically by AutoPackagesUpgrade script. \n\nPACKAGE UPGRADE LOGS:\n ```'+$upgradeLogs+'```'
		.\Bitbucket\CreatePullRequest.ps1 -Credential $Credential -FromBranch $updateBranchName -ToBranch $OnBranch -Reviewers $PRreviewers -Project "IN" -Repository IntegrationPoints -Title "$JiraNumber $commitMessage" -Description $pullRequestDescription
	}
	if($changeIssueStatusEnabled -eq $true)
	{
		.\Jira\ChangeIssueStatus.ps1 -Credential $Credential -JiraNumber $JiraNumber -StatusName Review
	}
	.\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RipSourceCodePath
	if(!$SkipStashing)
	{
		.\Git\PopStashIfExistsOnTop.ps1 -StashName $stashName -Path $RipSourceCodePath
	}

	Write-Host "Relativity version in RIP updated successfully to $ToVersion!" -ForegroundColor Green
	
	Write-Verbose "End of Process block in UpdateRelativityInRip.ps1"
}
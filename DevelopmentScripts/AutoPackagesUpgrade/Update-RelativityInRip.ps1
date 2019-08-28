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

.PARAMETER SkipPullRequest
	Skips creating a PR to $OnBranch

.PARAMETER Credential
	Domain user credentials

.EXAMPLE
	.\Update-RelativityInRip.ps1 -ToVersion 10.3.90.11-DEV -OnBranch develop -RipSourceCodePath S:\integrationpoints -JiraNumber REL-331932

.EXAMPLE
	.\Update-RelativityInRip.ps1 -OnBranch develop -RipSourceCodePath S:\integrationpoints
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
	[switch]$SkipPullRequest,
	[Parameter(Mandatory=$False)]
	[pscredential]$Credential
)
Begin
{
	. ".\Config.ps1" 
	. ".\Utils.ps1"

	Write-Verbose "Beginning of Begin block in Update-RelativityInRip.ps1"

	function Create-JiraWithSubtasks($Credential, $BranchName)
	{
		$currentDate = Get-Date -Uformat "%D"
		$parentJiraSummary = "Update RIP <-> Relativity on $BranchName $currentDate"
		$parentJiraDescription = "$parentJiraSummary. $AutoPackageUpgradeAdnotation"
		$parentJiraKey = .\Jira\Create-Jira.ps1 -Credential $Credential -Project REL -Summary $parentJiraSummary -Description $parentJiraDescription -Product "Data Transfer" -Feature "Integration Points" -Team $TeamName -IssueType Maintenance -Label $MainJiraLabel -Assignee $JiraAssignee
		$ripUpdateJiraNumber = .\Jira\Create-OrGetIfExistsJiraSubtask.ps1 -Credential $Credential -Project REL -ParentIssueKey $parentJiraKey -Summary "Update Relativity in RIP on $BranchName" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RipUpdateJiraLabel -Assignee $JiraAssignee
		$relativityUpdateJiraNumber = .\Jira\Create-OrGetIfExistsJiraSubtask.ps1 -Credential $Credential -Project REL -ParentIssueKey $parentJiraKey -Summary "Update RIP in Relativity on $BranchName" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RelativityUpdateJiraLabel -Assignee $JiraAssignee
		
		$ripUpdateJiraNumber
	}

	function Are-PackagesUpToDate($Credential, $ToPackageVersions, $OnBranch, $RipSourceCodePath)
	{
		Write-Host "Checking if packages are up to date."

		$initialBranchName = .\Git\Get-CurrentBranchName.ps1 -Path $RipSourceCodePath
		$stashName = "Saved changes before checking packages"
    	.\Git\Stash.ps1 -StashName $stashName -Path $RipSourceCodePath
		.\Git\Checkout.ps1 -BranchName $OnBranch -Path $RipSourceCodePath
		
		$areRipDependenciesOutOfDate = ($ToPackageVersions | Where-Object { (.\PackageUpdate\Is-PackageInRipUpToDate.ps1 -PackageName $_.name -NewVersion $_.version -RipSourceCodePath $RipSourceCodePath) }).Count -gt 0

		.\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RipSourceCodePath
		.\Git\Pop-StashIfExistsOnTop.ps1 -StashName $stashName -Path $RipSourceCodePath

		if($areRipDependenciesOutOfDate -eq $false)
		{
			Write-Host "Dependencies in RIP are same or higher! Upgrade not needed." -ForegroundColor Cyan
			$true
		}

		Write-Host "Packages are out of date. Updating..." -ForegroundColor Cyan

		$false 
	}

	function Get-PackageVersionsToBeUpdated($Credential, $ToRelativityVersion, $OnBranch)
	{
		try 
		{
			$packageVersions = @( 
				(New-Object PSObject -Property @{ name = "kCura"; version = .\Proget\Get-LatestPackageVersion.ps1 -PackageName "kCura" }),
				(New-Object PSObject -Property @{ name = "kCura.Agent"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "kCura.EventHandler"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "kCura.Relativity.Client"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.Authentication"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.CustomPages"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.Data"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.DataExchange.Client.SDK"; version = .\Proget\Get-LatestPackageVersion.ps1 -PackageName "Relativity.DataExchange.Client.SDK" }),
				(New-Object PSObject -Property @{ name = "Relativity.Services.Interfaces"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.Services.Interfaces.Private"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.Services.DataContracts"; version = $ToRelativityVersion }),
				(New-Object PSObject -Property @{ name = "Relativity.Sync"; version = .\Jenkins\Get-LastSuccessfulBuildVersion.ps1 -Credential $Credential -Category DataTransfer -SubCategory RelativitySync -Pipeline RelativitySync -Branch $OnBranch }),
				(New-Object PSObject -Property @{ name = "Relativity.API"; version = .\Proget\Get-LatestPackageVersion.ps1 -PackageName "Relativity.API" })
			)
		}
		catch 
		{
			Write-Error "Fetching package versions failed with $($_.Exception.Message)" -ErrorAction Stop
		}
		
		$packageVersions
	}

	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

	if(!$Credential)
	{
		$Credential = Get-Credential
	}

	if(!$ToVersion)
	{
		$buildVersion = .\Jenkins\Get-LastSuccessfulBuildVersion.ps1 -Credential $Credential -Category Relativity -Pipeline RelativityBuild -Branch $OnBranch
		$ToVersion = Map-JenkinsBuildVersionToRelativityPackageVersion -BuildVersion $buildVersion
	}

	$packagesVersions = Get-PackageVersionsToBeUpdated -Credential $Credential -ToRelativityVersion $ToVersion -OnBranch $OnBranch

	Write-Host "Going to upgrade RIP to the following package versions:" -ForegroundColor Cyan
	$packagesVersions | ForEach-Object { Write-Host $_.name, $_.version -ForegroundColor Cyan }

	$shouldUpdate = Are-PackagesUpToDate -Credential $Credential -ToPackageVersions $packagesVersions -OnBranch $OnBranch -RipSourceCodePath $RipSourceCodePath
	if($shouldUpdate -eq $false)
	{
		break
	}

	$changeIssueStatusEnabled = $false
	if(!$JiraNumber)
	{
		$changeIssueStatusEnabled = $true
		$JiraNumber = Create-JiraWithSubtasks -Credential $Credential -BranchName $OnBranch
	}

	Write-Verbose "End of Begin block in Update-RelativityInRip.ps1"
}
Process
{
	Write-Verbose "Beginning of Process block in Update-RelativityInRip.ps1"

	$updateBranchName = "$JiraNumber-update-relativity-in-rip"
	$stashName = "Saved changes before auto package upgrade"
	$commitMessage = "Relativity packages updated to $ToVersion on $OnBranch"
	$initialBranchName = .\Git\Get-CurrentBranchName.ps1 -Path $RipSourceCodePath

	.\Git\Stash.ps1 -StashName $stashName -Path $RipSourceCodePath
	if($changeIssueStatusEnabled -eq $true)
	{
		.\Jira\Change-IssueStatus.ps1 -Credential $Credential -JiraNumber $JiraNumber -StatusName "In Progress"
	}
	.\Git\Create-Branch.ps1 -ParentBranch $OnBranch -BranchName $updateBranchName -Path $RipSourceCodePath
	$upgradeLogs = .\PackageUpdate\Update-RelativityPackagesInRip.ps1 -ToPackagesVersions $packagesVersions -RipSourceCodePath $RipSourceCodePath
	.\Git\Commit.ps1 -JiraNumber $JiraNumber -Message $commitMessage -Path $RipSourceCodePath
	.\Git\Push.ps1 -BranchName $updateBranchName -Path $RipSourceCodePath
	if(!$SkipPullRequest)
	{
		.\Bitbucket\Create-PullRequest.ps1 -Credential $Credential -FromBranch $updateBranchName -ToBranch $OnBranch -Reviewers $PRreviewers -Project "IN" -Repository IntegrationPoints -Title "$JiraNumber $commitMessage"
	}
	if($changeIssueStatusEnabled -eq $true)
	{
		.\Jira\Change-IssueStatus.ps1 -Credential $Credential -JiraNumber $JiraNumber -StatusName Review
	}
	.\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RipSourceCodePath
	.\Git\Pop-StashIfExistsOnTop.ps1 -StashName $stashName -Path $RipSourceCodePath

	Write-Host "Relativity version in RIP updated successfully to $ToVersion!" -ForegroundColor Green
	
	Write-Verbose "End of Process block in Update-RelativityInRip.ps1"
}
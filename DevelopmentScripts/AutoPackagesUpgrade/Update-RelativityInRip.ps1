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
    Domain user's credentials

.EXAMPLE
    .\Update-RelativityInRip.ps1 -ToVersion 10.3.90.11-DEV -OnBranch develop -RipSourceCodePath S:\integrationpoints -JiraNumber REL-331932

.EXAMPLE
    .\Update-RelativityInRip.ps1 -OnBranch develop -RipSourceCodePath S:\integrationpoints
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
    $RipSourceCodePath,

    [Parameter(Mandatory=$False)]
    [string]
    $JiraNumber,

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
    Import-Module -Name $PSScriptRoot\Bitbucket\New-PullRequest
    Import-Module -Name $PSScriptRoot\Git\Find-CurrentBranchName
    Import-Module -Name $PSScriptRoot\Git\New-Branch
    Import-Module -Name $PSScriptRoot\Git\New-Checkout
    Import-Module -Name $PSScriptRoot\Git\New-Commit
    Import-Module -Name $PSScriptRoot\Git\New-Pull
    Import-Module -Name $PSScriptRoot\Git\New-Push
    Import-Module -Name $PSScriptRoot\Git\New-Stash
    Import-Module -Name $PSScriptRoot\Git\Pop-StashIfExistsOnTop
    Import-Module -Name $PSScriptRoot\Jenkins\Find-LastSuccessfulBuildVersion
    Import-Module -Name $PSScriptRoot\Jira\Find-OrCreateIfNotExistJiraSubtask
    Import-Module -Name $PSScriptRoot\Jira\New-Jira
    Import-Module -Name $PSScriptRoot\Jira\Update-JiraStatus
    Import-Module -Name $PSScriptRoot\PackageUpdate\Test-IsPackageInRipUpToDate
    Import-Module -Name $PSScriptRoot\PackageUpdate\Update-RelativityPackagesInRip
    Import-Module -Name $PSScriptRoot\Proget\Find-LatestPackageVersion

    Write-Host "Beginning of Begin block in $($MyInvocation.MyCommand.Name)"

    function New-JiraWithSubtasks($Credential, $BranchName)
    {
        $currentDate = Get-Date -Uformat "%D"
        $parentJiraSummary = "Update RIP <-> Relativity on $BranchName $currentDate"
        $parentJiraDescription = "$parentJiraSummary. $AutoPackageUpgradeAdnotation"
        $parentJiraKey = New-Jira -Credential $Credential -Project REL -Summary $parentJiraSummary -Description $parentJiraDescription -Product "Data Transfer" -Feature "Integration Points" -Team $TeamName -IssueType Maintenance -Label $MainJiraLabel -Assignee $JiraAssignee
        $ripUpdateJiraNumber = Find-OrCreateIfNotExistJiraSubtask -Credential $Credential -Project REL -ParentIssueKey $parentJiraKey -Summary "Update Relativity in RIP on $BranchName" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RipUpdateJiraLabel -Assignee $JiraAssignee
        $relativityUpdateJiraNumber = Find-OrCreateIfNotExistJiraSubtask -Credential $Credential -Project REL -ParentIssueKey $parentJiraKey -Summary "Update RIP in Relativity on $BranchName" -Description $AutoPackageUpgradeAdnotation -IssueType DEV -Label $RelativityUpdateJiraLabel -Assignee $JiraAssignee
        
        $ripUpdateJiraNumber
    }

    function Test-ArePackagesUpToDate($Credential, $ToPackageVersions, $OnBranch, $RipSourceCodePath)
    {
        Write-Host "Checking if packages are up to date..."

        $initialBranchName = Find-CurrentBranchName -Path $RipSourceCodePath
        $stashName = "Saved changes before checking packages"
        New-Stash -StashName $stashName -Path $RipSourceCodePath
        New-Checkout -BranchName $OnBranch -Path $RipSourceCodePath
        New-Pull -Path $RipSourceCodePath
        
        $areRipDependenciesOutOfDate = ($ToPackageVersions | Where-Object { (Test-IsPackageInRipUpToDate -PackageName $_.name -NewVersion $_.version -RipSourceCodePath $RipSourceCodePath) }).Count -gt 0

        New-Checkout -BranchName $initialBranchName -Path $RipSourceCodePath
        Pop-StashIfExistsOnTop -StashName $stashName -Path $RipSourceCodePath

        if($areRipDependenciesOutOfDate -eq $false)
        {
            Write-Host "Dependencies in RIP are same or higher! Upgrade not needed." -ForegroundColor Cyan
            $true
        }

        Write-Host "Packages are out of date. Updating..." -ForegroundColor Cyan

        $false 
    }

    function Find-PackageVersionsToBeUpdated($Credential, $ToRelativityVersion, $OnBranch)
    {
        try 
        {
            $packageVersions = @( 
                (New-Object PSObject -Property @{ name = "kCura"; version = Find-LatestPackageVersion -PackageName "kCura" }),
                (New-Object PSObject -Property @{ name = "kCura.Agent"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "kCura.EventHandler"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "kCura.Relativity.Client"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.Authentication"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.CustomPages"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.Data"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.DataExchange.Client.SDK"; version = Find-LatestPackageVersion -PackageName "Relativity.DataExchange.Client.SDK" }),
                (New-Object PSObject -Property @{ name = "Relativity.Services.Interfaces"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.Services.Interfaces.Private"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.Services.DataContracts"; version = $ToRelativityVersion }),
                (New-Object PSObject -Property @{ name = "Relativity.Sync"; version = Find-LastSuccessfulBuildVersion -Credential $Credential -Category DataTransfer -SubCategory RelativitySync -Pipeline RelativitySync -Branch $OnBranch }),
                (New-Object PSObject -Property @{ name = "Relativity.API"; version = Find-LatestPackageVersion -PackageName "Relativity.API" })
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
        $buildVersion = Find-LastSuccessfulBuildVersion -Credential $Credential -Category Relativity -Pipeline RelativityBuild -Branch $OnBranch
        $ToVersion = Format-JenkinsBuildVersionToRelativityPackageVersion -BuildVersion $buildVersion
    }

    $packagesVersions = Find-PackageVersionsToBeUpdated -Credential $Credential -ToRelativityVersion $ToVersion -OnBranch $OnBranch

    Write-Host "Going to upgrade RIP to the following package versions:" -ForegroundColor Cyan
    $packagesVersions | ForEach-Object { Write-Host $_.name, $_.version -ForegroundColor Cyan }

    $shouldUpdate = Test-ArePackagesUpToDate -Credential $Credential -ToPackageVersions $packagesVersions -OnBranch $OnBranch -RipSourceCodePath $RipSourceCodePath
    if($shouldUpdate -eq $false)
    {
        break
    }

    $changeIssueStatusEnabled = $false
    if(!$JiraNumber)
    {
        $changeIssueStatusEnabled = $true
        $JiraNumber = New-JiraWithSubtasks -Credential $Credential -BranchName $OnBranch
    }

    Write-Host "End of Begin block in $($MyInvocation.MyCommand.Name)"
}
Process
{
    Write-Host "Beginning of Process block in $($MyInvocation.MyCommand.Name)"

    $updateBranchName = "$JiraNumber-update-relativity-in-rip"
    $stashName = "Saved changes before auto package upgrade"
    $commitMessage = "Relativity packages updated to $ToVersion on $OnBranch"
    $initialBranchName = Find-CurrentBranchName -Path $RipSourceCodePath

    New-Stash -StashName $stashName -Path $RipSourceCodePath
    if($changeIssueStatusEnabled -eq $true)
    {
        Update-JiraStatus -Credential $Credential -JiraNumber $JiraNumber -StatusName "In Progress"
    }
    New-Branch -ParentBranch $OnBranch -BranchName $updateBranchName -Path $RipSourceCodePath
    $upgradeLogs = Update-RelativityPackagesInRip -ToPackagesVersions $packagesVersions -RipSourceCodePath $RipSourceCodePath
    New-Commit -JiraNumber $JiraNumber -Message $commitMessage -Path $RipSourceCodePath
    New-Push -BranchName $updateBranchName -Path $RipSourceCodePath
    if(!$SkipPullRequest)
    {
        New-PullRequest -Credential $Credential -FromBranch $updateBranchName -ToBranch $OnBranch -Reviewers $PRreviewers -Project "IN" -Repository IntegrationPoints -Title "$JiraNumber $commitMessage"
    }
    if($changeIssueStatusEnabled -eq $true)
    {
        Update-JiraStatus -Credential $Credential -JiraNumber $JiraNumber -StatusName Review
    }
    New-Checkout -BranchName $initialBranchName -Path $RipSourceCodePath
    Pop-StashIfExistsOnTop -StashName $stashName -Path $RipSourceCodePath

    Write-Host "Relativity version in RIP updated successfully to $ToVersion!" -ForegroundColor Green
    
    Write-Host "End of Process block in $($MyInvocation.MyCommand.Name)"
}
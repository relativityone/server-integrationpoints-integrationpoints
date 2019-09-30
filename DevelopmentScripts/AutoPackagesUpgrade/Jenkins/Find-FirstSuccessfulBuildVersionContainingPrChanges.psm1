<#  
.SYNOPSIS  
    Finds first successful build version which contains specified pull request's changes for specified pipeline

.PARAMETER Credential
    Domain user's credentials

.PARAMETER JiraKey
    Related jira key

.PARAMETER Category
    Jenkin's build category name

.PARAMETER Pipeline
    Jenkin's build pipeline name

.PARAMETER Branch
    Jenkin's build branch name
#>

function Find-FirstSuccessfulBuildVersionContainingPrChanges
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]
        [string]
        $JiraKey,

        [Parameter(Mandatory=$True)]
        [string]
        $Category,

        [Parameter(Mandatory=$True)]
        [string]
        $Pipeline,

        [Parameter(Mandatory=$True)]
        [string]
        $Branch
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils
   
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"
    
    $apiUriSegment = "api/json"
    $getJobUri = "$JenkinsApiUri/job/$Category/job/$Pipeline/job/$Branch/$apiUriSegment"
  
    Write-Host $getJobUri
    
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {
        $response = Invoke-RestMethod -Uri $getJobUri -Method GET -Headers $headers -UseBasicParsing

        $buildsNumberAndUri = $response.builds | Select-Object -Property number,url | ForEach-Object { $_.url += $apiUriSegment; $_ }

        $buildsUri = $buildsNumberAndUri | Select-Object -ExpandProperty url
        $buildContainingPrChanges = Find-FirstBuildContainingPrChanges -JiraKey $JiraKey -Branch $Branch -BuildsUri $buildsUri -Headers $headers

        $firstSuccessfulBuildContainingPrChanges = Find-FirstSuccessfulBuildAfter -Build $buildContainingPrChanges -BuildsNumberAndUri $buildsNumberAndUri -Headers $headers

        $version = $firstSuccessfulBuildContainingPrChanges.displayName
    }  
    catch 
    {  
        Exit-AndLogHttpError -CmdName $MyInvocation.MyCommand.Name
    }  
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $version
}

function Find-FirstBuildContainingPrChanges($JiraKey, $Branch, $BuildsUri, $Headers)
{
    $commitMessageRegex = "(?s:.)*$JiraKey(?s:.)*$Branch"
    foreach($buildUri in $BuildsUri)
    {
        Write-Host $buildUri
        $build = Invoke-RestMethod -Uri $buildUri -Method GET -Headers $Headers -UseBasicParsing
        $buildChangeSetCommitMessages = $build.changeSets | Select-Object -ExpandProperty items | Select-Object -ExpandProperty comment
        $containsPullRequest = ($buildChangeSetCommitMessages | Where-Object {$_ -match $commitMessageRegex}).Count -gt 0
        if($containsPullRequest -eq $true)
        {
            $buildContainingPullRequest = $build
            break
        }
    }
    if(!$buildContainingPullRequest)
    {
        throw "Cannot find build containing PR changes: $JiraKey to $Branch"
    }
    $buildContainingPullRequest
}

function Find-FirstSuccessfulBuildAfter($Build, $BuildsNumberAndUri, $Headers)
{
    if($Build.result -eq "SUCCESS")
    {
        $Build
    }

    $firstBuildIndex = ($BuildsNumberAndUri | Select-Object -ExpandProperty number).IndexOf($Build.number)
    $nextBuilds = $BuildsNumberAndUri[0..$firstBuildIndex]
    [array]::Reverse($nextBuilds)
    $nextSuccessfulBuilds = @()

    foreach($nextBuild in $nextBuilds)
    {
        $nextSuccessfulBuilds += Invoke-RestMethod -Uri $nextBuild.url -Method GET -Headers $Headers -UseBasicParsing
    }

    $nextSuccessfulBuilds = @($nextSuccessfulBuilds | Where-Object { $_.result -eq "SUCCESS" })
    
    if($nextSuccessfulBuilds.length -eq 0)
    {
        throw "There is no successful build containing specified PR changes: $JiraKey to $Branch"
    }
    $nextSuccessfulBuilds | Select-Object -First 1
}

Export-ModuleMember -Function Find-FirstSuccessfulBuildVersionContainingPrChanges
<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)]  
    [pscredential]$Credential,
    [Parameter(Mandatory=$True)]
    [string]$JiraKey,
	[Parameter(Mandatory=$True)]
	[string]$Category,
	[Parameter(Mandatory=$True)]
    [string]$Pipeline,
    [Parameter(Mandatory=$True)]
    [string]$Branch
)
Begin
{
    function Get-FirstBuildContainingPrChanges($JiraKey, $Branch, $BuildsUri, $Headers)
    {
        $commitMessageRegex = "Merge pull request(?s:.)*$JiraKey(?s:.)* to $Branch"

        foreach($buildUri in $BuildsUri)
        {
            Write-Verbose $buildUri
            $build = Invoke-RestMethod -Uri $buildUri -Method GET -Headers $Headers -UseBasicParsing
            $buildChangeSetCommitMessages = $build.changeSets | Select-Object -ExpandProperty items | Select-Object -ExpandProperty comment

            $containsPullRequest = ($buildChangeSetCommitMessages | Where-Object {$_ -match $commitMessageRegex}).Count -gt 0

            if($containsPullRequest -eq $true)
            {
                $build
            }
        }

        throw "Cannot find build containing PR changes: $JiraKey to $Branch"
    }

    function Get-FirstSuccessfulBuildAfter($Build, $BuildsNumberAndUri, $Headers)
    {
        if($Build.result -eq "SUCCESS")
        {
            $Build
        }

        $firstBuildIndex = ($BuildsNumberAndUri | Select-Object -ExpandProperty number).IndexOf($Build.number)
        $nextBuilds = [array]::Reverse($BuildsNumberAndUri[0..$firstBuildIndex])

        $nextSuccessfulBuilds = $nextBuilds | Where-Object { result -eq "SUCCESS" }
        
        if($nextSuccessfulBuilds.length -eq 0)
        {
            throw "There is no successful build containing specified PR changes: $JiraKey to $Branch"
        }

        $nextSuccessfulBuilds | Select-Object -First 1
    }

    . ".\Config.ps1"  
	. ".\Utils.ps1"  
}
Process
{    
    Write-Verbose "Beginning of Get-FirstSuccessfulBuildVersionContainingPrChanges.ps1"
    
    $apiUriSegment = "api/json"
	$getJobUri = "$JenkinsApiUri/job/$Category/job/$Pipeline/job/$Branch/$apiUriSegment"
  
    Write-Verbose $getJobUri
    
    $headers = Get-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {
        $response = Invoke-RestMethod -Uri $getJobUri -Method GET -Headers $headers -UseBasicParsing

        $buildsNumberAndUri = $response.builds | Select-Object -Property number,url | ForEach-Object { $_.url += $apiUriSegment; $_ }

        $buildsUri = $buildsNumberAndUri | Select-Object -ExpandProperty url
        $buildContainingPrChanges = Get-FirstBuildContainingPrChanges -JiraKey $JiraKey -Branch $Branch -BuildsUri $buildsUri -Headers $headers

        $firstSuccessfulBuildContainingPrChanges = Get-FirstSuccessfulBuildAfter -Build $buildContainingPrChanges -BuildsNumberAndUri $buildsNumberAndUri -Headers $headers

        $version = $firstSuccessfulBuildContainingPrChanges.displayName
	}  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
        Write-Error "Get-FirstSuccessfulBuildVersionContainingPrChanges failed" -ErrorAction Stop 
    }  
    
    Write-Verbose "End of Get-FirstSuccessfulBuildVersionContainingPrChanges.ps1"

    $version
}


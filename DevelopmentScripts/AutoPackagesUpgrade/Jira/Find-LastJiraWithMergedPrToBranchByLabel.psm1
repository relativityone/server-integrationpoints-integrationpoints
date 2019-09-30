<#  
.SYNOPSIS  
    Finds JIRA object with last merged pull request's changes to specified branch by JIRA label 

.PARAMETER Credential
    Domain user's credentials

.PARAMETER LabelName
    JIRA's label name

.PARAMETER BranchName
    JIRA's branch name
#>
  
function Find-LastJiraWithMergedPrToBranchByLabel
{
    [CmdletBinding()]
    Param(  
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]  
        [string]
        $LabelName,

        [Parameter(Mandatory=$True)]  
        [string]
        $BranchName
    )  
    
    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils
  
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"  
    
    $jiraUri = "$JiraApiUri/api/2/search?jql=labels=$LabelName+AND+summary~$BranchName+order+by+created&maxResults=30&fields=key,customfield_15000,parent"
  
    Write-Host $jiraUri  
  
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  
        $response = Invoke-RestMethod -Uri $jiraUri -Method GET -Headers $headers -UseBasicParsing
    }  
    catch 
    {  
        Exit-AndLogHttpError -CmdName $MyInvocation.MyCommand.Name 
    }  
    
    if(!$response.issues -or $response.issues.Length -eq 0)
    {
        $issue = $null
    }
    else
    {
        try
        {
            $issue = Find-JiraWithLastMergedPullRequest -Issues $response.issues
        }
        catch
        {
            Write-Error "Finding last merged PR failed in $($MyInvocation.MyCommand.Name) with $($_.Exception.Message)" -ErrorAction Stop 
        }
    }

    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $issue
}

function Find-JiraWithLastMergedPullRequest($Issues)
{
    foreach($issue in $Issues)
    {
        $summaryField = $issue.fields.customfield_15000
        $jsonString = ($summaryField -split "devSummaryJson=")[-1]
        $json = $jsonString.Substring(0, $jsonString.Length-1) | ConvertFrom-Json
        $pullRequestState = $json.cachedValue.summary.pullrequest.overall.state
        if($pullRequestState -eq "MERGED")
        {
            $issueWithLastMergedPullRequest = $issue
            break
        }
    }
    $issueWithLastMergedPullRequest
}

Export-ModuleMember -Function Find-LastJiraWithMergedPrToBranchByLabel
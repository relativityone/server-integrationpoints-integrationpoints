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
    [string]$LabelName,
    [Parameter(Mandatory=$True)]  
    [string]$BranchName
)  
Begin
{
    function Get-IssueWithLastMergedPullRequest($Issues)
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

	. ".\Config.ps1"  
	. ".\Utils.ps1"
}
Process  
{  
	Write-Verbose "Beginning of Get-LastIssueWithMergedPrToBranchByLabel.ps1"  
	
	$jiraUri = "$JiraApiUri/api/2/search?jql=labels=$LabelName+AND+summary~$BranchName+order+by+created&maxResults=30&fields=key,customfield_15000,parent"
  
	Write-Verbose $jiraUri  
  
	$headers = Get-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  
        $response = Invoke-RestMethod -Uri $jiraUri -Method GET -Headers $headers -UseBasicParsing
        $response.issues.Length
    }  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
        Write-Error "Get-LastIssueWithMergedPrToBranchByLabel failed" -ErrorAction Stop 
	}  
    
    if(!$response.issues -or $response.issues.Length -eq 0)
    {
        $issue = $null
    }
    else
    {
        try
        {
            $issue = Get-IssueWithLastMergedPullRequest -Issues $response.issues
        }
        catch
        {
            Write-Error "Finding last merged PR failed in Get-LastIssueWithMergedPrToBranchByLabel with $($_.Exception.Message)" -ErrorAction Stop 
        }
    }

    Write-Verbose "End of Get-LastIssueWithMergedPrToBranchByLabel.ps1"

    $issue
}
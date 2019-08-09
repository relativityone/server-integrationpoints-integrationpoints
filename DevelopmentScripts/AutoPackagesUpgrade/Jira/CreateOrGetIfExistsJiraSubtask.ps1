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
	[string]$Summary, 
	[Parameter(Mandatory=$True)]  
    [string]$Project,
    [Parameter(Mandatory=$True)]
	[string]$ParentIssueKey,  
	[Parameter(Mandatory=$True)]  
	[string]$Description,
	[Parameter(Mandatory=$True)]  
	[string]$IssueType,
	[Parameter(Mandatory=$False)]
	[string]$Label,
	[Parameter(Mandatory=$False)]
	[string]$Assignee
)  
Begin
{
	. ".\Config.ps1"  
	. ".\Utils.ps1" 

	if($Label)
	{
		$Label = '"'+$Label+'"'
	}
}
Process  
{    
	Write-Verbose "Beginning of CreateOrGetIfExistsJiraSubtask.ps1"  

	$createJiraUri = "$JiraApiUri/api/2/issue/"
	$getJiraUri = "$JiraApiUri/api/2/search?jql=parent=$ParentIssueKey+AND+summary~$Summary&fields=key"
  
	Write-Verbose $jiraUri

	[String] $body = '{  
		"fields":{  
		   "project":{  
			  "key":"'+$Project+'"
		   },
		   "parent":{
			"key":"'+$ParentIssueKey+'"
		    },
		   "issuetype":{  
			  "name":"'+$IssueType+'"
		   },
		   "summary":"'+$Summary+'",
		   "description":"'+$Description+'",
		   "priority":{  
			  "name":"P3"
		   },
		   "labels": [
			   '+$Label+'
		   ],
		   "assignee": { "name": "'+$Assignee+'" }
		}
	 }'
	
	Write-Verbose $body  
  
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential
	try 
	{  	 
		$response = Invoke-RestMethod -Uri $getJiraUri -Method GET -Headers $headers -Body $body -UseBasicParsing
		
		if(!$response.key)
		{
			$response = Invoke-RestMethod -Uri $createJiraUri -Method POST -Headers $headers -Body $body -UseBasicParsing
		}    
		$jiraKey = $($response.key)
	}  
	catch 
	{  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
		Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
		Write-Error "Creating Jira Subtask failed" -ErrorAction Stop
	}  
	Write-Verbose "End of CreateOrGetIfExistsJiraSubtask.ps1"  
	return $jiraKey  
}
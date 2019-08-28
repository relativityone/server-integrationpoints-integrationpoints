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
	Write-Verbose "Beginning of Create-OrGetIfExistsJiraSubtask.ps1"  

	$createJiraUri = "$JiraApiUri/api/2/issue/"
	$getJiraUri = "$JiraApiUri/api/2/search?jql=parent=$ParentIssueKey+AND+labels=$Label&fields=key"
  
	Write-Verbose $createJiraUri
	Write-Verbose $getJiraUri

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
  
	$headers = Get-BasicAuthJsonHttpHeaders -Credential $Credential
	try 
	{  	 
		$response = Invoke-RestMethod -Uri $getJiraUri -Method GET -Headers $headers -UseBasicParsing
		
		if($response.issues -and $response.issues.length -gt 0)
		{
			$response = $response.issues[0]
		}
		else
		{
			Write-Verbose "Subtask does not exist - attempting to create a new one"
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

	Write-Verbose "End of Create-OrGetIfExistsJiraSubtask.ps1"  

	$jiraKey  
}